# DESIGN: AI Advanced Agents (WorldModel + Hybrid)

**Last Updated:** August 29, 2026

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
- [9. Task Breakdown](#9-task-breakdown)
- [10. Open Questions and Scope Reality](#10-open-questions-and-scope-reality)

---

## 1. Overview

Phase 9 layers learning-based agents on top of the rules-complete engine and the
Phase 7 ISMCTS baseline. Architecture follows the AlphaZero pattern: a pluggable
neural-network prior combined with MCTS verification, trained via self-play.

Key insight: Qwen AgentWorld's training methodology (CPT→SFT→RL on interaction
traces) applies well as the agent layer, while the engine provides ground-truth
game mechanics and Phase 8's event trace supplies the training data.

Scope note: the first agent — ISMCTS — is delivered in Phase 7 and specified in
`PHASE_007/DESIGN_AI_SearchStrategy.md`. This document covers only the two *advanced*
agents that build on it: the standalone WorldModel and the MCTS-guided Hybrid.

**Phase 9 deliverable:** a learned agent (`WorldModelStrategy` or `HybridStrategy`)
that beats the frozen Phase 7 MCTS-500 baseline on held-out seeds and decks, measured
by the same self-play harness (`PHASE_007/DESIGN_AI_SearchStrategy.md` §Self-Play). If no
learned agent clears that bar within a bounded training budget, the honest outcome is
"ISMCTS remains champion" — Phase 9 exists to *test* the learned-agent hypothesis, not
to assume it. See §10.

## 2. Strategy Landscape

Implementation order (each builds on the previous):

1. `MctsStrategy` — ISMCTS with determinization for hidden information.
   **Delivered in Phase 7** (`PHASE_007/DESIGN_AI_SearchStrategy.md`); listed here only
   as the foundation the advanced agents extend.
2. `WorldModelStrategy` — Standalone inference against a trained LLM.
3. `HybridStrategy` — MCTS guided by world-model priors (AlphaZero pattern).

ISMCTS was chosen over MCCFR because MTG's branch factor (2^N attacks, N×M blocks,
priority at every step) and non-abstractable information sets make CFR
convergence impractical.

`HybridStrategy` integrates the world-model policy by feeding its `[PRIOR]`
distribution into Phase 7's node selection as a PUCT term (prior-weighted UCT, the
AlphaZero selection rule) rather than the uniform-prior UCT the Phase 7 baseline uses;
the `[CONFIDENCE]` field gates the model — low confidence falls back to plain MCTS
selection. The model's `Evaluate` output can also replace or blend with the Phase 7
logistic leaf evaluator. Both hook points already exist in the Phase 7 seams, so the
Hybrid is additive, not a rewrite.

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

Superseded-in-part note: for Phase 7, this abstract `ISearchableGameState` /
`IStateEvaluator` framing was NOT used — ISMCTS binds directly to the shipped
engine contracts (`IStrategy`, `ITabletop`, `IJudicialAssistant`, `GameJudge`);
see `PHASE_007/DESIGN_AI_SearchStrategy.md`. This interface remains the target
abstraction for the Phase 9 WorldModel/Hybrid agents, which need a
model-friendly state façade independent of the engine's internal types.

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

| Stage            | Games | Purpose              |
| ---------------- | ----- | -------------------- |
| Random vs Random | 100K  | Rules/transitions    |
| MCTS vs Random   | 200K  | Basic heuristics     |
| MCTS vs MCTS     | 500K  | Interaction patterns |
| Annotated SFT    | 500K  | CoT from MCTS stats  |
| RL self-play     | 1M+   | Beat MCTS baseline   |
| Model self-play  | ∞     | Continuous flywheel  |

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

- **Hard dependency — rules & pool:** Phases 2-5 (keyword combat, the stack, targeting, triggered abilities, the full SBA checklist, and the stack-aware card pool). Vanilla creature combat produces trivially simple games with insufficient strategic depth for meaningful model training.
- **Hard dependency — Phase 7 (ISMCTS):** the frozen MCTS agent is both the training/eval opponent and the search Hybrid guides; there is no Phase 9 without a working Phase 7 baseline to beat.
- **Hard dependency — Phase 8 (event trace):** the typed, append-only event trace (`TASK_0801`/`TASK_0802`) is the self-play training corpus. Interaction traces → SFT/RL data (§6) exist only once Phase 8 emits them; `TASK_0903` reads that trace.
- **Prep work possible now (no dependency):** the `ISearchableGameState`/`IStateEvaluator` façade (`TASK_0901`), the `StateEncoder` skeleton (`TASK_0902`), and the `RunMatchup` eval scaffolding (`TASK_0906`) can be stubbed against the current engine before the gating phases land.

## 9. Task Breakdown

Six tasks in execution order. `TASK_0901`, `TASK_0902`, and `TASK_0906` are the
prep-work-possible-now slice and can start before the gating phases complete; the
learned-agent tasks (`TASK_0903`–`TASK_0905`) are hard-gated on Phases 7-8.

| Task      | Title                     | Deliverable                                                                                         | Gated on         |
| --------- | ------------------------- | --------------------------------------------------------------------------------------------------- | ---------------- |
| TASK_0901 | Model-facing state façade | `ISearchableGameState`/`IStateEvaluator` adapter over engine contracts (§3)                         | none (prep)      |
| TASK_0902 | State encoder             | `StateEncoder`: bracket-tag + NL format, 250-token budget, 5 compression tiers (§4)                 | 0901             |
| TASK_0903 | Training-data export      | Phase 8 event trace → interaction-trace corpus; synthetic-CoT generator from MCTS stats (§6)        | Phase 7, Phase 8 |
| TASK_0904 | `WorldModelStrategy`      | Standalone model inference + `[ACTION]/[CONFIDENCE]/[PRIOR]` parser + failure recovery (§5)         | 0902, 0903       |
| TASK_0905 | `HybridStrategy`          | PUCT prior injection into Phase 7 selection + confidence-gated MCTS fallback + leaf-eval blend (§2) | 0904, Phase 7    |
| TASK_0906 | Evaluation harness        | Comprehension suite, matchup win-rates, Elo pool, CoT-quality judge, `RunMatchup` (§7)              | none (prep)      |

Task numbering follows the phase-offset scheme (Phase 9 → `TASK_09xx`). These are
not-started planning IDs and remain re-sequenceable until externally referenced.

## 10. Open Questions and Scope Reality

These are unresolved and must be settled (or explicitly deferred) before Phase 9 work
starts. They are the reason this document lives in `BACKLOG/` rather than an active phase.

1. **Does the learned agent earn its keep?** A well-tuned Phase 7 ISMCTS may already
   be strong enough that an LLM layer adds cost without winning more. Phase 9 is a
   hypothesis test with a hard success gate (§1 deliverable), not a foregone conclusion.
2. **Inference substrate in a .NET engine — undecided.** Options: ONNX Runtime (in-proc,
   fits a distilled small model), a local `llama.cpp` server over HTTP, or an external
   API (breaks determinism and offline replay). Leaning ONNX/local for a reproducible,
   self-contained engine; API inference is incompatible with the seed-replay guarantee.
3. **Training-scale realism.** The §6 table (1M+ games) is the full-fat AlphaZero path
   and is aspirational for a solo side project. A sane MVP ladder: first fit a cheap
   policy (logistic/GBM) on MCTS visit stats to prove the prior-injection plumbing,
   then only escalate to an LLM if the cheap prior measurably lifts MCTS strength.
4. **Determinization ↔ model priors.** The model sees a masked/determinized state
   (`TASK_0602` seam). Priors conditioned on a single determinization may be
   biased; open question whether to average priors across determinizations or accept
   the bias for speed.
5. **Token budget vs. board complexity.** The 250-token encoding (§4) is untested
   against late-game boards with wide permanents + deep graveyards; the compression
   tiers may lose decision-relevant detail. Needs a fidelity check on real traces.
