# Worklog

Krótki stan pracy dla agenta na starcie sesji. Szczegóły są w git log i w `docs/OPEN_QUESTIONS.md` (sekcja „Rozstrzygnięte”).
Zasady aktualizacji: CLAUDE.md, sekcja „Pętla pracy”.

## Teraz

- Gałąź `dev`, wszystko wypchnięte na `origin/dev`.
- Ruch: etapy 1 i 2 gotowe (bieg z pędem, skok, coyote time, bufor, dash, ślizg, slide-jump). Czeka na playtest użytkownika w scenie `MovementTest`.
- Następny krok: etap 3 ruchu (wall run, wall jump, mantle). Najpierw plan do akceptacji. Ściana do testów już stoi w `MovementTest` (x=40).

## Mapa ruchu

- Logika: `Scripts/Gameplay/Movement/` (`LocomotionSimulator` + `DashState` + `SlideState`, czysty C#; `CharacterLocomotion` łączy to z OpenKCC).
- Wejście: `PlayerInputReader` → komendy → `PlayerMotor` (FixedUpdate).
- Parametry: `GameBalance._movement` (`MovementTuning`, `DashTuning`, `SlideTuning`), wartości startowe wpisuje `SandboxDataBuilder.EnsureMovementTuning`, tylko w puste bloki.
- Kolizje: `Assets/Plugins/OpenKCC` (nasza kopia, bez alokacji, jawny deltaTime).
- Wizualia: `TransformInterpolator` (wygładzanie między tickami), `SlidePose` (odchylenie o 30°), `SlideAnimator` (`crouch_idle`).
- Sceny buduje się z menu `Tools/Sandbox/...`: Sandbox, PSX Sandbox, Movement Test. Sceny przebudowuje się po zmianach w builderach.

## Pułapki

- Po kompilacji edytor przeładowuje domenę i Unity MCP rozłącza się na kilkanaście sekund: odczekaj i ponów wywołanie.
- Wynik `test_run` dla wszystkich testów jest duży: filtruj (`filter_type: assembly`, `filter: Game.Tests.EditMode`) i czytaj tylko `Summary`.
- Przebudowa scen przegenerowuje identyfikatory w `Enemy.prefab` i `AC_Albert.controller`. To szum, nie cofaj go, bo sceny linkują do nowych identyfikatorów.
- Pliki C# są różne: część z BOM, część bez. Przy edycji skryptem zachowuj to, co było.

## Historia (najnowsze na górze, max ok. 10 wpisów)

- 2026-11-01: worklog i zasada jego aktualizacji w CLAUDE.md.
- 2026-11-01 `e2a87a7`: ślizg do zera i kucanie w miejscu, zsuwanie ze stoku, HUD prędkości, half-pipe, MovementTest bez wrogów.
- 2026-11-01 `bf26cd5`: etap 2 ruchu (dash, ślizg, slide-jump), scena MovementTest.
- 2026-11-01 `01b4bad`: etap 1 ruchu na OpenKCC (bieg, skok, interpolacja modelu).
- 2026-11-01 `ca82f01`: porządki w OpenKCC (zero alokacji, testy regresji).
