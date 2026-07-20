# DECISION: AI.Kvasir

**Last Updated:** July 11, 2026

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
