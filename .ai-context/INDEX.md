# INDEX: AI.Kvasir

**Last Updated:** July 5, 2026

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

| Phase | Name              | Goal                                                          | Status  |
|-------|-------------------|---------------------------------------------------------------|---------|
| 1     | Engine Correctness | Rules-compliant simulation for current card pool             | Active  |
| 2     | Keyword Abilities | Expand mechanics + card pool organically                      | Planned |
| 3     | Spells & Triggers | Non-creature spells, triggered/static abilities               | Planned |
| 4     | AI Strategies     | ISMCTS, WorldModel (AgentWorld), Hybrid (AlphaZero pattern)   | Planned |

## 4. Current Sprint

**Phase 1 — Engine Correctness**

| Task | Title | Status | Assignee |
|------|-------|--------|----------|
| TASK_0101 | Seed Determinism Fix | Not Started | Cahya |
| TASK_0102 | Combat Validation | Not Started | Cahya |
| TASK_0103 | London Mulligan | Not Started | Cahya |
| TASK_0104 | Priority Loop (RX-117) | Not Started | Cahya |

**Execution order:** TASK_0101 → TASK_0102 → TASK_0103 → TASK_0104

**Phase 2 — Keyword Abilities**

| Task | Title | Status | Assignee |
|------|-------|--------|----------|
| TASK_0201 | Keyword Data Model + Parser Extension | Not Started | Cahya |
| TASK_0202 | Flying + Reach | Not Started | Cahya |
| TASK_0203 | First Strike | Not Started | Cahya |
| TASK_0204 | Double Strike | Not Started | Cahya |
| TASK_0205 | Trample | Not Started | Cahya |
| TASK_0206 | Deathtouch | Not Started | Cahya |
| TASK_0207 | Lifelink | Not Started | Cahya |
| TASK_0208 | Haste | Not Started | Cahya |
| TASK_0209 | Vigilance | Not Started | Cahya |
| TASK_0210 | Menace | Not Started | Cahya |
| TASK_0211 | Defender | Not Started | Cahya |
| TASK_0212 | Indestructible | Not Started | Cahya |
| TASK_0213 | Card Pool Expansion (Keyword Creatures) | Not Started | Cahya |

**Execution order:** TASK_0201 → TASK_0202 → TASK_0203 → TASK_0204 → TASK_0205 → TASK_0206 → TASK_0207 → TASK_0208 → TASK_0209 → TASK_0210 → TASK_0211 → TASK_0212 → TASK_0213

## 5. Phase Index

| Phase | Folder | Documents |
|-------|--------|-----------|
| 1 | `PHASE_001/` | TASK_0101–TASK_0104 |
| 2 | `PHASE_002/` | DESIGN_Priority_Stack_And_Combat_Keywords, TASK_0201–TASK_0213 |
| Backlog | `BACKLOG/` | DESIGN_AI_Strategy_Architecture |
