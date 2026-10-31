# Assety zewnętrzne

Kupione paczki **nie są w gicie**. Licencje (heyheythere, StarkCrafts) zabraniają udostępniania plików, a repo jest publiczne.
Cały folder `Assets/ThirdParty/` i katalog z archiwami `zips/` są w `.gitignore`. Każdy importuje paczki lokalnie z archiwów.

Kod gry nie kompiluje się przeciwko skryptom paczek. Prefaby i sceny w `_Project` odwołują się tylko do modeli, tekstur,
materiałów, animacji i shaderów. Bez paczek projekt się kompiluje, ale scena `SandboxPsx` ma brakujące referencje.

## Paczki

| Paczka | Źródło | Gdzie w projekcie | Co bierzemy |
|---|---|---|---|
| PSX Castle Kit 1.0.4 | `psx-castle-kit-assets.zip` → `unity/*.unitypackage` | `ThirdParty/HeyHeyThere/PSXCastleKit` | całość bez demo |
| PSX Dungeon Kit 1.0.5 | `psx-dungeon-kit-assets.zip` → `unity/` | `…/PSXDungeonKit` | całość bez demo |
| PSX Hell Temple Kit 1.0.4 | `psx-hell-temple-kit-assets.zip` → `unity/` | `…/PSXHellTempleKit` | całość bez demo |
| PSX Tech Base Kit 1.0.4 | `psx-tech-base-kit-assets.zip` → `unity/` | `…/PSXTechBaseKit` | całość bez demo |
| PSX Firearms 1.4.0 | `psx-firearms-assets.zip` → `unity/` | `…/PSXFirearms` | całość bez demo |
| PSX Melee Weapons 1.4.3 | `psx-melee-weapons-assets.zip` → `unity/` | `…/PSXMeleeWeapons` | całość bez demo |
| PSX Sci-Fi Weapons 1.0.1 | `psx-scifi-weapons-assets.zip` → `unity/` | `…/PSXSciFiWeapons` | całość bez demo |
| PSX Retro FPS Monsters 1.1.0 | `psx-retro-fps-monsters-assets.zip` → `unity/` | `…/PSXRetroFPSMonsters` | całość bez demo |
| PSX Survival Pickups 1.0.8 | `psx-survival-pickups-assets.zip` → `unity/` | `…/PSXSurvivalPickups` | całość bez demo |
| PSX Skyboxes 1.1.3 | `psx-skyboxes-assets.zip` → `unity/` | `…/PSXSkyboxes` | całość bez demo |
| Retro FPS Textures 1.0.1 | `retro-fps-textures-assets.zip` → `unity/` | `…/RetroFPSTextures` | tekstury i materiały |
| PSX FX 1.3.0 | `psx-fx-1.3.0.unitypackage` | `…/PSXFX` | sprite FX, prefaby, `SpriteFX.cs` |
| PSX Look 1.4.0 | `psx-look-1.4.0.unitypackage` | `…/PSXLook` | shadery PSX, `PSXScreen` na kamerę |
| Retro Screen FX 1.3.0 | `retro-screen-fx-1.3.0.unitypackage` | `…/RetroScreenFX` | efekt ekranu |
| PSX Animation Library Free | `psx-animation-library-free-assets.zip` (surowe) | `…/PSXAnimationLibraryFree` | `fbx/` → `Models/`, `textures/` → `Textures/` |
| PSX Character Creator Free | `psx-character-creator-free-assets.zip` (surowe) | `…/PSXCharacterCreatorFree` | `fbx/` → `Models/`, `textures/` → `Textures/` |
| Explosion FX | `explosion-fx-assets.zip` (surowe) | `…/ExplosionFX/Sheets` | `sheets/` |
| Retro FPS SFX | `retro-fps-starter-kit-assets.zip` → `addons/retro_fps_sfx` | `…/RetroFPSSfx` | tylko `.ogg` |
| Retro FPS HUD | `retro-fps-starter-kit-assets.zip` → `addons/retro_fps_hud` | `…/RetroFPSHud` | `.png` i `.fnt` bez `hud/` |
| PSX Forest (StarkCrafts) | `PSX_Forest_Level_byStarkCrafts.zip` | `ThirdParty/StarkCrafts/PSXForest` | FBX i tekstury |
| PSX Day/Night HDRI | `PSX_Daysky_HDRI.zip`, `PSX_Nightsky_HDRI.zip` | `ThirdParty/PSXSkyHDRI` | `.hdr` |

Pominięte: wersje pod Godota (`psx-fx-assets.zip`, `psx-look-assets.zip`, `retro-screen-fx-assets.zip`) i reszta Retro FPS Starter Kit
(duplikaty paczek powyżej plus projekt Godota).

## Import krok po kroku

1. `.unitypackage`: `Assets > Import Package > Custom Package`, potem przenieś folder z `Assets/<Paczka>` do
   `Assets/ThirdParty/HeyHeyThere/<Paczka>` (przenoszenie w Unity zachowuje GUID-y).
2. Usuń demo: foldery `Demo/`, `DemoArt/`, `Scripts/Demo/`, `PSXSkyboxes/Materials/Demo/` oraz skrypty `PropsDemo`, `LevelWalker`, `ArmsRange`,
   `RoomDemo`, `DemoStyle`, `DemoMaterials`, `SkyViewer` (używają starego `Input`, a projekt ma tylko Input System).
   Usuń też pusty `RetroFPSTextures/Scripts/`.
3. Surowe pliki skopiuj do folderów z tabeli. Teksturom ustaw filtr Point, bez mipmap i bez kompresji; grafikom HUD typ Sprite.
4. Lokalna poprawka w `PSXLook/Scripts/PSXLights.cs`, linia 46: `FindObjectsByType<Light>(FindObjectsSortMode.None)` →
   `FindObjectsByType<Light>()` (ostrzeżenie o przestarzałym API w Unity 6.6).
5. `Tools/Sandbox/Build PSX Sandbox Scene` buduje `Assets/_Project/Scenes/SandboxPsx.unity`.

## GUID-y

Assety z `.unitypackage` mają stałe GUID-y, więc commitowane prefaby i sceny trafiają w nie u każdego.
Pliki surowe (manekin z Animation Library) dostają losowe GUID-y przy imporcie. Po imporcie uruchom builder sceny PSX ponownie:
on sam ustawia import manekina (rig Generic z awatarem, pętle `idle`/`walk`/`run`) i podpina go na nowo.
