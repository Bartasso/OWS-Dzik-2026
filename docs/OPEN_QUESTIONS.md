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

- [ ] **Pełna formuła obrażeń**: kolejność i łączenie składników (baza broni × poziom broni × cecha × trinkety × talenty × krytyk). Bonusy z różnych źródeł sumują się czy mnożą? Czy wrogowie mają pancerz/odporności?
- [ ] **Skrajne i ujemne wartości**: STR −25 = −125% obrażeń, INT +50 = −100% cooldownu, ujemna szybkość ataku, ujemna szansa na krytyk. Clamp (min/max w `GameBalance`) czy malejące zyski?
- [ ] **Brakujące liczby**: bazowe HP i regeneracja (CON), efekty WIS (leczenie, Loki, zasięg XP), wydłużenie boostów (CHA), odrzut (STR), wpływ LCK na rzadkość kart/dropów i na efekty procentowe trinketów (dodatkowy rzut jak w RoR2 czy +X%?).
- [ ] **Kategorie broni**: bonus z cechy dotyczy kategorii (siłowe, zręcznościowe, arkaniczne, natura/boskie, bardowskie), a „Pięść Mędrca” zmienia skalowanie „czarów”. Propozycja: broń ma tag kategorii + cechę skalującą. Którą cechą skaluje się „miecz i tarcza”?
- [ ] **Stackowanie trinketów**: dokładne krzywe (liniowa, hiperboliczna `1 − 1/(1 + k·n)`?).
- [ ] **Ubrania**: ile cechy daje typ i jak rośnie z poziomem; typ może dawać kilka statystyk (Klapki). Przy 2 pierścieniach gracz wybiera, który wymienić?
- [ ] **Wartości tymczasowe do M0**: max poziom broni i ubrań, reroll kart, krzywa XP, wzrost trudności w czasie.

## Skill checki (`Core.SkillCheck`, M1)

- [ ] **Kanoniczna lista umiejętności**: GDD wymienia m.in. podnoszenie, skoki, wytrzymałość, które nie są niczyją biegłością. Propozycja: `SkillDef` z przypisaną cechą, sytuacja wskazuje umiejętność.
- [ ] **Przerzut z LCK przy advantage**: jedna kość czy znowu dwie? Propozycja: przerzut tylko gdy wybrany wynik = 1, jedną kością, szansa przycięta do 0–100%.
- [ ] **DC od numeru areny**: formuła. Wartość biegłości (+2) do `GameBalance`.
- [ ] **Pauza w co-op (M2)**: czy gra staje u wszystkich? Propozycja: pauza jako stan symulacji, nie `Time.timeScale`.

## Architektura

- [ ] **Efekty talentów i trinketów**: SO w `Data` nie mogą znać `Gameplay`. Propozycja: system modyfikatorów i hooków w `Core`, efekty jako `[SerializeReference]` w `Data`. Najważniejsza decyzja przed M0.
- [ ] **Wstrzykiwanie serwisów** do obiektów ze sceny i z poolingu bez singletonów: ręczne `Init(...)` od bootstrapu czy prosty kontener?
- [ ] **Zdarzenia**: C# `event` ze structami czy kanały SO?
- [ ] **Tick symulacji** pod co-op (`FixedUpdate`?) i czy `Gameplay` może używać CharacterController/Rigidbody.
- [ ] **Walidacja w buildzie release**: log, ekran błędu?
- [ ] **Wczytywanie `GameBalance`**: referencja z bootstrapu, `Resources`, Addressables (nowy pakiet)?
- [ ] **Zapis meta-progresji**: format, lokalizacja, wersjonowanie.
- [ ] **Flow field na terenie 3D**: rozdzielczość, wysokości/piętra, separacja wrogów przy 300+.
- [ ] **Lokalizacja tekstów**: polski na sztywno czy Unity Localization (nowy pakiet)?
- [ ] **Gapior „odwraca sterowanie”**: propozycja: modyfikator na warstwie komend, nie na Input.

---

## Rozstrzygnięte

- 2026-10-29: Klawisz E = interakcja (skill checki, skrzynie, portal, Nocny). Skille broni pasywnych przeniesione na Q, R, C (GDD sekcja 3).
