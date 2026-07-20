# DESIGN: AI Strategy Architecture

**Last Updated:** 2026-06-27

---

## Table of Contents

- [1. Overview](#1-overview)
- [2. Strategy Landscape](#2-strategy-landscape)
- [3. Interface Design](#3-interface-design)
- [4. State Encoding](#4-state-encoding)
- [5. Output Format](#5-output-format)
- [6. Training Pipeline](#6-training-pipeline)
- [7. Evaluation Harness](#7-evaluation-harness)
- [8. Dependencies](#8-dependencies)

---

## 1. Overview

Phase 4 layers AI strategy implementations on top of the rules-complete engine.
Architecture follows the AlphaZero pattern: pluggable neural network prior
combined with MCTS verification, trained via self-play.

Key insight: Qwen AgentWorld's training methodology (CPT→SFT→RL on interaction
traces) applies well as the agent layer, while the engine provides ground-truth
game mechanics.

## 2. Strategy Landscape

Implementation order (each builds on the previous):

1. `MCTSStrategy` — ISMCTS with determinization for hidden information
2. `WorldModelStrategy` — Standalone inference against a trained LLM
3. `HybridStrategy` — MCTS guided by world model priors (AlphaZero pattern)

ISMCTS chosen over MCCFR because MTG's branch factor (2^N attacks, N×M blocks,
priority at every step) and non-abstractable information sets make CFR
convergence impractical.

## 3. Interface Design

```csharp
public interface ISearchableGameState
{
    ISearchableGameState Clone();
    ISearchableGameState ApplyAction(IAction action);
    ISearchableGameState Determinize(PlayerId perspective, Random rng);
    PlayerId ActivePlayer { get; }
    bool IsTerminal { get; }
    GameResult? Result { get; }
    IReadOnlyList<IAction> GetLegalActions();
}

public interface IStateEvaluator
{
    float Evaluate(ISearchableGameState state, PlayerId perspective);
}
```

Design tradeoff: Clone() over undo-stack for simplicity and parallelism.
Optimize to structural sharing only if profiling shows cloning as bottleneck.

## 4. State Encoding

Hybrid bracket-tag + natural language format. 250-token budget.

```
[STATE turn=5 phase=declare_attackers active=P1]
[LIFE P1=17 P2=14]
[BOARD P1] Serra Angel 4/4 {flying vigilance} untapped
[BOARD P2] Sengir Vampire 4/4 {flying} untapped
[HAND P1] Wrath of God | Plains
[MANA P1=WWWW P2=BBB]
[ACTIONS] 0:cast_wrath 1:attack_all 2:pass
```

Compression techniques (applied in order when over budget):
1. Group identical permanents
2. Relevance tiers (active/passive/background)
3. Zone compression (graveyard type distribution)
4. Mana compression (available symbols, not individual lands)
5. Decision-relative windowing

## 5. Output Format

```
[THINK] reasoning chain [/THINK]
[ACTION] 0
[CONFIDENCE] 0.83
[PRIOR] 0:0.62 1:0.25 2:0.13
```

- `[ACTION]` — numbered index, unambiguous parsing
- `[THINK]` — optional CoT (training signal, skipped during fast rollouts)
- `[CONFIDENCE]` — routes to MCTS fallback when low
- `[PRIOR]` — policy distribution for MCTS exploration budget

Failure recovery: invalid index → mask + resample, or MCTS fallback.

## 6. Training Pipeline

| Stage | Games | Purpose |
|-------|-------|---------|
| Random vs Random | 100K | Rules/transitions |
| MCTS vs Random | 200K | Basic heuristics |
| MCTS vs MCTS | 500K | Interaction patterns |
| Annotated SFT | 500K | CoT from MCTS stats |
| RL self-play | 1M+ | Beat MCTS baseline |
| Model self-play | ∞ | Continuous flywheel |

Synthetic CoT: template-based generator converts MCTS visit statistics to
reasoning text. LLM-enhanced rewriting in Stage 2.

Reward shaping: potential-based (life diff, card advantage, lethal detection)
plus small-weight non-potential (mana efficiency, threat diversity).
Phase-dependent multipliers.

## 7. Evaluation Harness

1. **Comprehension suite** — factual questions about board state, ground truth from engine
2. **Matchup win rates** — vs Random (>90%), Heuristic (>65%), MCTS-500 (>45%)
3. **Elo pool** — Random=800, Heuristic=1200, MCTS tiers, model rated dynamically
4. **CoT quality** — consistency (reasoning matches action), fact accuracy, LLM-as-judge

## 8. Dependencies

- **Hard dependency:** Phase 2 (keywords, instants) + Phase 3 (spells, triggers)
- **Rationale:** Vanilla creature combat produces trivially simple games with
  insufficient strategic depth for meaningful model training
- **Prep work possible now:** `ISearchableGameState` interface, `StateEncoder`
  skeleton, `RunMatchup` eval infrastructure
