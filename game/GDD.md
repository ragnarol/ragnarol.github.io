# Fading Suns RPG — Game Design Document

## Concept

An isometric tile-based pixel art RPG set in the Fading Suns universe: a grim
space opera of feudal nobles, zealous priests, scheming guild merchants, and
alien peoples, where the very stars are dimming. Inspired by the original
Fading Suns tabletop RPG (HDI/Holistic Design) and the conversation depth of
the Ultima series.

---

## Core Pillars

| Pillar | Description |
|---|---|
| **Story over stats** | Progression is through narrative milestones, not XP/levels. |
| **Words matter** | Ultima keyword dialogue — what you ask and who you ask it to shapes the world. |
| **Factions are alive** | Every action shifts standing with Nobles, Church, Guild, Aliens, or Underworld. |
| **Small party, big world** | Start alone, recruit up to 3 companions through trust-building. |

---

## Setting: The Planet Aragon (working name)

One planet, four distinct regions. Travel between them is gated by
milestones and faction standing, not an arbitrary level requirement.

### Regions

| Region | Atmosphere | Key NPCs | Notes |
|---|---|---|---|
| **City** | Crowded market streets, crumbling noble quarter, beggars and merchants | Guildmaster Orlan, Brother Caspar, Fence Yara | Starting region. City guard faction patrol. |
| **Spaceport / Agora** | Cosmopolitan crossroads, alien traders, docking bays, black market | Vorox trader Grr'kash, Pilot Lena, Inquisitor Harkan | Unlocked after first milestone. Smuggler routes. |
| **Palace** | Cold marble, silk curtains, political intrigue | Baron Alexei, Lady Seraphina, Court Chaplain | Requires noble letter or Church sponsor to enter. |
| **Wilderness** | Alien ruins, dangerous fauna, occult sites, rebel camps | Ur-Obun sage Sila, Rebel leader Tomas, Antinomist cultist | Random encounter chance. Ancient artefacts. |

---

## Characters

### Starting Archetypes (choose one at game start)

**Noble Heir**
> "You carry the name of a crumbling house and a sword that still remembers
> better days."
- Body 5, Mind 4, Spirit 4
- Skills: Melee 3, Charm 3, Lore 2, Knavery 1
- Faction bonus: NoblesHouse +20
- Starts with: Letter of Introduction (opens Palace dialogue)
- Unique dialogue keyword: `house`

**Church Acolyte**
> "The Pancreator's light guides your hand. So does the Inquisition's eye."
- Body 3, Mind 5, Spirit 6 | Wyrd 15 | Theurgy rating 1
- Skills: Occultism 3, Lore 3, Medicine 2, Charm 2
- Faction bonus: UniversalChurch +20
- Starts with: Relic Shard (required for Sanctify rite)
- Unique dialogue keyword: `pancreator`

**Guild Merchant**
> "Coin speaks louder than a baron's seal. You've always known this."
- Body 4, Mind 6, Spirit 3
- Skills: Charm 3, Knavery 3, Artifice 2, TechRedemption 2
- Faction bonus: MerchantLeague +20
- Starts with: 50 Firebirds
- Unique dialogue keyword: `trade`

---

### Recruitable Companions (unlock via milestones + NPC trust ≥ 50)

| Name | Origin | Role | Where to Find |
|---|---|---|---|
| Grr'kash | Alien Vorox | Warrior, intimidation specialist | Spaceport |
| Sila | Alien Ur-Obun | Sage, Psi user, healer | Wilderness |
| Brother Caspar | Human Priest | Theurgy, medicine | City (Church quarter) |

---

## Progression: Milestones

No XP. No levels. You grow by *doing things that matter*.

### Milestone Chain (first act)

```
[Start]
  └─ Milestone 1: "First Contact"
       Trigger: Speak with any NPC and complete their keyword tree
       Reward: Dialogue option "rumour" unlocked with all NPCs

       └─ Milestone 2: "A Name in the City"
            Trigger: Help or hinder one faction in the City
            Reward: Spaceport region unlocked, party slot 2 unlocked

            ├─ Milestone 3a (Noble path): "The Baron's Ear"
            │    Trigger: Deliver letter to Baron Alexei
            │    Reward: Palace region unlocked, Noble faction +20

            ├─ Milestone 3b (Church path): "The Inquisitor's Test"
            │    Trigger: Assist Inquisitor Harkan with investigation
            │    Reward: Palace region unlocked, Church faction +20

            └─ Milestone 3c (Guild path): "A Deal Well Made"
                 Trigger: Complete a smuggling run via Spaceport
                 Reward: Black market access, Guild faction +20

  (All paths eventually converge on the central mystery of the dimming sun)
```

---

## Dialogue System (Ultima Style)

Each NPC has a **DialogueProfile** containing:
- A greeting (changes with faction standing)
- A set of **keywords** — words the player can type or click
- Each keyword has: response text, conditions, consequences, and new keywords to reveal

### How it works

1. Player approaches NPC, clicks to talk
2. Greeting appears; NPC hints at keywords (bolded in their speech)
3. Player clicks a keyword button **or types any word** in the input box
4. NPC looks up the keyword, responds, may reveal more keywords
5. Player types `bye` or clicks "Farewell" to end

### Keyword example: Brother Caspar

| Keyword | Aliases | Response | Reveals |
|---|---|---|---|
| `name` | — | "I am Brother Caspar, servant of the Pancreator." | `job`, `church` |
| `job` | `work`, `occupation` | "I tend the sick and hear confessions." | `sick`, `confession` |
| `church` | `faith`, `pancreator` | "The Universal Church is our light in dark times." | `inquisitor`, `theurgy` |
| `inquisitor` | — | "Harkan watches for tech heresy. Stay clear of him." | `harkan`, `tech` |
| `join` | `party`, `travel` | *[only if trust ≥ 50]* "If the Pancreator wills it, I shall accompany you." | — |

---

## Faction System

Five factions; reputation range −100 to +100.

| Faction | Identity | Allied benefit | Enemy consequence |
|---|---|---|---|
| **NoblesHouse** | Feudal aristocracy | Palace access, noble guards ignore you | Guards attack on sight |
| **UniversalChurch** | Theocratic faith | Shrines heal double, rites cheaper | Inquisitors pursue you |
| **MerchantLeague** | Commerce guild | Black market open, prices drop 20% | Embargo: no vendor sales |
| **Aliens** | Vorox, Ur-Obun, Ukar | Alien companions recruit at lower trust | Alien areas are hostile |
| **Underworld** | Criminals, rebels | Fence access, safe houses | City guard bounty on you |

Faction changes **propagate**: helping Nobles slightly hurts Church and Guild
(rivals); helping Church slightly helps Nobles (traditional allies).

---

## Combat

Turn-based, on the isometric tile grid.

- **Initiative**: Spirit + 1d10 (highest goes first)
- **Turn**: 4 move points + 2 action points
- **Attack roll**: 2d10 + relevant skill vs target's defence
- **Critical failure**: double 1s — fumble, may trigger Urge (for Psi users)
- **Critical success**: double 10s — double damage
- **Essential NPCs**: incapacitate but never die (story protection)

### Occult in combat

| Type | Source | Pool | Risk |
|---|---|---|---|
| Psi | Born talent | Wyrd | Urge (loss of control on crit fail) |
| Theurgy | Church training | Wyrd | Church standing drop on crit fail |

---

## Technical Architecture (Unity)

```
Assets/
  Scripts/
    Core/         GameManager, MilestoneManager, WorldState, FactionManager,
                  ItemManager, SaveSystem, AudioManager, SceneBootstrap,
                  CharacterCreation
    Characters/   Character (base), PlayerCharacter, NPC, PartyManager,
                  AbilitySet, Inventory
    Dialogue/     DialogueManager
    Combat/       CombatManager
    Occult/       OccultSystem
    World/        IsometricTileMap, Region (+ 4 subclasses), RegionGate,
                  NPCRoster, InteractableObject, GameClock,
                  IsometricCamera, PartyFollowAI
    UI/           DialogueUI, PartyUI, HUD, CombatUI, CharacterSheetUI,
                  FactionReputationUI, OccultUI, CharacterCreationUI,
                  RegionTravelUI
    Data/         CharacterData, NPCData, DialogueData (DialogueProfile +
                  DialogueKeyword + MilestoneData), FactionData, TileData,
                  AbilityData, ItemData
  Scenes/
    Bootstrap     (scene 0 — manager initialisation only)
    City
    Spaceport
    Palace
    Wilderness
  ScriptableObjects/
    Characters/   NobleHeir.asset, ChurchAcolyte.asset, GuildMerchant.asset
    NPCs/         BrotherCaspar.asset, GuildmasterOrlan.asset, …
    Factions/     NoblesHouse.asset, UniversalChurch.asset, …
    Items/        sword_basic.asset, relic_shard.asset, …
    Milestones/   milestone_first_contact.asset, …
  Sprites/        (pixel art — 32×32 characters, 64×32 isometric tiles)
  Tilemaps/       (Unity Tile assets for each region)
  Audio/          (music tracks per region + SFX)
```

### Key Unity settings

- **Tilemap mode**: Isometric Z as Y (Project Settings → 2D → Tilemap)
- **Camera**: Orthographic, 30° isometric angle
- **Pixel art**: Point (no filter) sampling on all sprites
- **Input**: Unity Input System package
- **UI**: TextMeshPro
- **Assembly**: Single `FadingSuns.asmdef` references `Unity.InputSystem` and `Unity.TextMeshPro`

---

## Pixel Art Style Guide

- **Tile size**: 64 × 32 px (2:1 isometric ratio)
- **Character sprite**: 32 × 48 px, 4-directional (8 frames walk, 4 frames idle)
- **Portrait**: 96 × 96 px, hand-painted style with visible brush strokes
- **Palette**: Muted, desaturated — the suns are fading. Accent colours for
  faction symbols and occult effects only.
- **Lighting**: Pre-baked shadow tiles; night palette swap via shader for
  after-21:00 time slots

---

## Milestones Roadmap (development)

- [ ] Bootstrap scene + scene transition system
- [ ] City region tilemap + NPC roster
- [ ] Dialogue system (keyword input + response)
- [ ] Basic click-to-move + party follow
- [ ] Faction reputation display
- [ ] Milestone 1 + 2 complete (story trigger)
- [ ] Spaceport region
- [ ] Combat system (first encounter)
- [ ] Palace region + noble questline
- [ ] Wilderness region + occult encounter
- [ ] Save / Load
- [ ] Audio (music per region, SFX)
- [ ] Companions (Grr'kash, Sila, Brother Caspar)
- [ ] Polish + pixel art pass
