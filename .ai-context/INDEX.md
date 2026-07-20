# INDEX: AI.Kvasir

**Last Updated:** July 11, 2026

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
| 1     | Engine Correctness             | Rules-compliant simulation for current card pool                                              | Active  |
| 2     | Keyword Combat Slice           | Data model + 5 combat-math keywords (flying, first/double strike, trample, deathtouch)        | Planned |
| 3     | Stack & Targeting              | Stack, instants/sorceries + timing, single- and multi-target spells                           | Planned |
| 4     | Triggers & State-Based Actions | Triggered abilities (APNAP), full 704.5 SBA checklist — completes the 117.5 procedure         | Planned |
| 5     | Deferred Keywords + Card Pool  | Lifelink, haste, vigilance, menace, defender, indestructible + stack-aware card pool capstone | Planned |
| 6     | AI Strategies                  | ISMCTS, WorldModel (AgentWorld), Hybrid (AlphaZero pattern)                                   | Planned |

Sequencing note (ADR-010, ADR-013): Phase 2 is a thin slice of only the combat-math keywords; the remaining keywords are deferred to Phase 5 because the genuine AI decision depth is gated on the stack and interaction layer (Phases 3-4). The original single stack phase was split into Phase 3 (Stack & Targeting) and Phase 4 (Triggers & SBAs) to fit a ~1–1.5 week part-time budget per phase. TASK_0104 (priority loop) sits in Phase 1 as a correctness fix but is the direct foundation for Phase 3; per ADR-012 it also owns the SBA seam plus a minimal SBA set, which Phase 4 (TASK_0402) later expands. Replacement effects (Rule 614/615) and the layer system (Rule 613) are deferred past the Phase 6 AI milestone (ADR-014); Phase 4 adds only a transparent event-interception seam (TASK_0403) so the later replacement phase does not force an engine-wide refactor.

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

**Phase 5 — Deferred Keywords + Card Pool**

| Task      | Title                                 | Status      |
|-----------|---------------------------------------|-------------|
| TASK_0501 | Lifelink                              | Not Started |
| TASK_0502 | Haste                                 | Not Started |
| TASK_0503 | Vigilance                             | Not Started |
| TASK_0504 | Menace                                | Not Started |
| TASK_0505 | Defender                              | Not Started |
| TASK_0506 | Indestructible                        | Not Started |
| TASK_0507 | Card Pool Capstone (Stack-Aware Pool) | Not Started |

## 5. Phase Index

| Phase   | Folder       | Documents                                                      |
|---------|--------------|----------------------------------------------------------------|
| 1       | `PHASE_001/` | TASK_0101–TASK_0104                                            |
| 2       | `PHASE_002/` | DESIGN_Priority_Stack_And_Combat_Keywords, TASK_0201–TASK_0206 |
| 3       | `PHASE_003/` | TASK_0301–TASK_0305                                            |
| 4       | `PHASE_004/` | TASK_0401–TASK_0404                                            |
| 5       | `PHASE_005/` | TASK_0501–TASK_0507                                            |
| Backlog | `BACKLOG/`   | DESIGN_AI_Strategy_Architecture (Phase 6 spec)                 |
