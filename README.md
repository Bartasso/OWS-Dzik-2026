# OWS Dzik — Albert's Boar Game

> **This is an experiment.** The project tests how far a Unity game can get when the code is written
> by an AI agent ([Claude Code](https://claude.com/claude-code)) driving the editor, while a human acts
> as designer, director and reviewer. The game is real, but first of all this is a testbed for an
> agentic workflow, not a product.

## The game

A fast-paced **third-person roguelike** in the spirit of Risk of Rain 2 and Megabonk, with D&D mechanics.
The heroes are real friends: Ada the elf ranger with her dog Loki, Bar the dwarf barbarian, Sir Seba the
knight and Albert the wizard. They cut through hordes of parodied monsters from the D&D bestiary in a
modern-fantasy world where technology runs on magic.

- **Hordes and builds:** one active weapon, three passive ones, gear, and trinkets that stack without limit.
  Any weapon fits any class, and talents change scaling (a wizard hitting with strength is a goal, not a bug).
- **A d20 when it matters:** combat has no rolls; instead the map is dotted with situations settled by
  a d20 skill check (a cat stuck in a tree, an overturned cart, an enchanted vending machine).
  Natural 20s and natural 1s are a spectacle.
- **Vices instead of class skills:** each hero has a buff on F (beer, homemade liqueur, vodka, cigarettes)
  and an intoxication meter that rewards risk and punishes excess.
- **Fast movement:** run, jump, dash, slide, wall run. Movement is the main defence against the horde.
- **The night shop and the resort:** between arenas, a 24/7 corner shop suspended between dimensions;
  between runs, a holiday resort where rescued friends end up.

Full design (in Polish): [`docs/GDD.md`](docs/GDD.md).

## How it is made

The human writes the design ([GDD](docs/GDD.md)) and the working rules ([`CLAUDE.md`](CLAUDE.md)) and makes
the decisions. The agent plans, writes code and tests, compiles, runs the tests and the game in the editor,
takes screenshots and reports back. Key pieces:

| Piece | Role |
|---|---|
| [`CLAUDE.md`](CLAUDE.md) | The agent's contract: architecture, hard rules, conventions, work loop (plan → code → tests → compile → summary). |
| [`docs/GDD.md`](docs/GDD.md) | Source of truth for mechanics. When code and GDD disagree, the agent must stop and ask. |
| [`docs/OPEN_QUESTIONS.md`](docs/OPEN_QUESTIONS.md) | Contradictions and gaps found by the agent, plus a log of decisions made. |
| Unity MCP | Bridge between the agent and the editor: compilation, console, tests, scenes, Play Mode, screenshots. |
| Editor scripts | Scenes and prefabs are built by code (`Tools/Sandbox/...`); nobody hand-edits `.unity` or `.prefab` files. |

Rules that keep AI-written code in check:

- **Fail loudly:** every data definition validates itself, and the game refuses to start with broken data.
- **Tested formulas:** damage, skill checks, ability scaling and stats are covered by EditMode tests.
- **Deterministic randomness:** one seed per run and separate RNG streams, for reproducible bugs and co-op.
- **Logic separated from presentation:** input becomes commands, and game state knows nothing about visuals.
- **Data in ScriptableObjects:** no balance numbers hard-coded.

## Status

Milestone **M0 (prototype)**. Working:

- core: RNG streams, d20 skill checks (advantage/disadvantage, natural 1 reroll from LCK), a stat system
  with modifiers, trinkets and talents with effects, data validation;
- a playable sandbox: Albert with a fireball staff, pooled enemy hordes, a boss, skill check sites that
  pause the game, damage numbers, a debug panel;
- a version of the sandbox built on PSX-style assets.

In progress: the character controller on [OpenKCC](https://github.com/nicholas-maltbie/OpenKCC) and the
movement from the GDD. Next: level-up cards, more weapons and enemies, vices, the night shop.
Roadmap: [GDD, section 12](docs/GDD.md#12-roadmapa).

## Running it

Requires **Unity 6.6 (`6000.6.7f1`)** with URP.

1. Open the project in Unity.
2. Run `Tools/Sandbox/Build Sandbox Scene`, then press Play in `Assets/_Project/Scenes/Sandbox.unity`.
3. Controls: WASD move, mouse aim, LMB shoot, E interact, Tab cursor, F1 panel, F5 new run.

The `SandboxPsx` scene uses purchased asset packs that are **not in the repository** (their licenses forbid
redistribution). Without them the project still compiles, but that scene has missing models.
Importing the packs is described in [`docs/THIRD_PARTY.md`](docs/THIRD_PARTY.md).

EditMode tests without an open editor:

```bash
<path-to-Unity>/Editor/Unity -batchmode -nographics -projectPath . \
  -runTests -testPlatform EditMode -testResults ./TestResults/editmode.xml -logFile -
```

## Layout

```
Assets/_Project/Scripts/
  Core/          plain C#: RNG, skill checks, formulas, stats (no UnityEngine)
  Data/          ScriptableObject definitions with validation
  Gameplay/      combat, enemies, player, commands, bootstrap
  Presentation/  camera, damage numbers, animation
  Sandbox/       test tools
  Editor/        scene and data builders, validators
Assets/_Project/Tests/EditMode/
Assets/Plugins/OpenKCC/   character collision (MIT, vendored copy with local changes)
docs/                     GDD, open questions, third-party assets
```

## Third-party licenses

- [OpenKCC](https://github.com/nicholas-maltbie/OpenKCC) by Nicholas Maltbie, MIT:
  `Assets/Plugins/OpenKCC/`.
- PSX asset packs (heyheythere, StarkCrafts) are purchased and kept local only, outside the repository.
