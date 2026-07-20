# DESIGN: Priority, Stack, and Combat Keywords

**Last Updated:** July 12, 2026

---

## Table of Contents

- [1. Overview](#1-overview)
- [2. Priority System](#2-priority-system)
- [3. Stack Implementation](#3-stack-implementation)
- [4. State Machine Model](#4-state-machine-model)
- [5. Action Space at Priority Nodes](#5-action-space-at-priority-nodes)
- [6. Combat Keywords](#6-combat-keywords)
- [7. Keyword Interaction Matrix](#7-keyword-interaction-matrix)
- [8. Damage Assignment Algorithm](#8-damage-assignment-algorithm)
- [9. Two-Step Combat Damage](#9-two-step-combat-damage)
- [10. Implementation Order](#10-implementation-order)
- [11. AI Implications](#11-ai-implications)
- [12. Rules References](#12-rules-references)

---

## 1. Overview

This document specifies the implementation requirements for the priority system
(Rule 117), the stack (Rule 405), and combat-relevant keyword abilities (Rule
702). These three systems together create the majority of decision points an AI
strategy must navigate.

Priority is Phase 1 scope (TASK_0104). Keywords span Phase 2 (thin slice) and Phase 5 (deferred); stack and
instant interactions are Phase 3 scope. This document covers the full target architecture.

## 2. Priority System

The priority loop (Rule 117) determines when players can act. It is the
heartbeat of MTG decision-making and the source of 80%+ of AI decision nodes.

### 2.1 Core Loop

```
1. Active player receives priority
2. Player with priority may:
   a. Cast a spell (instant anytime; non-instant only main phase + stack empty)
   b. Activate an ability
   c. Take a special action (play land, morph, etc.)
   d. Pass priority
3. If action taken (a/b) -> same player gets priority again
4. If pass -> next player in turn order gets priority
5. If ALL players pass in succession:
   - Stack non-empty -> resolve top object
   - Stack empty -> current step/phase ends
6. After any resolution -> active player receives priority
7. BEFORE granting priority: check SBAs repeatedly, then queue triggers
```

### 2.2 Pre-Priority Procedure (Rule 117.5)

Before any player receives priority, the game runs this loop:

```
repeat:
  perform all applicable SBAs simultaneously
until no SBAs performed
then:
  put all pending triggered abilities on stack (APNAP order)
repeat entire process until:
  no SBAs performed AND no triggers queued
THEN grant priority
```

### 2.3 Timing Restrictions

| Card Type         | When Castable                                      |
|-------------------|----------------------------------------------------|
| Instant           | Any time you have priority                         |
| Sorcery           | Main phase, stack empty, your turn                 |
| Creature          | Main phase, stack empty, your turn                 |
| Enchantment       | Main phase, stack empty, your turn                 |
| Artifact          | Main phase, stack empty, your turn                 |
| Planeswalker      | Main phase, stack empty, your turn                 |
| Activated ability | Any time you have priority (unless restricted)     |
| Mana ability      | Anytime, even during spell casting (no stack)      |
| Special action    | Varies (lands: main phase, stack empty, your turn) |

## 3. Stack Implementation

The stack is a LIFO data structure holding spells and abilities waiting to
resolve.

### 3.1 Key Rules

- Rule 405.1: Spells go on stack when cast; abilities when activated/triggered
- Rule 405.5: All pass in succession -> top resolves OR step/phase ends
- Rule 405.6c: Mana abilities resolve immediately, never use the stack
- Rule 608.2b: On resolution, check targets still legal; all illegal -> fizzle
- Rule 117.7: Casting while stack non-empty = "in response to" top object

### 3.2 Resolution Procedure

```
1. Pop top object from stack
2. If spell/ability has targets:
   a. Check each target still legal
   b. ALL targets illegal -> fizzle (spell to graveyard, ability ceases)
   c. Some targets illegal -> resolve with legal ones only
3. Follow instructions in order
4. If instant/sorcery -> put into owner's graveyard
5. If permanent spell -> put onto battlefield
6. If ability -> ceases to exist
7. Active player receives priority (after pre-priority SBA/trigger check)
```

## 4. State Machine Model

```
States:
  PROCESSING_SBAS        -- checking state-based actions + triggers
  GRANTING_PRIORITY(pid) -- player pid has priority
  RESOLVING              -- top of stack resolving
  STEP_TRANSITION        -- all passed, stack empty, advancing

Transitions:
  PROCESSING_SBAS -> GRANTING_PRIORITY(active)
      [no more SBAs or triggers to process]

  GRANTING_PRIORITY(pid) -> GRANTING_PRIORITY(pid)
      [player took action: cast, activate, special action]

  GRANTING_PRIORITY(pid) -> GRANTING_PRIORITY(next)
      [player passed, not all passed yet]

  GRANTING_PRIORITY(pid) -> RESOLVING
      [all passed in succession, stack non-empty]

  GRANTING_PRIORITY(pid) -> STEP_TRANSITION
      [all passed in succession, stack empty]

  RESOLVING -> PROCESSING_SBAS
      [resolution complete]

  STEP_TRANSITION -> PROCESSING_SBAS
      [new step/phase begins]
```

## 5. Action Space at Priority Nodes

At each `GRANTING_PRIORITY` state, `GetLegalActions()` must enumerate:

1. **Castable spells from hand** -- filtered by timing rules (Section 2.3)
2. **Activatable abilities** -- excluding summoning-sick tap abilities, already-used loyalty
3. **Special actions** -- play land (main phase, stack empty, once per turn)
4. **Pass priority** -- always legal

For targeted spells/abilities, each valid target combination is a distinct action.

### 5.1 Action Space Explosion

Worst case: 7 cards in hand x N targets each + M abilities x targets + pass.
Can reach 50-200 legal actions per priority hold.

Mitigations for MCTS:
- `[PRIOR]` from world model biases tree expansion
- Progressive widening: expand only top-K actions initially
- Macro-actions: "pass until opponent acts" for passive phases

## 6. Combat Keywords

### 6.1 Evasion Keywords (Restrict Blocking)

| Keyword | Rule    | Effect                                           |
|---------|---------|--------------------------------------------------|
| Flying  | 702.9   | Only blocked by flying or reach                  |
| Menace  | 702.111 | Must be blocked by 2+ creatures                  |
| Skulk   | 702.118 | Can't be blocked by creatures with greater power |
| Shadow  | 702.28  | Only blocked by shadow; can't block non-shadow   |

Evasion stacking: All restrictions are cumulative. Flying + menace = must be
blocked by 2+ creatures that each have flying or reach.

### 6.2 Damage-Modifying Keywords

| Keyword       | Rule   | Effect                                              |
|---------------|--------|-----------------------------------------------------|
| First Strike  | 702.7  | Deals damage only in first combat damage step       |
| Double Strike | 702.4  | Deals damage in both combat damage steps            |
| Deathtouch    | 702.2  | Any nonzero damage = lethal for assignment purposes |
| Trample       | 702.19 | Excess over lethal assigned to defending player     |
| Lifelink      | 702.15 | Controller gains life equal to damage dealt         |

### 6.3 State-Modifying Keywords

| Keyword        | Rule   | Effect                                         |
|----------------|--------|------------------------------------------------|
| Haste          | 702.10 | No summoning sickness                          |
| Vigilance      | 702.20 | Doesn't tap to attack                          |
| Reach          | 702.17 | Can block flying                               |
| Defender       | 702.3  | Can't attack                                   |
| Indestructible | 702.12 | Can't be destroyed (ignores lethal damage SBA) |

## 7. Keyword Interaction Matrix

### 7.1 Deathtouch + Trample

Rule 702.2c changes "lethal" to 1 for assignment. Rule 702.19b allows excess
to trample. Result: assign 1 to each blocker (lethal!), remainder to player.

Example: 6/6 deathtouch trample blocked by 5/5 + 3/3 -> assign 1+1, trample 4.

### 7.2 First Strike Timing

Two combat damage steps occur if any creature has first/double strike (Rule
510.4). Between steps: SBAs checked, dead creatures removed. A 2/2 first
striker kills a 4/2 vanilla before it deals damage.

### 7.3 Double Strike + Deathtouch + Trample

First strike step: 1 to blocker (lethal), rest tramples. SBAs: blocker dies.
Regular step: no blockers remain, all damage tramples.
Example: 3/3 triple-keyword vs 7/7 -> 1+2 trample + 3 trample = 5 to player.

### 7.4 Indestructible vs Deathtouch

Deathtouch causes destruction via SBA (704.5h). Indestructible prevents
destruction (702.12b). Result: deathtouch does nothing to indestructible.
Damage is still dealt (lifelink triggers, damage-tracking works).

### 7.5 Trample vs Indestructible Blocker

Indestructible doesn't change lethal for assignment purposes. A 7/7 trample
blocked by 2/2 indestructible: assign 2 (lethal), trample 5. Blocker survives,
player takes 5.

### 7.6 Protection Interactions

Protection prevents DEBT: Damage, Enchanting/Equipping, Blocking, Targeting.
For trample: lethal must still be assigned to pro-X blocker (then prevented),
so only lethal amount tramples.

## 8. Damage Assignment Algorithm

Rule 510.1c-d: Controller freely divides damage among blockers.

```
Input:
  attacker: { power, has_deathtouch, has_trample }
  blockers: [{ id, toughness, damage_already_marked }]

Output:
  assignment: Map<CreatureId, int>
  player_damage: int

Constraints:
  - Sum(assignments) + player_damage = attacker.power
  - player_damage > 0 ONLY IF has_trample AND all blockers >= lethal
  - lethal(b) = if has_deathtouch then 1
                else max(0, b.toughness - b.damage_already_marked)
  - Each assignment >= 0
  - Controller chooses freely (AI decides optimal division)

Special cases:
  - No blockers (removed): creature deals no combat damage
  - Trample + no blockers: all damage to player (Rule 702.19d)
  - 0 or negative power: no damage dealt (Rule 510.1a)
```

## 9. Two-Step Combat Damage

When first/double strike present (Rule 510.4):

```
FIRST COMBAT DAMAGE STEP:
  1. Only first/double strike creatures assign damage
  2. All damage dealt simultaneously
  3. SBAs checked (lethal -> destroy)
  4. Triggers on stack, priority passes normally
  5. Stack empties before proceeding

SECOND COMBAT DAMAGE STEP:
  1. Creatures that had NEITHER first/double strike at start of first step
     PLUS creatures that currently have double strike assign damage
  2. All damage dealt simultaneously
  3. SBAs, triggers, priority as normal

Key edge cases:
  - Removing first strike after first step: can't deal in second
  - Giving double strike after first step: creature deals in second
  - Creature killed in first step: never deals in second
```

## 10. Implementation Order

Combat-keyword priority ranking, most -> least AI decision impact (implementation order spans Phases 2 and 5):

| Priority | Keyword                      | Rationale                                    |
|----------|------------------------------|----------------------------------------------|
| 1        | Flying + Reach               | Most meaningful blocking decisions           |
| 2        | First Strike + Double Strike | Changes combat math fundamentally            |
| 3        | Trample                      | Adds damage-division decision                |
| 4        | Deathtouch                   | Changes lethal calculation, key interactions |
| 5        | Lifelink                     | Affects race calculations                    |
| 6        | Haste                        | Affects attack timing                        |
| 7        | Vigilance                    | Affects attack/defense tradeoff              |
| 8        | Menace                       | Changes blocking constraints                 |
| 9        | Defender                     | Simplest (restriction only)                  |
| 10       | Indestructible               | Changes trade evaluation                     |

## 11. AI Implications

### 11.1 Priority Creates the Decision Tree

MCTS must branch at every priority hold. The "respond" decision (counter?
pump in response to removal? let resolve?) is the core of strategic play.
Without the stack, the game is deterministic turn-taking.

### 11.2 Combat Evaluation

`IStateEvaluator` needs for combat position:
- **Clock**: turns to kill opponent
- **Defensive liability**: can opponent alpha-strike for lethal?
- **Trade evaluation**: which blocks produce favorable trades?
- **Evasion tax**: unblockable damage per turn

### 11.3 Minimum Viable Ruleset for Non-Trivial AI

MCTS produces interesting play only after Phase 3 (stack + instants). Vanilla
creatures + keywords alone have too shallow a decision tree. But Phase 2 data
bootstraps MCTS training pipeline stages 1-2.

## 12. Rules References

- Priority: Rules 117.1-117.7
- Stack: Rules 405.1-405.6
- Casting Spells: Rules 601.1-601.8
- Resolving: Rules 608.1-608.3
- State-Based Actions: Rules 704.1-704.8
- Combat Phase: Rules 506-511
- Combat Damage: Rules 510.1-510.4
- Keyword Abilities: Rules 702.2-702.20

Source: MTG Comprehensive Rules effective June 19, 2026.
