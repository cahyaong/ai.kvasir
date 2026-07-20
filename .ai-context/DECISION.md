# DECISION: AI.Kvasir

**Last Updated:** July 12, 2026

---

## Table of Contents

- [1. ADR-001: Development Approach](#1-adr-001-development-approach)
- [2. ADR-002: Keyword Parsing Strategy](#2-adr-002-keyword-parsing-strategy)
- [3. ADR-003: Card Pool Expansion Strategy](#3-adr-003-card-pool-expansion-strategy)
- [4. ADR-004: Phase 1 Execution Order](#4-adr-004-phase-1-execution-order)
- [5. ADR-005: AI Search Strategy](#5-adr-005-ai-search-strategy)
- [6. ADR-006: State Encoding Format](#6-adr-006-state-encoding-format)
- [7. ADR-007: Model Output Format](#7-adr-007-model-output-format)
- [8. ADR-008: Training Pipeline Architecture](#8-adr-008-training-pipeline-architecture)
- [9. ADR-009: Unified ANTLR Oracle Text Parsing](#9-adr-009-unified-antlr-oracle-text-parsing)
- [10. ADR-010: Thin-Slice Phase Resequencing](#10-adr-010-thin-slice-phase-resequencing)
- [11. ADR-011: Task ID Stability and Renumbering](#11-adr-011-task-id-stability-and-renumbering)
- [12. ADR-012: State-Based Action Seam Boundary](#12-adr-012-state-based-action-seam-boundary)
- [13. ADR-013: Stack Phase Split and Renumber Cascade](#13-adr-013-stack-phase-split-and-renumber-cascade)
- [14. ADR-014: Replacement Effects Deferred with Interception Seam](#14-adr-014-replacement-effects-deferred-with-interception-seam)
- [15. ADR-015: Priority Encoded by Phase and Task Numbering](#15-adr-015-priority-encoded-by-phase-and-task-numbering)

---

## 1. ADR-001: Development Approach

- **Date:** 2026-05-23
- **Status:** Accepted
- **Decision:** Path C — correctness-first, expand rules, then layer AI
- **Context:** Three paths considered: (A) snapshot-first for MCTS/search AI, (B) telemetry-first for ML/RL, (C) correctness-first then AI on top
- **Rationale:** The engine must simulate games correctly before AI results are meaningful. Expanding rules coverage also grows the card pool, making future experiments more interesting.
- **Consequences:** AI experimentation deferred to Phase 4. Priority loop (RX-117) is the largest Phase 1 task.

## 2. ADR-002: Keyword Parsing Strategy

- **Date:** 2026-05-23
- **Status:** Superseded by ADR-009 (2026-07-11)
- **Decision:** ~~Regex/pattern matching in `MagicCardProcessor` for keywords; reserve ANTLR for structured abilities~~ Superseded — see ADR-009
- **Context:** Keywords are flags on cards (flying, trample). ANTLR grammar currently only handles activated mana abilities.
- **Rationale:** Original reasoning held that keywords don't need full grammar parsing. This proved premature: keywords are static abilities (Rule 702) and share the oracle-text parsing domain with all other abilities. Splitting parsing between regex and ANTLR creates two code paths for one concern. See ADR-009 for the superseding decision.
- **Consequences:** Superseded. Keyword recognition moves into the ANTLR grammar per ADR-009.

## 3. ADR-003: Card Pool Expansion Strategy

- **Date:** 2026-05-23
- **Status:** Accepted
- **Decision:** Card pool grows organically with engine capability — each new mechanic unlocks a batch of cards
- **Context:** Current pool is hardcoded (Portal set basics). Options: curated small pool, full Scryfall pipeline, or organic growth.
- **Rationale:** Organic growth ensures every card in the pool is fully supported by the engine. No "partially working" cards.
- **Consequences:** Data pipeline (`ScryfallFetcher` → `MagicCardProcessor` → YAML) already exists. New cards added as their mechanics become supported.

## 4. ADR-004: Phase 1 Execution Order

- **Date:** 2026-05-23
- **Status:** Accepted
- **Decision:** Seed fix → Combat validation → Mulligan → Priority loop
- **Context:** All four tasks are independent except priority loop enables instant-speed interaction.
- **Rationale:** Seed fix is trivial (10 min). Combat validation and mulligan are medium complexity and independent. Priority loop is the largest task and benefits from having the others stable first.
- **Consequences:** TASK_0101 through TASK_0103 can be done in any order. TASK_0104 should be last.

## 5. ADR-005: AI Search Strategy

- **Date:** 2026-06-26
- **Status:** Accepted
- **Decision:** Information Set Monte Carlo Tree Search (ISMCTS) over Monte Carlo Counterfactual Regret Minimization (MCCFR)
- **Context:** Phase 4 requires a search algorithm that handles hidden information (opponent's hand, library order). Two candidates: ISMCTS (determinization-based tree search) and MCCFR (game-tree traversal with regret minimization).
- **Rationale:** MTG's state space is too large for CFR convergence. ISMCTS handles partial observability via determinization (sampling opponent's hidden cards) and scales with compute budget. MCCFR requires full game-tree traversal that is intractable for MTG's branching factor.
- **Consequences:** `ISearchableGameState` interface needs `Clone()`, `ApplyAction()`, `Determinize()`, `IsTerminal`, `GetLegalActions()`. Pluggable `IStateEvaluator` allows swapping heuristic for learned evaluation later.

## 6. ADR-006: State Encoding Format

- **Date:** 2026-06-27
- **Status:** Accepted
- **Decision:** Hybrid bracket-tag + natural language encoding with 250-token budget and relevance tiers
- **Context:** The world model strategy needs a compact game state representation for LLM consumption. Options: pure JSON, pure NL, or hybrid structured-natural.
- **Rationale:** Hybrid achieves ~60% fewer tokens than JSON while remaining parseable. Structured tags (`[HAND]`, `[BOARD]`, `[STACK]`) give reliable extraction; NL within sections provides semantic richness. `StateEncoder` class uses relevance tiers (active/passive/background) to prioritize what the model sees within the token budget.
- **Consequences:** `StateEncoder` becomes a core component with configurable budget parameter. Relevance tier assignment must be updated as game phases change (e.g., graveyard becomes active-tier when recursion effects are in play).

## 7. ADR-007: Model Output Format

- **Date:** 2026-06-27
- **Status:** Accepted
- **Decision:** `[ACTION] N` numbered index + optional `[THINK]` CoT + `[CONFIDENCE]` score + `[PRIOR]` distribution
- **Context:** The model needs a structured output format that is fast to parse, supports action validation, and feeds back into MCTS.
- **Rationale:** Numbered action index enables fast parsing and invalid-action masking (resample or fall back to MCTS on invalid output). Optional CoT supports fast/slow routing — skip reasoning at high confidence. Prior distribution feeds MCTS policy network for tree expansion guidance. `ModelOutput` record encapsulates action/reasoning/confidence/prior.
- **Consequences:** Action space must be enumerable and indexed per game state. Model failure recovery: mask invalid actions, resample, or fall back to pure MCTS. Training data must include all four fields.

## 8. ADR-008: Training Pipeline Architecture

- **Date:** 2026-06-27
- **Status:** Accepted
- **Decision:** Six-stage progressive pipeline targeting ~1.3M training games
- **Context:** The world model strategy needs training data that progresses from simple to complex play. Options: direct RL from scratch, imitation learning from human games, or progressive self-play bootstrap.
- **Rationale:** Bootstrap from weak opponents and progressively increase difficulty. Stages: (1) Random vs Random baseline, (2) MCTS vs Random, (3) MCTS vs MCTS, (4) Annotated SFT from MCTS visit statistics (synthetic CoT), (5) RL with potential-based reward shaping (life differential, card advantage), (6) Self-play flywheel. Synthetic CoT from MCTS converts search statistics into natural language reasoning for SFT.
- **Consequences:** Requires Phase 2-3 completion (instants, removal spells) before training data is meaningful. Evaluation harness needed: comprehension suite (engine ground truth), matchup win rates (Elo pool), CoT consistency checks, LLM-as-judge sampling.

## 9. ADR-009: Unified ANTLR Oracle Text Parsing

- **Date:** 2026-07-11
- **Status:** Accepted
- **Decision:** ANTLR grammar parses ALL oracle text uniformly, including static keyword abilities. Supersedes ADR-002.
- **Context:** ADR-002 originally split parsing: regex for keywords, ANTLR for structured abilities. Revisiting this before Phase 2 implementation, keywords are static abilities under Rule 702 — they belong to the same parsing domain as activated and triggered abilities, not a separate flag-detection concern.
- **Rationale:** A single grammar-based code path is more maintainable than two parallel parsers (regex + ANTLR) for one concern. Keywords are not always simple flags: they appear in comma-separated lists ("First strike, trample"), alongside reminder text ("Flying (This creature can't be blocked except by creatures with flying or reach.)"), and in parameterized forms ("Protection from red", "Hexproof from black", "Ward {2}") that will require grammar rules regardless. Starting keyword recognition in ANTLR makes the later extension to parameterized keywords natural rather than a rewrite. The grammar file (`MagicCardKeyword.g4`) already exists and only needs keyword tokens plus a keyword-line rule added.
- **Consequences:** TASK_0201 extends `MagicCardKeyword.g4` with keyword lexer tokens and a grammar rule matching keyword lines, rather than adding a regex layer in the processor. Parameterized keywords (protection, ward, hexproof-from) become a grammar extension in a later phase, not a separate parsing mechanism. ADR-002's regex approach is abandoned.

## 10. ADR-010: Thin-Slice Phase Resequencing

- **Date:** 2026-07-11
- **Status:** Accepted
- **Decision:** Build only a thin slice of combat-math keywords in Phase 2, then jump to the stack/instants phase (now Phase 3), deferring the remaining keywords and the card pool capstone to Phase 4. AI strategies move from Phase 4 to Phase 5.
- **Context:** The original plan built all 12 keywords across 13 Phase 2 tasks before touching the stack. But the rules gap analysis established that MCTS produces non-trivial play only after the stack and instants exist — vanilla creatures plus keywords have a near-deterministic decision tree. Most of the genuinely interesting AI decisions (holding up instants, responding on the stack, targeting) live in the stack phase.
- **Rationale:** When a roadmap's real value is gated on a later phase, fully completing the intermediate phase first delays the payoff for low marginal value. The combat-math keywords (flying, first strike, double strike, trample, deathtouch) are the ones that make combat non-trivial; the rest (lifelink, haste, vigilance, menace, defender, indestructible) add little decision depth. Double strike is included because it is a near-free increment once first strike builds the two-step combat framework. Card pool expansion is more valuable once instants exist, so the capstone waits for a stack-aware pool.
- **Consequences:** Phase 2 = TASK_0201-0206 (data model + 5 combat-math keywords). Phase 3 = priority/stack/instants (needs task breakdown). Phase 4 = deferred keywords + card pool capstone (TASK_0401-0407, renumbered from old 0207-0213). Phase 5 = AI strategies. TASK_0104 (priority loop) stays in Phase 1 as a correctness fix but is the direct Phase 3 foundation. Card pool follows a hybrid model: each slice keyword task adds its canonical test cards as real YAML now; the full capstone (TASK_0407) rounds out a stack-aware pool later.
- **Superseded in part:** ADR-013 (Phase 3 split + renumber cascade) revised the phase→task mapping described above — the Phase 3/4/5 assignments and the capstone ID (now TASK_0507) are renumbered there. See ADR-013 for the current mapping; this ADR's thin-slice rationale still stands.

## 11. ADR-011: Task ID Stability and Renumbering

- **Date:** 2026-07-11
- **Status:** Accepted
- **Decision:** Task IDs are freely renumberable while a task is not-started and referenced only inside the atomically-edited `.ai-context/` folder. An ID becomes stable ("load-bearing") only once something external references it.
- **Context:** The phase-offset numbering scheme (Phase N = N*100+1) was introduced to avoid cascading renumbers. This raised the question of whether IDs are immutable. The thin-slice resequencing (ADR-010) required renumbering not-started tasks 0207-0213 into Phase 4.
- **Rationale:** The phase-offset scheme solves one narrow problem — inserting a new task into an *active* phase without shifting siblings. It does not forbid wholesale re-sequencing of planning artifacts that have not been actioned. An ID is load-bearing only when an external artifact points to it: a git branch (`feature/TASK_XXXX`), a CR or commit message, in-progress code comments, or cross-references from other committed artifacts. This is the same principle as git history: local unpushed commits are fair game to rebase; pushed/shared history is not.
- **Consequences:** Renumbering not-started, internally-referenced-only tasks is a safe, single-commit operation (rename files, update internal IDs and cross-references atomically). Once a task is started or externally referenced, its ID freezes. This unblocks roadmap restructures like ADR-010 without contorting the folder structure to preserve stale numbers.

## 12. ADR-012: State-Based Action Seam Boundary

- **Date:** 2026-07-11
- **Status:** Accepted
- **Decision:** The priority loop (TASK_0104, Phase 1) owns the state-based-action *seam* (the `PROCESSING_SBAS` state and the Rule 117.5 pre-priority procedure) plus a *minimal SBA set* scoped to the current card pool. The Phase 4 SBA task (TASK_0402) owns only the *expansion* of the checklist — it never modifies the loop.
- **Context:** SBAs (Rule 704) are a Phase 3/4 must-have, but Rule 117.5 makes an SBA check point part of the priority loop's definition: SBAs are checked repeatedly immediately before any player receives priority. This couples TASK_0104 (Phase 1 priority loop) to the SBA system. If TASK_0104 built a bare loop with no SBA hook, the Phase 4 SBA task would have to reach back and refactor the just-built state machine, and "TASK_0104 done" would be a fiction. Three options were considered: (A) seam + minimal set in 0104, (B) bare loop now / all SBAs + loop refactor in Phase 4, (C) seam interface only with a no-op stub.
- **Rationale:** Option A chosen. ADR-001 is correctness-first — a priority loop that skips the 117.5 SBA check is knowingly wrong even for vanilla creatures, so the procedure is part of the loop, not a later feature. A avoids a Phase 4 rewrite of the freshly-built state machine and unifies "how things die" from Phase 1 on (combat death routes through the same checker). The minimal set is naturally bounded by the current pool (vanilla creatures + basic lands): only RX-704.5a (0 life → lose), RX-704.5c/f (0 toughness → graveyard), and RX-704.5g (lethal damage → destroy). This keeps TASK_0104 bounded (est. 4-6h → 6-8h) rather than pulling the full 704.5 checklist forward.
- **Consequences:** TASK_0104 gains the SBA seam, the minimal set, and combat-death routing in its acceptance criteria. TASK_0102 (Combat Validation) no longer applies lethal-damage death inline — it defers to the SBA checker. Phase 4's TASK_0402 becomes "expand the 704.5 checklist behind the existing seam," a clean additive task rather than a build-plus-refactor. The seam boundary is fixed: the SBA checklist may grow, the priority loop structure does not.

## 13. ADR-013: Stack Phase Split and Renumber Cascade

- **Date:** 2026-07-11
- **Status:** Accepted
- **Decision:** Split the original Phase 3 (priority/stack/instants/targeting/triggers/SBAs, ~20h) into two phases sized to a ~1–1.5 week part-time budget (~8-12h each): Phase 3 — Stack & Targeting (TASK_0301-0305) and Phase 4 — Triggers & State-Based Actions (TASK_0401-0402). The former deferred-keywords phase renumbers to Phase 5 (TASK_0501-0507) and the AI-strategies phase to Phase 6.
- **Context:** The Phase 3 task breakdown summed to roughly 20h — about 2.5 weeks part-time at ~8h/week — well over the target of a single 1–1.5 week phase. The work has a natural seam: the stack plus spell-speed casting and targeting form one shippable milestone (instants/sorceries + targeted removal work on the stack), while triggered abilities and the full SBA checklist form a second (the Rule 117.5 pre-priority procedure is complete).
- **Rationale:** Sizing phases to a consistent part-time budget keeps each phase independently shippable and reviewable, and gives a clear "done" milestone per phase. Triggers & SBAs are placed *before* the deferred keywords because several deferred keywords depend on them (lifelink is damage-linked life gain, indestructible modifies the destroy SBA), so the interaction layer must exist first. The renumber is safe under ADR-011: all affected tasks are not-started and referenced only inside the atomically-edited `.ai-context/` folder.
- **Consequences:** Phase 3 = Stack & Targeting (0301 stack, 0302 resolution, 0303 instant/sorcery types + timing, 0304 single-target, 0305 multi-target). Phase 4 = Triggers & SBAs (0401 triggered abilities + APNAP queue, 0402 SBA checklist expansion). Phase 5 = deferred keywords + card pool capstone (0501-0507, renumbered from 0401-0407). Phase 6 = AI strategies. TASK_0507's dependencies and its AI-phase reference were updated to the new numbers. Each Phase 3/4 task ships its own canonical test cards (hybrid card-pool model, ADR-010); the stack-aware pool capstone remains TASK_0507. Phase 2 (~13h) and Phase 5 (~13h) sit slightly over the budget and may be split later if desired.
- **Correction (2026-07-12):** The ~13h Phase 5 figure predates the granular per-task estimates, which sum to ~6.5-8.5h (six 30-45 minute keyword tasks plus the card-pool capstone). Phase 5 is a deliberately light phase and will not be split; the budget is a reviewability ceiling, not a quota. TASK_0507 is bumped to 3-4h to reflect authoring 20-30 cards plus a full integration test. The Phase 2 figure is unaffected by this note.

## 14. ADR-014: Replacement Effects Deferred with Interception Seam

- **Date:** 2026-07-12
- **Status:** Accepted
- **Decision:** Full replacement effects (Rule 614/615) and the continuous-effects layer system (Rule 613) are deferred to a phase after the Phase 6 AI milestone. A lightweight event-interception *seam* — no replacement logic — is added during Phases 3-4 as TASK_0403 so the later replacement phase does not force a re-plumbing of the engine's event pipeline.
- **Context:** Replacement effects were absent from the entire roadmap, raising whether they must precede AI the way the stack does. The 2026-06-27 rules-gap analysis had already scoped the must-have-before-AI set to priority, stack, instants/sorceries, activated abilities, triggered abilities, targeting, and SBAs — implicitly excluding 614. This ADR makes that decision explicit and addresses the seam question.
- **Rationale:** AI decision depth is gated on decision *branching*, which the stack and interaction layer (Phases 3-4) provide (ADR-010). Replacement effects are overwhelmingly automatic ("if X would happen, instead Y") and add rules-correctness weight but negligible branching; the few branching cases (Rule 616 multiple-replacement choice, "you may" replacements) are rare and shallow. The curated pool through Phase 5 contains no true 614 effect — notably indestructible is a Rule 704.5f/h SBA modification handled by TASK_0402/TASK_0506, not a replacement effect. Deferring the full 614/613 build keeps the pre-AI phases focused. However, per ADR-012's seam-not-implementation principle, skipping the seam entirely would force a later refactor of every event-application site; a cheap interception chokepoint added while those sites are already being touched avoids that.
- **Consequences:** TASK_0403 (Phase 4, depends on TASK_0401) introduces a single event-application chokepoint that combat damage (currently inline field mutation in `GameJudge.ExecuteResolvingCombatDamageStep`) and effect resolution (`BaseEffectHandler.Resolve`) route through, holding an empty pre-application replacement list. No 614/613 behavior is implemented — the list stays empty until the post-AI replacement phase. Indestructible remains a SBA modification, unaffected. A future ADR will define the full 614 pipeline, Rule 616 multiple-replacement ordering, and the Rule 613 layer system.
- **Relation:** Extends ADR-012 (state-based-action seam boundary) — same principle applied to the event/replacement pipeline.

## 15. ADR-015: Priority Encoded by Phase and Task Numbering

- **Date:** 2026-07-12
- **Status:** Accepted
- **Decision:** Remove the `Priority` field from all TASK Summary tables. Task priority is now encoded implicitly by phase number, then task number within the phase (lower = higher priority / done earlier). Task numbers within a phase are assigned in execution order.
- **Context:** TASK files carried a `Priority` field (P1/P2) that was explicitly exempted from RULE_Document §1.2 (which prohibits priority markers in documents). The field duplicated information already implied by phase sequencing plus the per-phase execution order, and could drift out of sync with either.
- **Rationale:** A single source of truth — the task number — is less error-prone than a parallel Priority field that must be kept consistent by hand. Encoding priority in the number also removes the RULE_Document §1.2 exemption, tightening compliance. Phase 5 was the only phase whose numeric order did not already match its execution order, so its keyword tasks were renumbered so that number equals priority. `Estimate` and `Status` remain — they are not priority signals.
- **Consequences:** The `Priority` row was removed from all 26 TASK files (the `Priority` column in `DESIGN_Priority_Stack_And_Combat_Keywords.md` is a content ranking table, not a metadata field, and is unaffected). Phase 5 renumber: defender → 0501, vigilance → 0502, haste → 0503, lifelink → 0504, menace → 0505 (indestructible 0506 and capstone 0507 unchanged). INDEX's Phase 5 sprint table and execution order were updated to plain ascending order. The stored `.ai-context` convention (previously "TASK files may carry status/priority") is updated to drop priority. The renumber is safe under ADR-011 — all tasks are not-started and referenced only inside the planning folder; local unpushed commit 7f5a5dc cites the old lifelink number 0501 in its message, which is cosmetic.
- **Relation:** Tightens RULE_Document §1.2 compliance; extends ADR-011 (renumber safety).
