# Third-party assets

The purchased packs are **not in git**. Their licenses (heyheythere, StarkCrafts) forbid sharing the files, and this
repository is public. The whole `Assets/ThirdParty/` folder and the `zips/` archive folder are in `.gitignore`.
Everyone imports the packs locally from the archives.

Game code never compiles against pack scripts. Prefabs and scenes in `_Project` only reference models, textures,
materials, animations and shaders. Without the packs the project compiles, but the `SandboxPsx` scene has missing
references.

## Packs

| Pack | Source | Location in project | What we use |
|---|---|---|---|
| PSX Castle Kit 1.0.4 | `psx-castle-kit-assets.zip` → `unity/*.unitypackage` | `ThirdParty/HeyHeyThere/PSXCastleKit` | everything but demos |
| PSX Dungeon Kit 1.0.5 | `psx-dungeon-kit-assets.zip` → `unity/` | `…/PSXDungeonKit` | everything but demos |
| PSX Hell Temple Kit 1.0.4 | `psx-hell-temple-kit-assets.zip` → `unity/` | `…/PSXHellTempleKit` | everything but demos |
| PSX Tech Base Kit 1.0.4 | `psx-tech-base-kit-assets.zip` → `unity/` | `…/PSXTechBaseKit` | everything but demos |
| PSX Firearms 1.4.0 | `psx-firearms-assets.zip` → `unity/` | `…/PSXFirearms` | everything but demos |
| PSX Melee Weapons 1.4.3 | `psx-melee-weapons-assets.zip` → `unity/` | `…/PSXMeleeWeapons` | everything but demos |
| PSX Sci-Fi Weapons 1.0.1 | `psx-scifi-weapons-assets.zip` → `unity/` | `…/PSXSciFiWeapons` | everything but demos |
| PSX Retro FPS Monsters 1.1.0 | `psx-retro-fps-monsters-assets.zip` → `unity/` | `…/PSXRetroFPSMonsters` | everything but demos |
| PSX Survival Pickups 1.0.8 | `psx-survival-pickups-assets.zip` → `unity/` | `…/PSXSurvivalPickups` | everything but demos |
| PSX Skyboxes 1.1.3 | `psx-skyboxes-assets.zip` → `unity/` | `…/PSXSkyboxes` | everything but demos |
| Retro FPS Textures 1.0.1 | `retro-fps-textures-assets.zip` → `unity/` | `…/RetroFPSTextures` | textures and materials |
| PSX FX 1.3.0 | `psx-fx-1.3.0.unitypackage` | `…/PSXFX` | sprite FX, prefabs, `SpriteFX.cs` |
| PSX Look 1.4.0 | `psx-look-1.4.0.unitypackage` | `…/PSXLook` | PSX shaders, `PSXScreen` on the camera |
| Retro Screen FX 1.3.0 | `retro-screen-fx-1.3.0.unitypackage` | `…/RetroScreenFX` | screen effect |
| PSX Animation Library Free | `psx-animation-library-free-assets.zip` (raw) | `…/PSXAnimationLibraryFree` | `fbx/` → `Models/`, `textures/` → `Textures/` |
| PSX Character Creator Free | `psx-character-creator-free-assets.zip` (raw) | `…/PSXCharacterCreatorFree` | `fbx/` → `Models/`, `textures/` → `Textures/` |
| Explosion FX | `explosion-fx-assets.zip` (raw) | `…/ExplosionFX/Sheets` | `sheets/` |
| Retro FPS SFX | `retro-fps-starter-kit-assets.zip` → `addons/retro_fps_sfx` | `…/RetroFPSSfx` | `.ogg` only |
| Retro FPS HUD | `retro-fps-starter-kit-assets.zip` → `addons/retro_fps_hud` | `…/RetroFPSHud` | `.png` and `.fnt`, without `hud/` |
| PSX Forest (StarkCrafts) | `PSX_Forest_Level_byStarkCrafts.zip` | `ThirdParty/StarkCrafts/PSXForest` | FBX and textures |
| PSX Day/Night HDRI | `PSX_Daysky_HDRI.zip`, `PSX_Nightsky_HDRI.zip` | `ThirdParty/PSXSkyHDRI` | `.hdr` |

Skipped: the Godot versions (`psx-fx-assets.zip`, `psx-look-assets.zip`, `retro-screen-fx-assets.zip`) and the rest of
the Retro FPS Starter Kit (duplicates of the packs above plus a Godot project).

## Import step by step

1. `.unitypackage`: `Assets > Import Package > Custom Package`, then move the folder from `Assets/<Pack>` to
   `Assets/ThirdParty/HeyHeyThere/<Pack>` (moving inside Unity keeps the GUIDs).
2. Remove demos: the `Demo/`, `DemoArt/`, `Scripts/Demo/` and `PSXSkyboxes/Materials/Demo/` folders, and the scripts
   `PropsDemo`, `LevelWalker`, `ArmsRange`, `RoomDemo`, `DemoStyle`, `DemoMaterials`, `SkyViewer` (they use the old
   `Input` class, while the project only has the Input System). Also remove the empty `RetroFPSTextures/Scripts/`.
3. Copy the raw files into the folders from the table. Set their textures to Point filter, no mipmaps, no compression;
   HUD graphics to Sprite.
4. Local fix in `PSXLook/Scripts/PSXLights.cs`, line 46: `FindObjectsByType<Light>(FindObjectsSortMode.None)` →
   `FindObjectsByType<Light>()` (obsolete API warning in Unity 6.6).
5. `Tools/Sandbox/Build PSX Sandbox Scene` builds `Assets/_Project/Scenes/SandboxPsx.unity`.

## GUIDs

Assets from `.unitypackage` files have fixed GUIDs, so committed prefabs and scenes resolve on every machine.
Raw files (the mannequin from the Animation Library) get random GUIDs on import. After importing, run the PSX scene
builder again: it sets up the mannequin import itself (Generic rig with an avatar, looping `idle`/`walk`/`run`) and
wires it in again.
