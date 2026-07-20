# INDEX: AI.Kvasir

**Last Updated:** July 18, 2026

---

## Table of Contents

- [1. Project Overview](#1-project-overview)
- [2. Architecture](#2-architecture)
- [3. Roadmap](#3-roadmap)
- [4. Current Sprint](#4-current-sprint)
- [5. Phase Index](#5-phase-index)

---

## 1. Project Overview

AI.Kvasir is a training ground for experimenting and benchmarking AI/ML techniques applicable to artificial agents playing Magic: The Gathering.

- **Repository:** [cahyaong/ai.kvasir](https://github.com/cahyaong/ai.kvasir)
- **Stack:** C# .NET 9, ANTLR4, Paket, xUnit, Azure Pipelines
- **Approach:** Path C — correctness-first, expand rules coverage, then layer AI experimentation
- **Workflow:** Cahya codes, Loki advises/reviews/manages roadmap

## 2. Architecture

```
Contract (interfaces/enums)
    ↓
Engine (game logic, handlers, strategies)
    ↓
Core (parsing, IO, serialization)
    ↓
Clients (WPF visual debugger, CLI runner)
```

- **Handler pattern:** Actions, Costs, and Effects each have dedicated handlers
- **Pluggable AI:** `IStrategy` interface — strategies make all game decisions
- **Data pipeline:** Scryfall API → `UnparsedBlob.Card` → ANTLR/Processor → `DefinedBlob.Card` (YAML) → `EntityFactory` → `ICard`
- **Deterministic replay:** Seeded `IRandomGenerator` enables reproducible games

## 3. Roadmap

| Phase | Name                           | Goal                                                                                          | Status  |
|-------|--------------------------------|-----------------------------------------------------------------------------------------------|---------|
| 1     | Engine Correctness             | A legal 2-player game of the current vanilla pool runs start-to-finish deterministically      | Active  |
| 2     | Keyword Combat Slice           | Data model + 5 combat-math keywords (flying, first/double strike, trample, deathtouch)        | Planned |
| 3     | Stack & Targeting              | Stack, instants/sorceries + timing, single- and multi-target spells                           | Planned |
| 4     | Triggers & State-Based Actions | Triggered abilities (APNAP), full 704.5 SBA checklist — completes the 117.5 procedure         | Planned |
| 5     | Card Pool Completion           | A curated stack-aware pool plays legally with all combat keywords (capstone-led)              | Planned |
| 6     | AI Enablement                  | Reproducible master seed + split RNG streams and a masked deep-clone seam (the ISMCTS substrate) | Planned |
| 7     | First Agent (ISMCTS)           | An ISMCTS agent plays legal games and beats RandomStrategy on the current pool                | Planned |
| 8     | Observability & Replay         | Typed event trace (JSONL) plus seed and decision-log replay with a validation hash            | Planned |
| 9     | Advanced Agents                | WorldModel (AgentWorld) then Hybrid (AlphaZero pattern) agents                                 | Planned |

Sequencing note (ADR-010, ADR-013, ADR-021): Phase 2 is a thin slice of only the combat-math keywords; the remaining keywords are deferred to Phase 5 (Card Pool Completion) because the genuine AI decision depth is gated on the stack and interaction layer (Phases 3-4). The original single stack phase was split into Phase 3 (Stack & Targeting) and Phase 4 (Triggers & SBAs) to fit a ~1-1.5 week part-time budget per phase. TASK_0104 (priority loop) sits in Phase 1 as a correctness fix but is the direct foundation for Phase 3; per ADR-012 it also owns the SBA seam plus a minimal SBA set, which Phase 4 (TASK_0402) later expands. Replacement effects (Rule 614/615) and the layer system (Rule 613) are deferred past the AI milestone (ADR-014); Phase 4 adds only a transparent event-interception seam (TASK_0403) so the later replacement phase does not force an engine-wide refactor.

Execution infrastructure and AI are thin-sliced the same way (ADR-021): rather than building all determinism, observability, replay, and cloning before any agent, Phase 6 (AI Enablement) builds only what ISMCTS strictly requires — the master seed plus split RNG streams (TASK_0601) and a masked deep-clone seam (TASK_0602) — and Phase 7 stands up the first ISMCTS agent on that substrate. Observability and replay (Phase 8: typed event trace, JSONL sink, seed- and decision-log replay) are built afterward, once a working agent has revealed what actually needs instrumenting, and before Phase 9 (Advanced Agents: WorldModel then Hybrid) because the WorldModel's self-play training pipeline (ADR-008) consumes the event trace as training data. Phase number therefore still encodes execution order (ADR-015) with no phase gated on a later one: 6 to 7 to 8 to 9, each depending only on what precedes it. Escape hatch: if stack/trigger debugging in Phases 3-4 becomes painful before the AI work, the event-taxonomy plus JSONL sink (TASK_0801-0802) may be pulled forward to serve as the debugging substrate, in which case those phases emit into it; the seed, clone, and replay tasks stay in their phases as they serve the AI work, not rules debugging.

## 4. Current Sprint

**Phase 1 — Engine Correctness**

| Task      | Title                  | Status      |
|-----------|------------------------|-------------|
| TASK_0101 | Seed Determinism Fix   | Not Started |
| TASK_0102 | Combat Validation      | Not Started |
| TASK_0103 | London Mulligan        | Not Started |
| TASK_0104 | Priority Loop (RX-117) | Not Started |

**Execution order:** TASK_0101 → TASK_0102 → TASK_0103 → TASK_0104

**Phase 2 — Keyword Combat Slice**

| Task      | Title                                 | Status      |
|-----------|---------------------------------------|-------------|
| TASK_0201 | Keyword Data Model + Parser Extension | Not Started |
| TASK_0202 | Flying + Reach                        | Not Started |
| TASK_0203 | First Strike                          | Not Started |
| TASK_0204 | Double Strike                         | Not Started |
| TASK_0205 | Trample                               | Not Started |
| TASK_0206 | Deathtouch                            | Not Started |

**Execution order:** TASK_0201 → TASK_0202 → TASK_0203 → TASK_0204 → TASK_0205 → TASK_0206

**Phase 3 — Stack & Targeting**

| Task      | Title                              | Status      |
|-----------|------------------------------------|-------------|
| TASK_0301 | Stack Data Structure               | Not Started |
| TASK_0302 | Stack Resolution + Zone Transfer   | Not Started |
| TASK_0303 | Instant & Sorcery Types + Timing   | Not Started |
| TASK_0304 | Single-Target Selection + Legality | Not Started |
| TASK_0305 | Multi-Target + Partial Fizzle      | Not Started |

**Execution order:** TASK_0301 → TASK_0302 → TASK_0303 → TASK_0304 → TASK_0305

**Phase 4 — Triggers & State-Based Actions**

| Task      | Title                             | Status      |
|-----------|-----------------------------------|-------------|
| TASK_0401 | Triggered Abilities + APNAP Queue | Not Started |
| TASK_0402 | SBA Checklist Expansion (704.5)   | Not Started |
| TASK_0403 | Event-Application Seam            | Not Started |
| TASK_0404 | Optional (May) Triggers           | Not Started |

**Execution order:** TASK_0401 → TASK_0402 → TASK_0403 → TASK_0404

**Phase 5 — Card Pool Completion**

| Task      | Title                                 | Status      |
|-----------|---------------------------------------|-------------|
| TASK_0501 | Defender                              | Not Started |
| TASK_0502 | Vigilance                             | Not Started |
| TASK_0503 | Haste                                 | Not Started |
| TASK_0504 | Lifelink                              | Not Started |
| TASK_0505 | Menace                                | Not Started |
| TASK_0506 | Indestructible                        | Not Started |
| TASK_0507 | Card Pool Capstone (Stack-Aware Pool) | Not Started |

**Execution order:** TASK_0501 → TASK_0502 → TASK_0503 → TASK_0504 → TASK_0505 → TASK_0506 → TASK_0507

**Phase 6 — AI Enablement**

| Task      | Title                             | Status      |
|-----------|-----------------------------------|-------------|
| TASK_0601 | Master Seed + Split RNG Streams   | Not Started |
| TASK_0602 | Deep Clone + Determinization Seam | Not Started |

**Execution order:** TASK_0601 → TASK_0602

**Phase 7 — First Agent (ISMCTS)**

Task breakdown pending — see `BACKLOG/DESIGN_AI_Strategy_Architecture.md` (ISMCTS section).

**Phase 8 — Observability & Replay**

| Task      | Title                            | Status      |
|-----------|----------------------------------|-------------|
| TASK_0801 | Game Event Taxonomy              | Not Started |
| TASK_0802 | Observer Redesign + JSONL Sink   | Not Started |
| TASK_0803 | Seed Replay + Validation Hash    | Not Started |
| TASK_0804 | Decision Log Replay + Branching  | Not Started |

**Execution order:** TASK_0801 → TASK_0802 → TASK_0803 → TASK_0804

**Phase 9 — Advanced Agents**

Task breakdown pending — see `BACKLOG/DESIGN_AI_Strategy_Architecture.md` (WorldModel and Hybrid sections).

## 5. Phase Index

| Phase   | Folder       | Documents                                                      |
|---------|--------------|----------------------------------------------------------------|
| 1       | `PHASE_001/` | TASK_0101–TASK_0104                                            |
| 2       | `PHASE_002/` | DESIGN_Priority_Stack_And_Combat_Keywords, TASK_0201–TASK_0206 |
| 3       | `PHASE_003/` | TASK_0301–TASK_0305                                            |
| 4       | `PHASE_004/` | TASK_0401–TASK_0404                                            |
| 5       | `PHASE_005/` | TASK_0501–TASK_0507                                            |
| 6       | `PHASE_006/` | TASK_0601–TASK_0602                                            |
| 7       | `PHASE_007/` | (task breakdown pending)                                       |
| 8       | `PHASE_008/` | TASK_0801–TASK_0804                                            |
| 9       | `PHASE_009/` | (task breakdown pending)                                       |
| Backlog | `BACKLOG/`   | DESIGN_AI_Strategy_Architecture (Phase 7 + 9 spec)             |
