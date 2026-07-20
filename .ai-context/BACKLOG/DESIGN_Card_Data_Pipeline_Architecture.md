# DESIGN: Card Data Pipeline Architecture

**Last Updated:** July 18, 2026

---

## Table of Contents

- [1. Overview](#1-overview)
- [2. Current Pipeline](#2-current-pipeline)
- [3. Problems](#3-problems)
- [4. Design Principle: Definition vs Instance](#4-design-principle-definition-vs-instance)
  - [4.1 Serialization-Separation Tripwire](#41-serialization-separation-tripwire)
- [5. Target Architecture](#5-target-architecture)
- [6. Transformation Spine](#6-transformation-spine)
- [7. Migration Notes](#7-migration-notes)
- [8. Open Questions](#8-open-questions)

---

## 1. Overview

This document reviews the card data pipeline — Scryfall scraping through ANTLR
parsing into the game engine — and proposes a more scalable, clearer shape. The
motivating concern is the proliferation of data-transfer types: the same domain
nouns (Card, Ability, Cost, Effect, Deck, Player) are re-declared across three
families, and adding a field or a mechanic touches many files.

The conclusion is that the type *count* is a symptom, not the disease. The three
stages are individually defensible; the real costs are (a) two divergent
transformation mechanisms and (b) a runtime layer that duplicates the definition
instead of holding per-instance state. Fixing those removes most of the types as
a side effect. This design backs ADR-019 (runtime-layer collapse) and ADR-020
(parse-spine unification).

## 2. Current Pipeline

For the single noun "Card" there are four representations:

| Stage       | Type                         | Location            | Persistence           |
|-------------|------------------------------|---------------------|-----------------------|
| Wire        | Scryfall `JToken`            | `ScryfallFetcher`   | Cached HTTP responses |
| Raw         | `UnparsedBlob.Card`          | `Kvasir.Contract`   | Lucene index          |
| Defined     | `DefinedBlob.Card`           | `Kvasir.Contract`   | YAML per card         |
| Runtime     | `Engine.Card` + `Permanent`  | `Kvasir.Engine`     | In-memory game state  |

`Cost`, `Effect`, and `Ability` each get declared two-to-three times
(`DefinedBlob.*` and `Engine.*`, over interfaces `ICost`/`IEffect`/`IAbility`).

Three transformation mechanisms move data between stages:

1. **JSON to Raw** — hand-mapped `token.ReadValue(...)` calls in `ScryfallFetcher`.
   This is the anti-corruption layer against Scryfall's schema. It is correct and
   should stay.
2. **Raw to Defined** — `MagicCardProcessor` for card-level fields, delegating to
   `MagicCardParser` (ANTLR) for mana cost and oracle text.
3. **Defined to Runtime** — `EntityFactory` converts a `DefinedBlob.Card` into an
   `Engine.Card`, then `DataExtensions.AsPermanent` wraps it into a `Permanent`
   with `IPart` components; `Creature`/`Land` are proxy views over a permanent.

## 3. Problems

- **The runtime Cost/Effect/Ability layer is anemic glue.** `Engine.Cost` and
  `Engine.Effect` hold only a `Kind` enum plus an `IParameter` bag keyed by
  `ParameterKey` (only `Amount` exists today). All behavior lives in handlers
  (`BaseCostHandler`/`BaseEffectHandler` and the concrete `PayingManaHandler`,
  `ProducingManaHandler`, `TappingHandler`). `EntityFactory` converts a typed
  `DefinedBlob.PayingManaCost` into `Engine.Cost { Parameter[Amount] = ManaBlob }`,
  and the handler casts it straight back out via `FindValue<IManaCost>(Amount)` —
  a typed to untyped to typed round-trip that erases the parse result.
- **Two transformation idioms on adjacent segments.** Raw-to-Defined uses the
  reflection-based `ProcessingResult` (non-generic, holds `object`, binds fields
  via `propertyInfo.SetValue` on init-only record properties). The ANTLR half uses
  a clean generic `ParsingResult<T>` with per-rule visitors and builders. The
  processor even re-wraps the typed ANTLR results back into untyped ones.
- **Reflection binding defeats records.** `BindTo` mutates `init` properties
  post-construction, boxes every `ushort`/`uint`/enum through
  `Expression<Func<Card, object>>`, has no compile-time safety, and silently binds
  default on an invalid child.
- **Duplicated base stats.** `CreaturePart` copies `Power`/`Toughness` from the
  card definition rather than referencing base stats and holding only mutable
  `Damage`.
- **Minor:** `Power = -42` sentinel null-objects; `RunSynchronously` async-over-sync
  with per-entry YAML loads and no card-instance sharing in `CreateDeck`;
  `Enum.TryParse` directly on Scryfall type strings (brittle, silently `Unknown`).

## 4. Design Principle: Definition vs Instance

The axis that matters is not serializable-vs-runtime; it is definition-vs-instance.

- **Definition** — shared, immutable, serializable. One card definition is a
  flyweight referenced by every copy of that card. `DefinedBlob.Card/Cost/Effect/
  Ability` are definitions. They carry no game state.
- **Instance** — per-object mutable state. `Permanent` (tapped, controller) plus
  `CreaturePart` (damage, summoning sickness) is an instance. A future
  `StackedEffect` (a defined effect plus its chosen target, X, and source) is an
  instance.

A runtime type earns its keep only when it holds instance state or behavior the
definition must not. `Permanent` does — it references the definition and adds only
mutable state. `Engine.Cost/Effect/Ability` do not — they re-declare the
definition and hold nothing extra, so they pay the full cost of a parallel family
plus a mapper for no benefit, and introduced the stringly-typed `Parameter` bag as
collateral.

Rule of thumb: unify while the shapes are identical; split at the moment they
diverge, not preemptively. `Cost/Effect/Ability` are identical across Defined and
Runtime today, so unify. `Card` genuinely diverges (mutable permanent state), so
keep `Permanent` separate — which it already is.

### 4.1 Serialization-Separation Tripwire

Keeping `DefinedBlob` as *both* the on-disk contract and the engine-consumed
definition is correct now: the shapes are identical and behavior is externalized
into handlers. Re-introduce a distinct serialization contract only when one of
these bites:

- A save or replay format must be frozen independently of engine internals.
- `DefinedBlob` starts accreting serialization cruft (type discriminators, schema
  version) that should not sit on the engine hot path.

Both triggers align with the Phase 8 replay work (ADR-017). Until then, a separate
serialization family is cost without benefit.

## 5. Target Architecture

- Keep `UnparsedBlob` (raw) separate — the anti-corruption boundary earns its keep.
- Treat `DefinedBlob` as the single canonical definition, consumed directly by the
  engine. `Kvasir.Engine` already references `Kvasir.Contract`, so this adds no new
  dependency.
- Delete `Engine.Cost`, `Engine.Effect`, `Engine.Ability`, and
  `IParameter`/`Parameter`/`ParameterKey`. Handlers dispatch on `Kind` (as they
  already do) and read the typed `DefinedBlob` subclass directly.
- Keep the types that hold genuine runtime behavior or state:
  `ManaBlob`/`IManaCost`/`IManaPool` (behavior: `CanPay`, `Pay`, `TotalAmount`) and
  `Permanent` plus its parts.
- `Permanent` references the card definition; `CreaturePart` holds only `Damage`
  and counters, with effective power/toughness computed from base stats on the
  definition. This also seats the Phase 2 combat-keyword layer cleanly.
- Introduce instance wrappers (`StackedEffect`, `StackedAbility` = defined element
  plus chosen target/X/source) only when the Phase 3-4 stack and targeting work
  needs them — not preemptively, and never by reviving the `Parameter` bag.

Net effect on the original concern: the runtime shrinks from a parallel family to
`Permanent` plus a couple of instance wrappers, deleting roughly six to nine types
and removing the type-erasure smell in one move.

## 6. Transformation Spine

Consolidate Raw-to-Defined onto the ANTLR half's model and retire the reflection
half:

- **One result type:** the generic, immutable `ParsingResult<T>`. Delete
  `ProcessingResult`, `ValidProcessingResult`, `InvalidProcessingResult`, and the
  reflection `BindTo`.
- **Build `DefinedBlob.Card` via a `Builder`**, matching the existing
  `PayingManaCost.Builder`/`ProducingManaEffect.Builder` pattern — typed setters,
  no reflection, no boxing.
- **Move regex-parsed fields into the grammar** (type line, mana cost,
  power/toughness) per ADR-009. This removes `Pattern.Card.Type`/`ManaCost` and the
  second-face regex TODO, and gives uniform char-accurate error positions.
- Card-level composition aggregates child failures via the existing
  `ParsingResult.CreateFailure(params ParsingResult[])`.
- Fix the `ThenParseText` fall-through as part of the migration: the unparseable
  branch discards the `WithMessage` return value (it only works via in-place
  mutation, unlike every sibling) and still binds a `NotSupported` ability onto an
  otherwise-invalid card.

Combined with the runtime collapse, the end-to-end pipeline reduces to two
mechanisms: a hand-mapped anti-corruption fetch (JSON to `UnparsedBlob`) and a
single ANTLR parse spine (`UnparsedBlob` to `DefinedBlob`), both producing the one
canonical `DefinedBlob`.

## 7. Migration Notes

This refactor is unscheduled. When picked up, a low-risk order is:

1. **Spine unification first, on one field.** Port mana cost to the
   builder-plus-`ParsingResult<T>` path and delete its `ProcessingResult` usage, as
   a de-risking prototype. Extend field by field, then delete the
   `ProcessingResult` family.
2. **Runtime collapse on the mana path.** Make `PayingManaHandler`/
   `ProducingManaHandler` read the typed `DefinedBlob` subclass, moving the
   `ManaBlob` conversion into the handler or a thin adapter. Then delete
   `Engine.Cost/Effect/Ability` and the `Parameter` triple.
3. **`Permanent` references the definition;** fold base stats onto the definition
   and trim `CreaturePart` to mutable state only.

Sequencing against the roadmap: the parse-spine work is a natural companion to the
Phase 2 keyword parser extension (ADR-009, TASK_0201), and the runtime-collapse
touches the same handler and `EntityFactory` surface that Phases 3-4 rewire for the
stack, triggers, and the event-application seam (ADR-014). Folding these refactors
into that window avoids touching the same code twice.

## 8. Open Questions

- **Dependency direction (ADR-019).** Whether handlers consume the concrete
  `DefinedBlob` subclasses directly (simplest, tightest) or through a minimal
  runtime interface over them (preserves a `Contract`-to-`Engine` seam at the cost
  of one thin abstraction). Settle before implementation.
- **Card-instance sharing.** Whether `CreateDeck` should share a single definition
  instance across copies (the existing TODO) — trivial once `Permanent` references
  an immutable definition.
- **Phasing.** Whether to schedule this as its own refactor phase or fold it into
  Phase 2 (spine) and Phases 3-4 (runtime), per the sequencing note above.
