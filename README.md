# Ascension

An isometric-style action roguelite (Hades structure) where you climb out of Hell, through the mortal realm, and into Heaven to confront the Sovereign of Judgment. You play **Alistair the Defiant**; the **15 Constellations** are your sponsors, streaming down Stellar Blessings.

This repo is a **Unity 6 (6000.0.x) prototype**. It needs **no imported assets and no authored scene**: a `[RuntimeInitializeOnLoadMethod]` bootstrap builds the camera, hub, player, rooms and UI from code (procedural sprites + IMGUI). Open the folder in Unity, open any scene (an empty one is fine) and press Play.

## Controls

| Input | Action |
|---|---|
| WASD / arrows | Move |
| LMB (or J) | Melee arc |
| RMB (or K) | Ranged shot (disabled by the Meathead; costs gold with the Whale) |
| Space | Dash |
| Q | Sponsor special (Aegis shockwave, Leviathan vortex) |
| F | Accept a Chat Moderator prompt (or click it) |
| E | Interact / advance dialogue |
| 1-4, R | Pick a Blessing card / Fated Reroll |

## Game flow

```
Abyss Terminal (hub) --Gate of Ascent--> Run --die--> Result --> Abyss Terminal
                                          |--beat the Sovereign--> Ascension (victory) --> Abyss Terminal
```

* **Hub** - Vesper, Orrin and Cassian react to *how* you died (killer, hazard, biome, sponsors held, gold, death count). The **Stellar Mirror Matrix** spends banked Stardust on permanent upgrades and shows the Constellation codex.
* **Run** - four biomes in sequence: Ashlands of Hell -> Haunted Kingdoms of Earth -> Astral Gates of Heaven -> Apex Throne. Each biome is a list of layers offering 1-3 doors with a previewed reward icon; the last layer is the boss. Clearing a boss opens the transition door to the next realm.
* **Rewards** - Stellar Blessing (pick 1 of 3; re-picking an owned sponsor ranks it up to 5), Gold, Stardust (meta currency), Ambrosia Vial, Vitality Shard.
* **Star-Bursted State** - an enemy carrying two *different* cosmic ailments (Grave Bound, Stellar Burn, Bleed, Ignite, Corroded, Crushed) detonates on death for AoE damage that can chain.

## Architecture

All code is in `Assets/Scripts`, namespace `Ascension`. Dependencies point one way; cross-system communication goes through `GameEvents` or the `Player` facade.

```
Core/      Bootstrap, GameManager (flow state machine), CameraRig, GameEvents (event bus), InputReader
           (new Input System or legacy), SpriteFactory/Fx/YSort (procedural art), Layers, GlobalTime
Data/      ConstellationSponsor (ScriptableObject), ConstellationDatabase (the 15 defaults; assets in
           Resources/Constellations override them), EnemyDef/BossDef + EnemyDatabase + BiomeDatabase
Combat/    DamageInfo, Health, StatusController, Enemy (5 behaviours), Boss (phases + shields), Projectile
           (homing / wall-bending), StarBurst, HazardZone, Breakable
Player/    Player (facade), PlayerController (move/aim/dash), PlayerCombat (melee/ranged, single DealDamage
           choke point), PlayerStats
Sponsors/  SponsorBehaviour (hook base class), SponsorManager (the modifier state machine),
           SponsorFactory, Grim/Cosmic/Meta behaviours (5+5+5 classes), world objects (Thrall, Sun, Vortex, Spice)
Run/       RunState, RunMap (layered door graph), RunManager, RoomController (waves/rewards/doors),
           RoomBuilder, Door, RewardPickup, BlessingService, RunDrops
Meta/      MetaProgression (JSON save in PlayerPrefs + Matrix upgrades), DialogueDirector, HubController
UI/        IMGUI views: Hud, Blessing, Matrix, Dialogue, ChatPrompt, Result (GuiKit = 1280x720 scaled canvas)
```

### The 15-way modifier state machine

`SponsorManager` owns one `SponsorBehaviour` instance per equipped Constellation (re-equipping = rank up, +35% potency per rank). It forwards every gameplay hook to all of them; each sponsor overrides only what it changes and never references another sponsor:

| Hook | Used for |
|---|---|
| `ModifyOutgoing` / `ModifyVsTarget` | damage multipliers, status application, backstab crits |
| `OnHitDealt`, `OnEnemyKilled` | lifesteal, thralls, spices |
| `ModifyIncoming`, `InterceptFatal` | dodge chance, damage storage, time rewind |
| `OnHeal`, `OnDash`, `OnSpecial`, `Tick` | vial conversion, invisibility, shockwave/vortex, timers |
| `AllowsRanged`, `DashGivesIFrames`, `MoveSpeedMultiplier`, `MeleeCooldownMultiplier`, `GrantsTrueSight`, `RevealsMap` | policy queries aggregated across all sponsors |

Adding a 16th Constellation = one enum value, one `ConstellationDatabase` entry, one behaviour class, one line in `SponsorFactory`.

### Deviations from the supplied blueprints

* `ConstellationSponsor` is kept as specified, plus `category` and `maxRank`.
* `GimmickEngine` is folded into `ProcrastinatorBehaviour`: stillness charging is a per-sponsor concern, and the buff is consumed per *attack* (not per hit) via `OnAttackBegin/End`.
* `SpectralThrall` uses the `Enemy.Active` registry instead of `CompareTag("Enemy")` (no tag setup required, no physics query per retarget) and now attacks through `PlayerCombat.DealDamage`, so thralls inherit sponsor interactions (Grave Bound + another ailment = Star-Burst).
* The game is 2D top-down with Y-sorting to fake isometric depth, matching the `Rigidbody2D` blueprints.

## Status

Written without access to a Unity editor. The scripts compile cleanly against the UnityEngine 2021.3 reference assemblies (with `Rigidbody2D.velocity` standing in for Unity 6's `linearVelocity`), but **nothing has been playtested** - expect balance and feel tuning. The `ENABLE_INPUT_SYSTEM` branch of `InputReader` was reviewed by hand only. Placeholders to replace: procedural sprites (`SpriteFactory`), IMGUI (`UI/`), no audio.
