# DESIGN: AI Strategy (ISMCTS First Agent)

**Last Updated:** August 17, 2026

---

## Table of Contents

- [1. Overview](#1-overview)
- [2. Why ISMCTS](#2-why-ismcts)
- [3. Control-Flow Model](#3-control-flow-model)
- [4. Node and Tree Data Model](#4-node-and-tree-data-model)
- [5. MoveKey and ActionKey](#5-movekey-and-actionkey)
- [6. The Four-Phase Loop](#6-the-four-phase-loop)
- [7. Combat Node Model](#7-combat-node-model)
- [8. Rollout Turn-Cap and Scoring](#8-rollout-turn-cap-and-scoring)
- [9. Self-Play Tuning Harness](#9-self-play-tuning-harness)
- [10. Prerequisites and Dependencies](#10-prerequisites-and-dependencies)
- [11. Known Limitations (Deferred to Phase 9)](#11-known-limitations-deferred-to-phase-9)

---

## 1. Overview

Phase 7 stands up the first real agent: an Information Set Monte Carlo Tree Search
(ISMCTS) strategy that plays legal games on the current pool and beats
`RandomStrategy`. It sits on the Phase 6 substrate (master seed + split RNG,
masked deep-clone seam) and reuses the existing engine as its forward model.

The agent is a new `IStrategy` implementation. When the engine calls one of its
decision callbacks, the strategy *suspends the live game at that callback* and
runs thousands of independent cloned games to decide, then returns one concrete
decision. Re-entrancy is safe because clones share immutable cards by reference,
deep-copy mutable state, and draw only from derived RNG streams (TASK_0601/0602).

This document is the Phase 7 counterpart to `BACKLOG/DESIGN_AI_AdvancedAgent.md`,
which covers the later WorldModel/Hybrid agents (Phase 9). The AlphaZero
`ISearchableGameState`/`IStateEvaluator` framing there is superseded for Phase 7
by the concrete design below, which binds directly to the shipped engine
contracts (`IStrategy`, `ITabletop`, `IJudicialAssistant`, `GameJudge`).

## 2. Why ISMCTS

Vanilla MCTS assumes perfect information and determinism; MTG violates both
(hidden opponent hand/library; shuffles and draws). ISMCTS patches this with
**determinization**: each search iteration samples one concrete world consistent
with public information, runs a normal MCTS iteration on it, and shares the tree
across determinizations keyed by *moves* rather than exact states.

ISMCTS is chosen over MCCFR because MTG's branch factor (2^N attacks, N×M blocks,
priority at every step) and non-abstractable information sets make CFR
convergence impractical at this stage.

## 3. Control-Flow Model

The engine is inversion-of-control: `GameJudge` drives and calls *into*
`IStrategy` at decision points (`PerformPrioritizedAction`,
`PerformNonPrioritizedAction`, `DeclareAttacker`, `DeclareBlocker`,
`PerformRequiredAction`). The search therefore runs the engine forward on clones
rather than owning a pure step function.

- **Rollout** = clone the tabletop, install `RandomStrategy` (seeded from a
  derived stream) into both player strategy slots, loop `GameJudge.ExecuteNextTurn`
  until `ExecutionResult.IsTerminal`.
- **Selection / expansion** = a `PlanningStrategy : IStrategy` follows the tree
  policy on each callback during a cloned run and records the path; the opponent
  uses `RandomStrategy`. On reaching an unexpanded node, both slots flip to
  `RandomStrategy` and the run continues to terminal ("record-then-rollout").

The move generator already exists: `IJudicialAssistant.FindLegalActions(tabletop, modifier)`.
The transition function already exists: `IActionJudge.QueueAction / ExecuteAction`
inside `GameJudge`.

## 4. Node and Tree Data Model

Single-observer ISMCTS: only the root (searching) player's decision points are
tree nodes; the opponent and all chance are absorbed into the environment
transition. All node values are from the root player's perspective, so there is
**no sign-flipping**.

```csharp
public sealed class MctsNode
{
    public MctsNode? Parent { get; init; }
    public MoveKey? IncomingMove { get; init; }        // null at root
    public Dictionary<MoveKey, MctsNode> Children { get; } = new();

    public int    VisitCount     { get; set; }         // N(a)
    public double TotalValue     { get; set; }         // W(a), root-player perspective
    public int    AvailableCount { get; set; }         // ISMCTS: iterations where this edge was legal

    public double MeanValue => this.VisitCount == 0 ? 0.0 : this.TotalValue / this.VisitCount;
}
```

`AvailableCount` (not parent visits) is the ISMCTS correction: legal move sets
differ per determinization, so the exploration term divides by the number of
iterations in which the edge was actually available.

**UCB (availability-aware):**

```
UCB(a) = W(a)/N(a)  +  C * sqrt( ln( Available(a) ) / N(a) )
```

- Values are normalized to [0,1] (win = 1, loss = 0), so `C ≈ 0.7–1.0` (config), not √2.
- **Final move = robust child** (max `VisitCount`), not max value — more stable under noisy rollouts.

## 5. MoveKey and ActionKey

The tree keys children by `IAction` for priority *and* by combat subsets, so one
umbrella `MoveKey` unifies both; all variants collapse to a canonical string.

```csharp
public enum MoveCategory { Action, Attack, Block }
public readonly record struct MoveKey(MoveCategory Category, string Canonical);
```

`IAction` has no value-equality today and should NOT be made `IEquatable` (it is
a mutable entity — value-equality on a dictionary key is a footgun). Instead
project it to a reference-identity-free canonical string:

```csharp
public readonly record struct ActionKey
{
    public string Canonical { get; }              // sole equality/hash member
    public static ActionKey From(IAction a) => new(Canonicalize(a));
    // $"{Kind}|{OwningPlayer.Kind}|{subjectOracle}|{targetCanonical}|{parameterCanonical}"
}
```

**Subject-vs-target identity distinction:**
- A card cast/played from hand is interchangeable with its duplicate → key the
  subject by **oracle identity** (Name / oracle id), so two Forests collapse to
  one edge. On apply, bind to any concrete instance matching the key.
- A permanent on the battlefield (target, or mana-ability source) is a distinct
  game object → key it by **instance `Id`**.

Combat edge canonicals:
- Attack → sorted attacker permanent `Id`s.
- Block → sorted `(attackerId → sorted blockerIds)`.

**Footguns:** `ImmutableArray`/collections in a `record struct` do NOT give
structural equality — fold everything into the single sorted `Canonical` string.
`IParameter` is not enumerable (only `FindValue<T>(key)`); today only
`ParameterKey.Amount` exists, so probe known keys, and extend the canonicalizer
when a new key lands.

## 6. The Four-Phase Loop

Per iteration, determinize at the root (masked clone — TASK_0602), then:

1. **Selection** — from a node, among children legal in the current
   determinization, pick max UCB; descend until an untried-but-legal move exists.
2. **Expansion** — apply one untried legal move via cloned `GameJudge`, advance
   to the next root-player decision point, create the child keyed by `MoveKey`.
3. **Simulation** — both strategy slots = `RandomStrategy` (derived RNG), loop
   `ExecuteNextTurn` to terminal or the turn-cap.
4. **Backprop** — walk `Parent` up: `N++`, `W += value`; bump `AvailableCount`
   for every edge legal in this determinization at each visited node.

Determinization is a property of the clone, not the search — the MCTS code never
touches hidden-info logic.

## 7. Combat Node Model

Single-observer asymmetry resolves combat cleanly:
- Root is active player → `DeclareAttacker` is a tree node; opponent block is sampled.
- Root is defending player → `DeclareBlocker` is a tree node; attack is fixed upstream.

**Attack subsets** (`n` = legal attackers, from `IAttackingDecision.AttackingPermanents`, a flat set):
- `n ≤ 5`: enumerate the full power set — each subset one child.
- `n > 5`: progressive widening, cap children at `k = ⌈C·N^α⌉` (α ≈ 0.5); seed
  attack-all, attack-none, plus heuristic subsets and random fills.

**Block assignments** (`IBlockingDecision.Combats`, per-attacker ordered blocker
lists): space is ≈ `(|X|+1)^|B|` — never enumerate. Progressive widening + sane
candidate generators: no-blocks, block-to-kill, chump-the-lethal, random legal.

**Damage-order simplification:** the engine spills damage in `BlockingPermanents`
list order. Canonicalize multi-blocks by sorted blocker `Id` and feed that order
back into the `Combat` (deterministic). Defer "attacker chooses damage order" to
the instants phase.

**High-leverage recommendation:** because combat is the win condition in the
vanilla pool, let the *rollout* opponent block via the block-to-kill /
chump-lethal heuristic rather than pure random. Far cheaper than opponent trees,
much better attack quality. Full min/max opponent combat nodes are Phase 9.

## 8. Rollout Turn-Cap and Scoring

Decking guarantees termination but at ~2× library size in turns; the cap bounds
cost and also lowers variance. `ExecutionResult` has no draw type.

```csharp
double Rollout(ITabletop clone, IPlayer root, MctsOptions opt)
{
    var startTurn = clone.TurnId; var guard = 0;
    while (true)
    {
        var r = this._judge.ExecuteNextTurn(clone);
        if (r.HasError) return 0.5;                                        // spoiled — count it
        if (r.WinningPlayer.Kind != PlayerKind.None)
            return r.WinningPlayer.Kind == root.Kind ? 1.0 : 0.0;
        if (clone.TurnId - startTurn >= opt.TurnCap || ++guard >= opt.MaxTurnGuard)
            return Evaluate(clone, root, opt);
    }
}
```

**Scoring regimes and the ordering that must hold:** real win = 1.0 >
strongly-ahead-at-cap ≈ 0.95 > … heuristic in (0.05, 0.95) … > strongly-behind
≈ 0.05 > real loss = 0.0; spoiled (HasError) = 0.5. The heuristic MUST clamp
strictly inside (0,1) so true lethal always outranks "ahead at the cap".

```
raw = WLife·(root.Life - opp.Life) + WBoard·(boardScore_root - boardScore_opp) + WHand·ΔHand
v   = logistic(raw / LogisticScale);  return clamp(v, 0.05, 0.95)
boardScore(p) = Σ over p's creatures of (power + toughness)
```

**Starting knobs (`MctsOptions`, all config):** `TurnCap = 24`,
`MaxTurnGuard = 2×TurnCap`, `WLife = 1.0`, `WBoard = 0.7`, `WHand = 0.3`,
`LogisticScale ≈ 12`. Terminal is checked before the cap, so a boundary lethal
scores 1.0. The spoiled-rollout counter is a bring-up diagnostic: a nonzero
fraction means a clone/engine bug, not a tuning issue.

## 9. Self-Play Tuning Harness

You cannot tune against a mirror (~50% by construction). Two modes:
**absolute** (candidate vs a fixed version-pinned anchor — `RandomStrategy`
first, then a frozen good MCTS once candidates crush random) and **relative**
(round-robin / Elo among survivors).

Variance controls, in priority order:
1. **Paired (antithetic) seeds + side-swap** — for each master seed, play both
   orientations (A on the play / A on the draw); score A over the pair. Cancels
   first-player advantage and most shuffle luck. Ideal form derives each deck's
   shuffle stream from `hash(master, deckId)` (a small constraint on TASK_0601).
2. **Fixed decklist set** (aggro / midrange / defensive); report per-deck and aggregate.
3. **Hold search budget (`N_iter`) constant** while tuning eval params.

Identifiability: weights and scale are jointly non-identifiable — **pin `WLife = 1.0`**
and tune `{WBoard, WHand, LogisticScale, TurnCap}`. Expect a TurnCap×weights
interaction (high cap → weights barely matter); tune jointly.

Statistics: report **Wilson intervals**; ~1,000 unpaired games to resolve 55% vs
50% (paired cuts this). Promote a challenger only past the eval-noise band on
**held-out** seeds/decks. Efficiency upgrades: **SPRT** sequential testing +
**SPSA** optimizer (the chess-engine-tuning stack) once coarse search narrows the region.

Staged plan: (1) sanity MCTS(default) vs Random ≫50% — validates the harness and
cross-checks the spoiled counter; (2) coarse random search vs anchor, keep top-k;
(3) round-robin top-k → Elo → champion; (4) validate on held-out seeds/decks;
(5) lock θ, re-tune when the pool grows (weights are pool-dependent).

Harness runs `GameJudge` headless with `NoopObserver`, one independent RNG
factory per worker (games are embarrassingly parallel; shared immutable card DB
is read-only-safe), emitting one JSONL row per game
`{seed, deckA, deckB, side, winner, turns, spoiledRollouts, θ}` for offline analysis.

This is a concrete instance of the Self-Improving Champion Loop: hold champion θ,
one perturbation per SPSA step, promote only past the noise band on a holdout;
budget for rejected challengers is expected, not waste.

## 10. Prerequisites and Dependencies

- **Hard dependency:** Phase 6 — TASK_0601 (master seed + split RNG streams) and
  TASK_0602 (masked deep-clone seam). Id-stable clone is a hard acceptance
  criterion: `ActionKey` uses permanent instance `Id`s for board subjects/targets,
  so the clone MUST preserve `ICard.Id` and `IPermanent.Id` or the tree never
  accumulates visits.
- **Contract change:** promote the existing unused `ISource` interface onto
  `IAction` (`ISource Source { get; }`) so `ActionKey` can identify "which card /
  which permanent" unambiguously. Fallback (derive subject from `Target`) is fragile.

## 11. Known Limitations (Deferred to Phase 9)

- **Single-observer** evaluates against a random-rollout opponent → overvalues
  lines that only work if the opponent flails. Upgrade: opponent decision nodes
  (multi-tree ISMCTS / paranoid). The combat heuristic-block (Section 7) is the
  cheap partial mitigation shipped in Phase 7.
- **Static logistic eval** → replace with a learned value function (the WorldModel
  agent) once the event trace (Phase 8) exists as training data.
- **Deep clone per node/rollout** is the correctness baseline; copy-on-write /
  journal-undo perf tuning is deferred until profiling shows it as the bottleneck.
