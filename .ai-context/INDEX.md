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

| Phase | Name | Goal | Status |
|-------|------|------|--------|
| 1 | Engine Correctness | Rules-compliant simulation for current card pool | Active |
| 2 | Keyword Combat Slice | Data model + 5 combat-math keywords (flying, first/double strike, trample, deathtouch) | Planned |
| 3 | Priority, Stack, Instants | Priority loop, stack, instants/sorceries, targeting, triggered abilities — the real AI decision depth | Planned |
| 4 | Deferred Keywords + Card Pool | Lifelink, haste, vigilance, menace, defender, indestructible + stack-aware card pool capstone | Planned |
| 5 | AI Strategies | ISMCTS, WorldModel (AgentWorld), Hybrid (AlphaZero pattern) | Planned |

Sequencing note (ADR-010): Phase 2 is a thin slice of only the combat-math keywords; the remaining keywords are deferred to Phase 4 because the genuine AI decision depth is gated on Phase 3 (the stack). TASK_0104 (priority loop) sits in Phase 1 as a correctness fix but is the direct Phase 3 foundation.

## 4. Current Sprint

**Phase 1 — Engine Correctness**

| Task | Title | Status | Assignee |
|------|-------|--------|----------|
| TASK_0101 | Seed Determinism Fix | Not Started | Cahya |
| TASK_0102 | Combat Validation | Not Started | Cahya |
| TASK_0103 | London Mulligan | Not Started | Cahya |
| TASK_0104 | Priority Loop (RX-117) | Not Started | Cahya |

**Execution order:** TASK_0101 → TASK_0102 → TASK_0103 → TASK_0104

**Phase 2 — Keyword Combat Slice**

| Task | Title | Status | Assignee |
|------|-------|--------|----------|
| TASK_0201 | Keyword Data Model + Parser Extension | Not Started | Cahya |
| TASK_0202 | Flying + Reach | Not Started | Cahya |
| TASK_0203 | First Strike | Not Started | Cahya |
| TASK_0204 | Double Strike | Not Started | Cahya |
| TASK_0205 | Trample | Not Started | Cahya |
| TASK_0206 | Deathtouch | Not Started | Cahya |

**Execution order:** TASK_0201 → TASK_0202 → TASK_0203 → TASK_0204 → TASK_0205 → TASK_0206

**Phase 3 — Priority, Stack, Instants** (needs task breakdown; see `PHASE_002/DESIGN_Priority_Stack_And_Combat_Keywords.md` for priority/stack design)

**Phase 4 — Deferred Keywords + Card Pool**

| Task | Title | Status | Assignee |
|------|-------|--------|----------|
| TASK_0401 | Lifelink | Not Started | Cahya |
| TASK_0402 | Haste | Not Started | Cahya |
| TASK_0403 | Vigilance | Not Started | Cahya |
| TASK_0404 | Menace | Not Started | Cahya |
| TASK_0405 | Defender | Not Started | Cahya |
| TASK_0406 | Indestructible | Not Started | Cahya |
| TASK_0407 | Card Pool Capstone (Stack-Aware Pool) | Not Started | Cahya |

## 5. Phase Index

| Phase | Folder | Documents |
|-------|--------|-----------|
| 1 | `PHASE_001/` | TASK_0101–TASK_0104 |
| 2 | `PHASE_002/` | DESIGN_Priority_Stack_And_Combat_Keywords, TASK_0201–TASK_0206 |
| 3 | `PHASE_003/` | (pending task breakdown) |
| 4 | `PHASE_004/` | TASK_0401–TASK_0407 |
| Backlog | `BACKLOG/` | DESIGN_AI_Strategy_Architecture (Phase 5 spec) |
