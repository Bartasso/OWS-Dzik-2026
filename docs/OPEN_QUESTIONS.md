# Otwarte pytania implementacyjne

Sprzeczności i luki znalezione przy czytaniu `CLAUDE.md` i `docs/GDD.md` (2026-10-29).
Do rozstrzygnięcia, gdy dojdziemy do danego systemu. Po decyzji: wpisz ją do GDD/CLAUDE.md i przenieś punkt do „Rozstrzygnięte”.

Pytania designowe (biom MVP, listy contentu, boss, portal, fabuła) są w GDD, sekcja 13.

---

## Sprzeczności w dokumentach

- [ ] **„Rozdane cechy”**: GDD sekcja 5 mówi, że nie ma ręcznego rozdawania punktów, a tabela meta-progresji (sekcja 9) wymienia „rozdane cechy” jako resetowane. Do tego Nocny sprzedaje „dodatkowe punkty cech”: czy gracz wybiera wtedy cechę ręcznie?
- [ ] **„Przedmiot”** to stary termin: ewolucje („broń plus przedmiot”), Kuźnia, meta-progresja („pula broni i przedmiotów”). Chodzi o ubranie (`Gear`), trinket (`Trinket`) czy oba?
- [ ] **Nat 1 w skill checku**: debuff z katastrofy to gameplay, a CLAUDE.md daje `UnityEngine.Random` do „wyboru animacji śmiesznej porażki”. Propozycja: efekt zapisany w SO sytuacji, losowa tylko animacja.
- [ ] **Brakujące strumienie RNG**: nagroda ze skill checku (`Loot` czy `SkillChecks`?), losowe decyzje AI wrogów, fazy bossa.

## Walka i formuły (`Core.StatFormulas`)

- [ ] **Pełna formuła obrażeń**: kolejność składników (baza broni × poziom broni × cecha × trinkety × talenty × krytyk). Łączenie bonusów statystyk jest rozstrzygnięte (kubełki), ale pipeline trafienia nie. Czy wrogowie mają pancerz/odporności? Czy cecha skalująca (STR −25 = −125%) też potrzebuje clampa?
- [ ] **Brakujące liczby**: bazowe HP i regeneracja (CON), efekty WIS (leczenie, Loki, zasięg XP), wydłużenie boostów (CHA), odrzut (STR), wpływ LCK na rzadkość kart/dropów.
- [ ] **Miecz i tarcza**: którą cechą i kategorią skaluje się startowa broń Sira Seby?
- [ ] **Wartości krzywych stackowania** dla konkretnych trinketów (mechanizm już jest: `Linear` / `Hyperbolic`).
- [ ] **Ubrania**: ile cechy daje typ i jak rośnie z poziomem; typ może dawać kilka statystyk (Klapki). Przy 2 pierścieniach gracz wybiera, który wymienić?
- [ ] **Wartości tymczasowe do M0**: max poziom broni i ubrań, reroll kart, krzywa XP, wzrost trudności w czasie.

## Skill checki (`Core.SkillCheck`, M1)

- [ ] **DC od numeru areny**: formuła. Wartość biegłości (+2) do `GameBalance`.
- [ ] **Pauza w co-op (M2)**: czy gra staje u wszystkich? Propozycja: pauza jako stan symulacji, nie `Time.timeScale`.

## Architektura

- [ ] **Efekty-zachowania i klasowe**: aura spowolnienia (Skarpety do Sandałów), Loki, Szał Piwny. Potrzebują prefabów lub interfejsów skilli klasowych w `IEffectContext`.
- [ ] **Tick symulacji** pod co-op (`FixedUpdate`?) i czy `Gameplay` może używać CharacterController/Rigidbody.
- [ ] **Walidacja w buildzie release**: log, ekran błędu?
- [ ] **Zapis meta-progresji**: format, lokalizacja, wersjonowanie.
- [ ] **Flow field na terenie 3D**: rozdzielczość, wysokości/piętra, separacja wrogów przy 300+.
- [ ] **Lokalizacja tekstów**: polski na sztywno czy Unity Localization (nowy pakiet)?
- [ ] **Gapior „odwraca sterowanie”**: propozycja: modyfikator na warstwie komend, nie na Input.

---

## Rozstrzygnięte

- 2026-10-29: Klawisz E = interakcja (skill checki, skrzynie, portal, Nocny). Skille broni pasywnych przeniesione na Q, R, C (GDD sekcja 3).
- 2026-10-29: **Przerzut z LCK przy advantage/disadvantage**: przerzut tylko gdy *wybrany* wynik = 1, jedną kością, szansa przycięta do 0–100%. Wynik przerzutu ostateczny; przerzut na 20 liczy się jako Nat20.
- 2026-10-29: **Wczytywanie `GameBalance`**: referencja z bootstrapu w Inspectorze (bez `Resources` i Addressables). Jeden asset w `Assets/_Project/Data/`.
- 2026-10-29: **`Game.Core` ma `noEngineReferences: true`**: kompilator pilnuje, że Core to czysty C#.
- 2026-10-29: **System modyfikatorów i hooków**: `StatSheet` w `Core` z kubełkami `(baza + Σflat) × (1 + Σ%add) × Π(1 + %mult)`, potem clamp. Efekty trinketów/talentów to klasy `[SerializeReference]` w `Data` implementujące interfejsy z `Core` (`IEffect`, `ITriggerHandler`); hooki przez `TriggerBus`.
- 2026-10-29: **Cechy i statystyki skill checków są całkowite**: przyjmują tylko modyfikatory `Flat` o całych wartościach.
- 2026-10-29: **Skrajne wartości**: clamp wyniku końcowego per statystyka, limity w `GameBalance` (`_statLimits`). Malejące zyski tylko przez krzywą `Hyperbolic` trinketów.
- 2026-10-29: **LCK a efekty procentowe**: `szansa × (1 + LCK × 5%)`, clamp 0–100%, mnożnik w `GameBalance`. Jedyne miejsce: `IEffectContext.RollProc` (strumień `Rng.Combat`).
- 2026-10-29: **Stackowanie trinketów**: krzywe `Linear` (`wartość × n`) i `Hyperbolic` (`1 − 1/(1 + k·n)`).
- 2026-10-29: **Kategorie broni**: broń ma `WeaponCategory` (Strength, Dexterity, Arcane, Nature, Bardic) i cechę skalującą; talenty nadpisują cechę per kategoria w `ScalingTable`.
- 2026-10-29: **Formuła skill checka**: `wynik = naturalny + cecha + biegłość + SkillCheckBonus`. Advantage/disadvantage, gdy statystyki `SkillCheckAdvantage`/`Disadvantage` > 0.
- 2026-10-30: **Umiejętności**: `SkillDef` (nazwa + cecha), postać ma listę biegłości (`SkillDef`), sytuacja (`SkillCheckSiteDef`) wskazuje umiejętność. Pełna lista umiejętności na MVP nadal do ustalenia.
- 2026-10-30: **Wstrzykiwanie serwisów**: ręczne `Init(...)` z `GameBootstrap`; komponenty sceny startują wyłączone i włączają się w `Init`.
- 2026-10-30: **Zdarzenia**: C# `event` ze structami (`CombatEvents`), tylko Gameplay → Presentation/UI.
- 2026-10-30: **Tymczasowa formuła obrażeń (M0, sandbox)**: `baza broni × max(0, 1 + 5% × cecha skalująca)`, krytyk `× max(1, bazowy mnożnik + bonus)`. Pełny pipeline nadal otwarty (punkt wyżej).
- 2026-10-30: **Nat 20 w skill checku**: nagroda × `GameBalance._nat20RewardCopies` (na razie 2).
