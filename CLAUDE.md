# CLAUDE.md — Gra o dzikach Alberta

Roguelike TPP (Risk of Rain 2 + Megabonk + D&D) z moimi znajomymi jako bohaterami.
Pełne zasady gry: `docs/GDD.md`. **GDD jest źródłem prawdy dla mechanik.** Jeśli kod i GDD się rozjeżdżają, zatrzymaj się i zapytaj, nie zgaduj.
Znane sprzeczności i nierozstrzygnięte kwestie: `docs/OPEN_QUESTIONS.md`. Sprawdź go przed pracą nad danym systemem.
**Na starcie sesji przeczytaj `docs/WORKLOG.md`**: aktualny stan, następny krok, pułapki.

Komunikacja ze mną: po polsku. Kod, nazwy, komentarze i commity: po angielsku.

---

## Stack

| Element | Wartość |
|---|---|
| Silnik | Unity 6.6 (`6000.6.7f1`), URP |
| Język | C# |
| System | CachyOS (Linux, Arch). System plików rozróżnia wielkość liter. |
| IDE | VS Code + rozszerzenie Unity |
| Most do edytora | Unity MCP (konsola, kompilacja, testy, sceny) |
| Docelowo | Co-op 2–4 graczy (Netcode for GameObjects lub FishNet, jeszcze nie wybrane) |

---

## Struktura projektu

```
Assets/_Project/
  Scripts/
    Core/          # czysty C#: RNG, skill checki k20, formuły, cechy. Bez MonoBehaviour.
    Gameplay/      # logika gry: walka, bronie, wrogowie, spawn, run, areny
    Data/          # definicje ScriptableObjectów (WeaponDef, GearDef, TrinketDef, TalentDef...)
    Presentation/  # wizualia: animacje, VFX, liczby obrażeń, audio
    UI/
    Hub/           # ośrodek, budynki, questy, meta-progresja
    Editor/        # narzędzia edytora, walidatory
  Tests/
    EditMode/
    PlayMode/
  Data/            # instancje ScriptableObjectów (.asset)
  Prefabs/
  Scenes/
  Art/
docs/
  GDD.md
  OPEN_QUESTIONS.md   # sprzeczności i luki do rozstrzygnięcia
```

Każdy folder w `Scripts/` ma własny `.asmdef`. Zależności idą tylko w dół:

```
Presentation, UI, Hub  →  Gameplay  →  Data  →  Core
Editor                 →  wszystko
Tests                  →  testowany moduł
```

`Core` nie zależy od niczego. `Gameplay` nie wie o `Presentation`. Nowa zależność w złą stronę = błąd architektury, zapytaj zanim ją dodasz.

---

## Zasady architektury

### 1. Logika oddzielona od prezentacji (pod co-op)
- Stan gry (HP, pozycje logiczne, ekwipunek, wyniki rzutów) żyje w `Gameplay`/`Core`.
- `Presentation` tylko słucha zdarzeń (`OnDamageDealt`, `OnCrit`, `OnSkillCheckResolved`, `OnLevelUp`, `OnViceUsed`, `OnBlackout`) i rysuje.
- Wejście gracza zamieniane jest na komendy (`MoveCommand`, `UseSkillCommand`), a dopiero komendy zmieniają stan. Nigdy `Input` bezpośrednio w logice walki.

### 2. Dane w ScriptableObjectach
- Bronie, ubrania, trinkety, talenty, wrogowie, boosty, ewolucje, sytuacje skill check = ScriptableObjecty w `Data/`.
- Każda definicja ma stabilne `string Id` (np. `weapon.sword`, `trinket.grandpas_die`). ID nigdy się nie zmienia po utworzeniu, bo zależą od niego zapisy meta-progresji.
- Żadnych liczb balansu wpisanych na sztywno w kod. Liczba balansu = pole w ScriptableObjecie albo w `GameBalance` (jeden asset ze stałymi globalnymi).

### 3. Fail loudly
- Każda definicja implementuje `IValidatable` i sprawdza się sama: brakujące referencje, puste/zduplikowane ID, wartości poza zakresem.
- Walidacja odpala się: w `OnValidate`, z menu `Tools/Validate All Data` oraz przy starcie gry (bootstrap).
- Błąd walidacji przy starcie = `throw` w buildzie deweloperskim i w edytorze. Gra nie może ruszyć z zepsutymi danymi.
- Nie łap wyjątków po to, żeby je uciszyć. Żadnego `catch { }`, żadnego `?.` maskującego brak referencji, która musi istnieć.
- Wymagane referencje z Inspectora: `[SerializeField] private`, sprawdzone w `Awake` z czytelnym komunikatem (`"{name}: missing WeaponDef reference"`).

### 4. Losowość
- **Nigdy `UnityEngine.Random` ani `System.Random` w logice gry.** Tylko `Core.Rng`.
- Jeden seed na run, osobne strumienie. Dodanie losowania w jednym systemie nie może zmienić wyników w innym.

| Strumień | Co losuje |
|---|---|
| `Rng.Combat` | Krytyki, efekty procentowe trinketów, losowy czar Alberta |
| `Rng.Loot` | Dropy, zawartość skrzyń, boosty z wrogów |
| `Rng.Cards` | Karty level-up, rerolle, talenty |
| `Rng.Shop` | Oferta Nocnego, opcje treningu |
| `Rng.SkillChecks` | Rzuty k20, przerzut naturalnej 1 z LCK |
| `Rng.Spawns` | Wrogowie, elity, rozmieszczenie sytuacji skill check |

- Nowe losowanie, które nie pasuje do tabeli = zapytaj, zanim dodasz strumień.
- `UnityEngine.Random` wolno tylko w `Presentation` (rozrzut cząsteczek, wybór animacji śmiesznej porażki).

### 5. Walka (GDD, sekcja 5)
- Trafienia zawsze trafiają. W walce **nie ma rzutów k20**.
- Krytyk = szansa procentowa z `Rng.Combat` (bazowo 5% + LCK + trinkety), mnożnik bazowo ×2. Wartości bazowe w `GameBalance`.

### 5a. Ruch (GDD, sekcja 3)
- Ruch liczony w stałym ticku (`FixedUpdate`) z komend, nie bezpośrednio z inputu. Stan ruchu (prędkość, ładunki dasha, liczba skoków, wall run) żyje w `Gameplay`.
- Kontroler kinematyczny, nie fizyczny `Rigidbody` ani goły `CharacterController`. Kolizje: OpenKCC (MIT, rdzeń w `Assets/Plugins/OpenKCC`, nasza kopia do poprawiania); logika ruchu własna w `Gameplay`, OpenKCC tylko przesuwa kapsułę.
- Wszystkie parametry ruchu (prędkości, czasy, limity) w `GameBalance` lub definicji postaci, nie w kodzie.
- Modyfikatory ruchu z trinketów (liczba skoków, ładunki dasha) idą przez ten sam system modyfikatorów co cechy, nie przez `if` na konkretny przedmiot.

### 5b. Używki i promile (GDD, sekcja 7)
- Używka (`Vice`) to definicja w `Data/`: buff, czas trwania, cooldown, przyrost promili, efekt urwanego filmu, profil efektu ekranowego. Zero logiki konkretnej postaci w kodzie ogólnym.
- Promile (`Intoxication`) to stan postaci w `Gameplay`: 0–100%, spadek w czasie, przekroczenie 100% wyzwala urwany film (`Blackout`), potem reset do 50%. Kac (`Hangover`) jako debuff po końcu buffu.
- Efekty ekranowe (kołysanie, filtry, slow-motion, audio) tylko w `Presentation`, sterowane stanem promili i zdarzeniami. Zawsze przemnożone przez ustawienie intensywności (0–100%).
- Slow-motion Sir Seby to efekt wizualny lub lokalny, nie zmiana `Time.timeScale` dla całej symulacji (co-op).
- Toast (co-op): wykrywany w `Gameplay` z okna czasowego w `GameBalance`.

### 6. Skill checki k20 (GDD, sekcja 4)
- Wszystkie rzuty k20 przechodzą przez jeden serwis `Core.SkillCheck`. Nigdzie indziej nie licz k20.
- Kolejność: advantage i disadvantage się znoszą (nie stackują) → rzut 1 lub 2 k20, wybór wyższej/niższej → naturalne 1 i szansa na przerzut z LCK (+5% za każdy +1 LCK, wynik przerzutu ostateczny) → wynik = naturalny + modyfikator cechy + biegłość → porównanie z DC.
- Naturalne 20 = zawsze sukces (wzmocniona nagroda). Naturalne 1 = zawsze porażka.
- Wynik zwracany jako struct (naturalny, końcowy, sukces, nat20, nat1), bez alokacji.
- Gra jest wstrzymana na czas rzutu; jedna próba na sytuację, potem sytuacja zablokowana.

### 7. Cechy i skalowanie (GDD, sekcja 5)
- 7 cech: STR, DEX, CON, INT, WIS, CHA, LCK. Zapisane **wprost jako modyfikatory** (int, start 0, mogą być ujemne, bez limitu). Żadnych wartości typu 10/12 ani przeliczania (X−10)/2.
- Formuły skalowania w jednym miejscu: `Core.StatFormulas`.
- Broń deklaruje cechę skalującą w swojej definicji. Talenty mogą ją podmienić (np. Wizard skaluje czary ze STR) przez modyfikator, nie przez `if (class == Wizard)` w kodzie broni.
- Formuły mają testy w `Tests/EditMode`. Zmiana formuły = zmiana testu tylko za moją zgodą.

---

## Wydajność

Cel: stabilne 60 FPS przy 300+ wrogach.

- Wrogowie, pociski, liczby obrażeń, VFX: **object pooling**. Żadnego `Instantiate`/`Destroy` w trakcie walki.
- W `Update`/`FixedUpdate` i innych gorących ścieżkach: zero alokacji. Bez LINQ, bez `GetComponent`, bez `Find*`, bez konkatenacji stringów, bez lambd tworzonych na klatkę.
- Ruch hord: flow field, nie NavMesh per wróg.
- Optymalizacje typu Jobs/Burst tylko po pomiarze w Profilerze i po uzgodnieniu ze mną.

---

## Konwencje kodu

- Namespace = folder: `Game.Core`, `Game.Gameplay.Combat`, `Game.Data`...
- `PascalCase` dla typów i metod publicznych, `_camelCase` dla pól prywatnych, `camelCase` dla lokalnych.
- Pola dla Inspectora: `[SerializeField] private`, nigdy publiczne pola.
- Bez statycznych singletonów. Serwisy rejestruje bootstrap i przekazuje je jawnie.
- Klasa robi jedną rzecz. Plik ponad ~300 linii to sygnał, żeby zapytać o podział.
- Komentarze tylko tam, gdzie kod nie mówi „dlaczego”.

---

## Twarde zakazy

- **Nie edytuj ręcznie** plików `.unity`, `.prefab`, `.asset`, `.meta`, `ProjectSettings/`. Sceny i prefaby zmieniaj przez Unity MCP albo skrypt edytora.
- **Nie zmieniaj ani nie usuwaj testów**, żeby przeszły. Jeśli test jest zły, powiedz mi dlaczego.
- **Nie dodawaj pakietów** (UPM, NuGet, Asset Store) bez pytania.
- **Nie rób commitów** bez prośby. Na koniec zadania zaproponuj treść commita.
- **Nie zmieniaj ID** istniejących definicji danych.
- Nie zostawiaj `TODO` bez zgłoszenia ich mi w podsumowaniu.

---

## Pętla pracy

1. Przy zadaniu na więcej niż 2–3 pliki: najpierw krótki plan (pliki, klasy, zależności), czekaj na akceptację.
2. Pisz kod i testy EditMode dla logiki w `Core` i `Gameplay`.
3. Po zmianach: sprawdź kompilację i konsolę przez Unity MCP, uruchom testy.
4. Zero błędów i ostrzeżeń kompilatora przed zgłoszeniem, że gotowe.
5. Podsumowanie: co zmieniłeś, co mam sprawdzić w Play Mode, propozycja commita.
6. Przy każdym commicie zaktualizuj `docs/WORKLOG.md` w tym samym commicie: popraw sekcję „Teraz” i dopisz jedną linię na górę „Historii” (data i co, bez hasha). Nie przepisuj całego pliku, edytuj tylko te miejsca. Starsze wpisy ponad ok. 10 usuwaj. „Mapę” i „Pułapki” zmieniaj tylko wtedy, gdy przestały być prawdziwe albo odkryłeś coś, co oszczędzi czas następnej sesji.

Testy bez edytora (gdy MCP nie działa):

```bash
/media/main/Projects/Unity/Installs/6000.6.7f1/Editor/Unity \
  -batchmode -nographics -projectPath . \
  -runTests -testPlatform EditMode \
  -testResults ./TestResults/editmode.xml -logFile -
```

Uwaga: tryb batch nie zadziała, gdy ten sam projekt jest otwarty w edytorze.

---

## Słownik (GDD po polsku → kod po angielsku)

| GDD | Kod |
|---|---|
| Cecha (STR, DEX, ..., LCK) | `Ability`, wartość: `AbilityModifier` (int) |
| Talent | `Talent` |
| Boost | `Boost` (pickup czasowy) |
| Ubranie (slot ekwipunku) | `Gear`, slot: `GearSlot` |
| Trinket | `Trinket` (stackuje się, `StackCount`) |
| Karta level-up / reroll | `LevelUpCard` / `Reroll` |
| Broń aktywna / pasywna | `ActiveWeapon` / `PassiveWeapon` |
| Ewolucja broni | `WeaponEvolution` |
| Pasywka klasowa | `ClassPassive` |
| Używka / Promile | `Vice` / `Intoxication` |
| Kac / Urwany film / Toast | `Hangover` / `Blackout` / `Toast` |
| Skill check / sytuacja na mapie | `SkillCheck` / `SkillCheckSite` |
| Biegłość | `Proficiency` |
| Trudność checku | `Dc` |
| Nat 20 / Nat 1 | `Nat20` / `Nat1` |
| Rzadkość | `Rarity` (`Common`, `Uncommon`, `Rare`, `Epic`, `Legendary`) |
| Nocny (sklep między arenami) | `Shop` (scena `Nocny`) |
| Rezygnacja z runu | `CashOut` |
| Ośrodek | `Hub` |
| Run / Arena / Biom | `Run` / `Arena` / `Biome` |
| Portal końca areny | `ArenaPortal` |
| Złoto / Piwo | `Gold` / `Beer` |

## Postacie

| Postać | Klasa | Cechy główne (+2) | Biegłości | Pasywka | Używka (F) |
|---|---|---|---|---|---|
| Ada | Ranger | DEX, WIS | Zwierzęta, percepcja | Pies Loki | Dzikówka: Zew Dzika |
| Bar | Barbarzyńca | STR, CON | Siłowanie, picie | Szał | Piwo: Szał Piwny |
| Sir Seba | Rycerz | STR, CHA | Perswazja, występy | Honor | Wódka: Odwaga z Butelki |
| Albert | Wizard | INT, WIS | Zagadki, magiczne mechanizmy | Księga Czarów | Papierosy: Dymek |

Sterowanie: LPM atak, PPM skill broni aktywnej, F używka, E interakcja, Shift dash, Spacja skok, Ctrl ślizg. Bronie pasywne nie mają skilli.
