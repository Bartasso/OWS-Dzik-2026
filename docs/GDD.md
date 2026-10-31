# Gra o dzikach Alberta — Game Design Document

Stan na: 2026-10-26 · autor: Michael

## 1. Wizja

**Pitch:** szybki roguelike TPP, w którym moi znajomi jako bohaterowie D&D siekają hordy sparodiowanych potworów w świecie modern-fantasy, a k20 rozstrzyga szalone sytuacje na mapie. Run trwa do 45 minut, między runami rozbudowujemy ośrodek wypoczynkowy, do którego trafiają uratowani przyjaciele.

**Tytuł roboczy:** Gra o dzikach Alberta. - OWS Dzik

### Filary

| Filar | Co znaczy w praktyce |
| --- | --- |
| Dziwne buildy | Każda broń dostępna dla każdej klasy, talenty zmieniają skalowanie (np. Wizard pod STR), trinkety stackują się bez limitu. Ewolucje broni do odkrycia. |
| Kość w kluczowych momentach | k20 toczy się na ekranie w skill checkach na mapie (kot na drzewie, przewrócony wóz). Nat 20 i nat 1 to widowisko. |
| Kumple i humor | Grywalni są prawdziwi znajomi, świat jest pełen inside joków i parodii D&D. |
| Lekko i szybko | Niskie wymagania sprzętowe, setki wrogów na ekranie, płynne 60 FPS. |

### Inspiracje

| Źródło | Co bierzemy |
| --- | --- |
| Risk of Rain 2 | Kamera TPP, struktura areny (eksploracja, teleporter, boss), trudność rosnąca z czasem, trinkety stackujące się bez limitu |
| Megabonk | Hordy, bronie działające automatycznie, szybki level up z wyborem kart |
| Brotato | Sklep i trening cech między falami, przedmioty z trade-offem (duży plus za minus) |
| D&D / Pathfinder | Cechy, k20 i skill checki, biegłości, talenty (featy), klasy, potwory |

### Wyróżnik

Trzy rzeczy naraz, których nie ma konkurencja: **skill checki k20 na mapie**, **prawdziwi znajomi jako bohaterowie** oraz **ośrodek-hub**, który rośnie wraz z uratowanymi postaciami.

## 2. Pętla rozgrywki

Gra ma dwie pętle: krótką w runie (walcz, wybieraj karty, pokonaj bossa, zrób zakupy w Nocnym) i długą w ośrodku, która między runami daje trwałe postępy.

**W runie (do 45 min):**
Arena (walka, XP, złoto, skill checki k20) → Level-up (wybór 1 z 3 kart, talent co 5 poziomów) → Portal i boss (koniec areny) → Nocny (sklep i trening albo rezygnacja) → kolejna arena (3–4 na run).

**W ośrodku (między runami):**
Koniec runu → Odbierz waluty (złoto i piwo) → Rozbudowuj (budynki i stałe ulepszenia) → Questy (wyzwania od znajomych) → Wybierz postać (uratowani znajomi) → nowy run.

Run kończy się na trzy sposoby: śmiercią, rezygnacją w Nocnym albo pokonaniem ostatniego bossa. Każdy daje inną nagrodę dla ośrodka (sekcja 9), a tylko wygrana może uwolnić kolejnego znajomego.

## 3. Sterowanie i walka

Kamera TPP zza pleców jak w Risk of Rain 2. Ruch jest szybki i oparty na momentum, a broń strzela normalnie w każdym stanie ruchu (ślizg, powietrze, ściana). Walka jest hybrydą: jedna broń aktywna, którą celujesz, plus bronie pasywne działające same. Układ do weryfikacji w playtestach.

### Ruch

Ruch jest wspólny dla wszystkich postaci (bez ruchu klasowego) i jest główną obroną przed hordą. Wartości liczbowe to propozycja startowa do balansu.

| Mechanika | Działanie |
| --- | --- |
| Bieg | Zawsze, bez staminy i bez klawisza sprintu. Szybkość rośnie z DEX. |
| Skok | Pojedynczy bazowo. Kolejne skoki tylko z trinketów (stackują się: 2, 3, … skoki). Coyote time i bufor wciśnięcia skoku. |
| Dash | Bazowo dla wszystkich: 1 ładunek, odnowienie ok. 2 s, i-frame'y ok. 0,2 s, działa też w powietrzu (jeden na skok). |
| Slide | Kucnięcie w biegu. Utrzymuje prędkość, w dół zbocza przyspiesza, pod górę hamuje. |
| Slide-jump | Skok ze ślizgu zachowuje prędkość; dobrze zgrane serie podbijają ją do miękkiego limitu. |
| Wall run | Bieg po ścianie do ok. 1,5 s. Tej samej ściany nie można użyć ponownie bez dotknięcia ziemi lub innej ściany. |
| Wall jump | Odbicie od ściany, przywraca jeden skok w powietrzu. |
| Mantle | Automatyczne wciąganie się na krawędź po doskoczeniu. |
| Kontrola w powietrzu | Ograniczona: można skręcać, nie można zahamować w miejscu. |
| Obrażenia od upadku | Brak. |

**Ulepszenia ruchu** pochodzą z trinketów i kart: kolejne skoki, ładunki dasha, szybsze odnowienie, efekty dasha (zostawia ogień, rani przy przejściu przez wrogów, odnawia się przy zabiciu).

**Rozpęd:** sama prędkość nie zadaje obrażeń. Robią to tylko specjalne przedmioty, np. kolczasta zbroja, która przy szybkim biegu taranuje wrogów.

**Feedback prędkości:** FOV rośnie z prędkością, speed lines, wiatr w audio.

**Parkour w biomach:** Zaspa daje trzepaki, dachy garaży i balkony do wall runów; Las daje zbocza do ślizgów i grzyby-trampoliny.

### Sterowanie i sloty broni

| Klawisz | Akcja |
| --- | --- |
| LPM | Atak bronią aktywną (1 slot) |
| PPM | Skill broni aktywnej |
| F | Używka postaci: buff z długim cooldownem (sekcja 7) |
| E | Interakcja (skill checki, skrzynie, portal, sklep w Nocnym) |
| Shift | Dash |
| Spacja | Skok |
| Ctrl | Ślizg |

Bronie pasywne (3 sloty) działają automatycznie (orbita, aura, pociski, wieżyczka) i nie mają własnych skilli. Mniej klawiszy, więcej uwagi na ruch.

### Bronie a klasy

- Każda postać może użyć każdej broni.
- Broń skaluje się z przypisaną cechą (miecz ze STR, łuk z DEX, różdżka z INT, ukulele z CHA).
- Klasa nie zabrania broni. Daje talenty, które mogą zmienić skalowanie, np. talent Wizarda sprawiający, że czary skalują się ze STR. Dziwne buildy są celem, nie błędem.

### Ewolucje broni

- Konkretna para broni (lub broń plus przedmiot) na wystarczającym poziomie ewoluuje w nową, silniejszą broń.
- Ewolucje są **do odkrycia**: gra ich nie podaje. Odkryta ewolucja trafia do Księgi Ewolucji w hubie.
- Wskazówki do nieodkrytych ewolucji dają questy i NPC w ośrodku.
- Przykład roboczy: miecz + tarcza → szarżujący bastion; pochodnia + wachlarz → ogniste tornado.

### Poziom broni

Nowe bronie i kolejne poziomy broni wybierasz z kart na level-upie (sekcja 5). Karta nowej broni pojawia się tylko wtedy, gdy masz wolny slot. Maksymalny poziom broni to warunek ewolucji.

## 4. Skill checki (k20)

Kość nie bierze udziału w zwykłej walce: trafienia zawsze trafiają, a krytyk to zwykła szansa procentowa (sekcja 5). k20 toczy się na ekranie w skill checkach, czyli śmiesznych sytuacjach rozsianych po arenie.

### Przebieg

1. Podejdź do sytuacji i wciśnij E. Gra się zatrzymuje.
2. Na ekranie toczy się duża k20. Wynik = rzut + modyfikator cechy + biegłość klasowa (jeśli dotyczy).
3. Wynik porównany z DC sytuacji, efekt, gra wraca.
4. Jedna próba. Po rzucie sytuacja blokuje się w komiczny sposób (kot ucieka na wyższą gałąź, wóz rozpada się na kawałki).

### Wyniki

| Wynik | Efekt |
| --- | --- |
| Naturalne 20 | Zawsze sukces, nagroda wzmocniona (rzadszy trinket, mocniejszy buff), fanfara |
| Sukces (wynik ≥ DC) | Nagroda: buff do końca areny, trinket, złoto albo piwo |
| Porażka | Nic albo drobny, śmieszny minus |
| Naturalne 1 | Zawsze porażka i śmieszna katastrofa (kot drapie i daje krótki debuff, głaz spada na stopę) |

### Zasady

| Zasada | Działanie |
| --- | --- |
| Modyfikator | Wartość cechy wprost, np. STR +3 (sekcja 5) |
| Biegłość klasowa | Każda postać ma 2 rodzaje checków z bonusem (+2, wartość do balansu) |
| Szczęście | Każdy +1 LCK daje +5% szansy na automatyczny przerzut naturalnej 1. Wynik przerzutu jest ostateczny. |
| Advantage / disadvantage | Z trinketów, talentów i debuffów. Advantage: 2 k20, bierzesz wyższą. Disadvantage: niższą. Nie stackują się, a jedno i drugie naraz się znosi (jak w D&D 5e). |
| DC | Rośnie z numerem areny, żeby wysokie modyfikatory w late game nie dawały automatycznego sukcesu |
| Częstotliwość | 1–3 sytuacje na arenę, część na uboczu, żeby nagradzać eksplorację |

### Przykłady sytuacji

| Sytuacja | Cecha | Biegłość |
| --- | --- | --- |
| Ściągnij kota z drzewa | WIS (zwierzęta) | Ada |
| Podnieś przewrócony wóz, spod którego coś wystaje | STR (siłowanie) | Bar |
| Przechytrz zaklęty automat z napojami | INT (magiczne mechanizmy) | Albert |
| Wyperswaduj goblinowi lepszą cenę | CHA (perswazja) | Sir Seba |
| Przeskocz dziurę w moście po skarb | DEX (akrobatyka) | — |
| Wypij kufel z krasnoludem na czas | CON (picie) | Bar |
| Ruletka u podejrzanego handlarza | LCK (hazard) | — |
| Babcia na ławce na Zaspie: buff za ploteczki | CHA (perswazja) | Sir Seba |
| Gołębie kradnące loot | DEX (akrobatyka) | — |

## 5. Cechy i rozwój postaci

Siedem cech zapisanych jako same modyfikatory: postać nie ma „12 siły”, tylko STR +1. Każda cecha startuje od 0, klasa daje +2 do każdej z dwóch cech głównych. Wartości nie mają górnego limitu i mogą być ujemne (trinkety z minusem). Nie ma ręcznego rozdawania punktów co poziom.

### Cechy

Wartości są propozycją startową do balansu. Ujemna cecha działa odwrotnie (STR −1 = −5% obrażeń broni siłowych).

| Cecha | Za każdy +1 | W skill checkach |
| --- | --- | --- |
| STR (Siła) | +5% obrażeń broni siłowych (miecze, topory, młoty), większy odrzut | Siłowanie, podnoszenie |
| DEX (Zręczność) | +5% obrażeń broni zręcznościowych (łuki, sztylety), +2% szybkości ataku wszystkich broni, +1% szybkości ruchu | Akrobatyka, skoki |
| CON (Kondycja) | +8% max HP, +regeneracja HP | Picie, wytrzymałość |
| INT (Intelekt) | +5% obrażeń czarów arkanicznych, −2% cooldownu skilli | Zagadki, magiczne mechanizmy |
| WIS (Mądrość) | +5% obrażeń broni natury i boskich, siła leczenia i towarzyszy (Loki), zasięg zbierania XP | Zwierzęta, percepcja |
| CHA (Charyzma) | +5% obrażeń broni bardowskich (ukulele, trąbka), dłuższe boosty, niższe ceny w Nocnym | Perswazja, występy |
| LCK (Szczęście) | +1% szansy na krytyk, częstsze efekty procentowe trinketów, rzadsze karty i dropy | Hazard, przerzut naturalnej 1 |

**Krytyk:** bazowo 5% szansy i ×2 obrażeń. Trinkety i talenty podnoszą szansę i mnożnik.

### Skąd biorą się cechy

- Ubrania: bonus zależny od typu, rośnie z poziomem ubrania (sekcja 6).
- Trinkety (sekcja 6).
- Karty na level-upie (karta ulepszenia cechy).
- Trening i zakupy w Nocnym między arenami (sekcja 8).

### Level-up

Każdy poziom to wybór 1 z 3 losowych kart, z możliwością rerolla (liczba lub koszt do ustalenia). Rzadkość kart rośnie z LCK. Cel: ok. 50 poziomu przy pełnym runie, szybkie pierwsze poziomy, wolniejsze później.

| Karta | Działanie |
| --- | --- |
| Nowa broń | Trafia do wolnego slotu; pojawia się tylko przy wolnym slocie |
| Poziom broni | +1 poziom posiadanej broni |
| Nowe ubranie | Do wolnego slotu albo zamiast obecnego (poziom starego przepada) |
| Poziom ubrania | +1 poziom posiadanego ubrania |
| Ulepszenie cechy | +1 do konkretnej cechy (rzadsza wersja: +2) |

### Talenty

- Co 5 poziomów (10 na run) osobny wybór 1 z 3 losowych talentów, z różną rzadkością.
- Dwie pule: **klasowe** (tylko dla danej klasy) i **ogólne** (dla wszystkich).
- Najciekawsze talenty łamią zasady: zmieniają skalowanie broni, łączą dwie cechy, przerabiają skill checki.
- Przykłady robocze: „Pięść Mędrca” (Wizard: czary skalują się ze STR zamiast INT), „Szczęściarz” (ogólny: +1 LCK za każdego pokonanego bossa w runie), „Szał Piwny” (Barbarzyńca: każdy boost wydłuża szał).

## 6. Ubrania, trinkety i boosty

Build składa się z trzech warstw. Bronie i ubrania to decyzje (ograniczone sloty), a trinkety to narastająca lawina mocy, która robi z late game chaos jak w RoR2.

| Warstwa | Ile | Skąd | Jak rośnie |
| --- | --- | --- | --- |
| Bronie | 1 aktywna + 3 pasywne | Karty na level-upie | Poziomy z kart, na maksie ewolucja |
| Ubrania | 7 slotów | Karty na level-upie, skrzynie, Nocny | Poziomy z kart |
| Trinkety | Bez limitu | Skrzynie, elity, bossowie, Nocny, skill checki | Stackowanie: każda kolejna kopia wzmacnia efekt |

### Ubrania

| Slot | Ile |
| --- | --- |
| Hełm | 1 |
| Zbroja | 1 |
| Rękawice | 1 |
| Buty | 1 |
| Pierścień | 2 |
| Amulet | 1 |

- Każde ubranie ma **typ**, który decyduje o cesze, którą daje, oraz opcjonalną pasywkę.
- Poziom ubrania (z kart) zwiększa bonus do cechy i wzmacnia pasywkę.
- Wymiana na nowe ubranie: poziom starego przepada.
- Przykłady robocze: Czapka Maga (hełm, INT), Kufajka Krasnoluda (zbroja, CON), Rękawice Łucznika (DEX), Klapki z Basenu (buty, DEX i szybkość ruchu), Sygnet Babci (pierścień, CHA), Amulet z Odpustu (amulet, LCK).

### Trinkety

- Bez limitu, stackują się. Każdy trinket określa, jak rośnie z kolejnymi kopiami (liniowo albo z malejącymi zyskami).
- Od słabych do legendarnych. Wyższa rzadkość = silniejszy albo bardziej zmieniający grę efekt.
- **Trade-offy:** część trinketów daje duży plus za minus, np. „Energetyk z Nocnego”: +25% szybkości ataku, −10% max HP. Nie wszystkie mają minus.

| Rzadkość | Kolor | Efekt |
| --- | --- | --- |
| Pospolity | Szary | Prosty bonus do cechy lub statystyki |
| Niepospolity | Zielony | Bonus plus mały efekt warunkowy |
| Rzadki | Niebieski | Wyraźny efekt procentowy lub warunkowy |
| Epicki | Fioletowy | Efekt zmieniający styl gry |
| Legendarny | Pomarańczowy | Unikalna mechanika, fundament buildu |

Przykłady robocze: „Kostka z Kieszeni Dziadka” (rzadki: +1 do skill checków za każdą kopię), „Skarpety do Sandałów” (niepospolity: +10% szybkości ruchu, wrogowie w pobliżu są spowolnieni).

### Boosty (chwilowe pickupy)

Wypadają z przeciwników losowo i działają kilka–kilkanaście sekund. CHA wydłuża ich czas.

| Boost | Efekt | Typ |
| --- | --- | --- |
| Magnes | Przyciąga cały XP i złoto z mapy | Zwykły |
| Niezniszczalność | Brak obrażeń przez kilka sekund | Zwykły |
| Mikstura życia | Leczy część HP | Zwykły |
| Smok | Zamieniasz się w zionącego ogniem smoka | Śmieszny |
| Powiększenie | Rośniesz, depczesz małych wrogów | Śmieszny |
| Szczęśliwa Godzina | Każde trafienie jest krytyczne przez 5 s | Rzadki |

## 7. Postacie

W pierwszej wersji gramy czwórką znajomych. Każdy ma klasę, dwie cechy główne, biegłości, pasywkę klasową i swoją używkę pod klawiszem F (inspiracja: Debauchery and Magic). Efekty to propozycje do dopracowania.

| Postać | Rasa i klasa | Cechy główne (+2) | Biegłości | Pasywka klasowa | Używka (F) | Startowa broń |
| --- | --- | --- | --- | --- | --- | --- |
| Ada | Elfka, Ranger | DEX, WIS | Zwierzęta, percepcja | **Pies Loki**: towarzysz zawsze przy niej, atakuje sam, skaluje się z WIS | **Dzikówka** (nalewka): **Zew Dzika**, przez arenę przebiega stado dzików taranujących wrogów, Loki wpada w szał | Łuk |
| Bar | Krasnolud, Barbarzyńca | STR, CON | Siłowanie, picie | **Szał**: pasek ładuje się od zadawanych i otrzymywanych obrażeń; pełny skraca cooldown piwa | **Piwo**: **Szał Piwny**, więcej obrażeń, odporność, odrzut wrogów; broń aktywna sama celuje w najbliższego wroga | Topór dwuręczny |
| Sir Seba | Człowiek, Rycerz | STR, CHA | Perswazja, występy | **Honor**: rośnie w walce z elitami i bossami, daje bonus do obrażeń | **Wódka**: **Odwaga z Butelki**, tarcza blokuje kilka pierwszych trafień, taunt ściąga wrogów, rośnie honor | Miecz i tarcza (jedna broń) |
| Albert | Człowiek, Wizard | INT, WIS | Zagadki, magiczne mechanizmy | **Księga Czarów**: zabici wrogowie dają ładunki, co kilka ładunków sam rzuca się losowy czar | **Papierosy**: **Dymek**, chmura dymu wokół niego spowalnia i truje wrogów, czary wzmocnione na czas trwania | Różdżka |

### Używki i promile

Używka to buff postaci pod F: ok. 10 s działania, cooldown ok. 45 s (wartości w definicji używki). Każda używka podnosi pasek promili, który nagradza ryzyko i karze przesadę.

| Element | Działanie |
| --- | --- |
| Pasek promili | W rogu ekranu, 0–100%. U Alberta ten sam pasek nazywa się Nikotyna. |
| Przyrost | Własna używka ok. +40%, cudzy szot lub fajka ok. +25% |
| Spadek | Ok. 1,5% na sekundę |
| Siła buffu | Rośnie z promilami w chwili użycia; mocniejszy jest też efekt na ekranie |
| Kac | Po końcu buffu ok. 3 s spowolnienia ruchu (ok. −20%) |
| Urwany film (powyżej 100%) | Komiczny debuff na ok. 3–4 s, potem pasek spada do 50% |

Przy normalnym używaniu pasek nie przekracza 100%. Ryzyko pojawia się dopiero przy skracaniu cooldownu trinketami i piciu cudzych szotów.

| Postać | Urwany film (propozycja) | Ekran pod wpływem |
| --- | --- | --- |
| Bar | Zasypia na stojąco i chrapie, nie rusza się | Kołysanie kamery, lekkie podwójne widzenie, przytłumione dźwięki |
| Ada | Stado dzików taranuje też ją | Zielono-leśny filtr, kolory natury, odgłosy puszczy |
| Sir Seba | Śpiewa biesiadnie: nie atakuje, a wszyscy wrogowie idą do niego | Patos: chwilowe slow-motion, złote słońce, chór |
| Albert | Atak kaszlu: nie atakuje, stoi w swojej chmurze | Zadymiony ekran, filtr noir/sepia, jazz |

### Rozwinięcia używek

| Element | Działanie |
| --- | --- |
| Zagrycha (trinkety) | Kiszony Ogórek (brak kaca), Otwieracz z Breloczka (krótszy cooldown używki), Paczka Fajek (2 ładunki używki), Słone Paluszki (wolniejszy przyrost promili) |
| Cudze używki | Nocny sprzedaje jednorazowe szoty i fajki innych postaci; rzadko wypadają też z wrogów. Działają jak krótka wersja buffu tamtej postaci i podnoszą promile. |
| Stan a skill checki | Pod wpływem własnej używki: Bar +2 CHA i −2 DEX, Ada +2 WIS, Sir Seba +2 CHA, Albert +2 INT (wartości do balansu) |
| Toast (co-op) | Dwóch graczy używa używki w ciągu 2 s: „Na zdrowie!”, wspólny krótki bonus dla obu |
| Talenty używki | Część talentów klasowych zmienia używkę, np. Bar „Beczka” (piwo leczy), Albert „Cygaro” (większa chmura, dłuższy cooldown) |

### Odblokowanie

Na start grywalna jest jedna postać. Pozostałe są **uwięzione w runach**: uratowanie ich (np. pokonanie bossa, który ich więzi) przenosi je do ośrodka i czyni grywalnymi. Kolejność do ustalenia.

### Kolejni znajomi

Nowe postacie dochodzą jak content po pierwszej wersji. Każda potrzebuje: klasy, 2 cech głównych, biegłości, pasywki klasowej, używki z efektem ekranowym i urwanym filmem, startowej broni, puli talentów klasowych (ok. 15) i questa w ośrodku.

## 8. Areny, biomy i przeciwnicy

Run to 3–4 areny w stylu Risk of Rain 2, każda zakończona bossem, całość do 45 minut (ok. 10–12 minut na arenę). W pierwszej wersji wszystkie areny są z jednego biomu; docelowo każda kolejna arena to nowy, trudniejszy biom z trudniejszym bossem.

### Przebieg areny

1. Lądowanie, eksploracja, zabijanie fal przeciwników, zbieranie XP i złota.
2. Otwieranie skrzyń za złoto, skill checki (sekcja 4), boosty.
3. Znalezienie portalu (nazwa robocza; w klimacie modern-fantasy, np. magiczny przystanek tramwajowy).
4. Aktywacja portalu: pojawia się boss i wzmożone fale, trzeba przeżyć do naładowania portalu.
5. Pokonanie bossa i naładowanie portalu = przejście do Nocnego, a stamtąd na kolejną arenę.

Trudność rośnie z czasem (jak w RoR2), więc długie farmienie opłaca się tylko do pewnego momentu.

### Nocny (przerwa między arenami)

Po każdej arenie poza ostatnią gracz trafia do Nocnego: całodobowego sklepiku osiedlowego zawieszonego między wymiarami, z zaspanym sprzedawcą-czarodziejem. To mały hub 3D, po którym chodzisz postacią (jak sklep między etapami w RoR2, tyle że po każdej arenie).

| Co | Działanie |
| --- | --- |
| Sklep | Ubrania i trinkety za złoto z runu, ceny niższe z CHA |
| Lada z używkami | Jednorazowe szoty i fajki innych postaci (sekcja 7) |
| Trening | Darmowy wybór 1 z 3 ulepszeń cech |
| Dodatkowe punkty cech | Za złoto, cena rośnie z każdym zakupem |
| Rezygnacja z runu | Kończy run i zamienia ekwipunek na złoto dla ośrodka (sekcja 9) |
| Wyjście | Przejście na kolejną arenę |

### Biomy

| Biom | Klimat | Status |
| --- | --- | --- |
| Las | Klasyczny las fantasy z modern-fantasy akcentami (magiczne słupy energetyczne, zgubione grille). Dużo darmowych assetów low poly. | Kandydat na MVP |
| Zaspa | Gdańskie blokowisko: bloki, murale, trzepaki, osiedlowe sklepiki, przystanki | Kandydat na MVP, bardziej unikalny |
| Kolejne | Do wymyślenia (inspiracje z życia znajomych) | Później |

### Przeciwnicy

Potwory to parodie bestiariusza D&D w humorystycznym tonie. Mimik i sowoniedźwiedź są w SRD (wolna licencja), więc można je użyć swobodnie. Beholder i mind flayer to własność Wizards of the Coast, więc ich parodie muszą być wyraźnie przerobione.

| Przeciwnik (roboczo) | Wzorowany na | Pomysł | Biom |
| --- | --- | --- | --- |
| Mimik-śmietnik | Mimik | Udaje kontener na śmieci lub skrzynię z lootem, gryzie | Zaspa, wszędzie |
| Gapior | Beholder (parodia) | Latająca gałka oczna z mackami-pilotami do TV, spojrzenie spowalnia i odwraca sterowanie na chwilę | Zaspa |
| Dresiarskie gobliny | Gobliny | Szybkie, w grupach, rzucają butelkami | Zaspa |
| Sowoniedźwiedź | Owlbear | Tank, szarża | Las |
| Grzybiarze | Myconidy | Wypuszczają trujące zarodniki | Las |

### Elity i bossowie

- **Elity**: zwykli przeciwnicy z modyfikatorem (ognisty, lodowy, szybki, olbrzymi), więcej HP, lepszy drop.
- **Bossowie**: do zaprojektowania. Każdy boss biomu ma 2–3 fazy i więzi jednego ze znajomych (sekcja 7).

## 9. Hub: Ośrodek

Między runami gracz wraca do ośrodka wypoczynkowego wzorowanego na miejscu, do którego jeździmy ze znajomymi. Trafiają tu uratowane postacie, dają questy, a gracz rozbudowuje budynki dające stałe ulepszenia.

**Rekomendacja na start: klikalna mapa ośrodka** (ilustracja z budynkami do kliknięcia). Jest kilka razy tańsza w produkcji niż chodzenie w 3D i łatwiej zrobić z niej lobby co-op. Wersję 3D, po której się chodzi, można zrobić później, gdy rdzeń gry będzie gotowy.

### Waluty

| Waluta | Skąd | Na co |
| --- | --- | --- |
| Złoto | Zbierane w runie (skrzynie, wrogowie). Po runie część przechodzi do ośrodka (zależnie od tego, jak run się skończył). | W runie: skrzynie i Nocny. W ośrodku: budowa i zwykłe ulepszenia. |
| Piwo | Rzadkie: bossowie, questy, wyzwania, skill checki | Budynki i ulepszenia premium, odblokowania |

### Koniec runu

| Sposób | Co trafia do ośrodka |
| --- | --- |
| Śmierć | Część zebranego złota |
| Rezygnacja w Nocnym | Złoto z runu plus ekwipunek (bronie, ubrania, trinkety) zamieniony na złoto według rzadkości i poziomu. Bez uwolnienia znajomego. To decyzja push-your-luck: ryzykujesz dalej czy bierzesz pewną nagrodę. |
| Wygrana | Najwięcej złota, piwo i uwolnienie kolejnego znajomego |

### Budynki (propozycja)

| Budynek | Co daje |
| --- | --- |
| Tawerna | Questy od znajomych, wymiana piwa |
| Kuźnia | Odblokowanie nowych broni i przedmiotów w puli lootów |
| Boisko | Małe stałe bonusy do cech (trening) |
| Biblioteka | Księga Ewolucji, bestiariusz, wskazówki do nieodkrytych ewolucji |
| Domki znajomych | Pojawiają się po uratowaniu postaci, ulepszenia konkretnej postaci |
| Przystanek | Start runu, wybór postaci (później lobby co-op) |

### Questy

Znajomi w ośrodku dają wyzwania, np. „wygraj run Barem bez zbroi”, „zdaj 3 skill checki pod rząd”. Nagrody: piwo, nowe przedmioty i bronie w puli, wskazówki do ewolucji, kosmetyki.

### Meta-progresja (podsumowanie)

| Co zostaje między runami | Co się resetuje |
| --- | --- |
| Odblokowane postacie, budynki i ich ulepszenia, pula broni i przedmiotów, odkryte ewolucje, waluty ośrodka | Poziom postaci, rozdane cechy, talenty, ekwipunek, bronie, złoto z runu (poza częścią przeniesioną) |

## 10. Setting, ton i styl graficzny

Świat to modern-fantasy: współczesne technologie istnieją, ale działają dzięki magii. Ton jest totalnie humorystyczny, pełen inside joków znajomych i parodii D&D.

### Zasady settingu

- Technologia = magia: mecze ogląda się w kryształowej kuli, piloty do TV to różdżki, tramwaje jadą na runach.
- Miejsca z naszego życia (Zaspa, ośrodek wypoczynkowy) w wersji fantasy.
- Potwory D&D przerobione na śmieszne wersje (sekcja 8).
- Fabuła i główny zły: odłożone na później.

### Styl graficzny

| Element | Kierunek |
| --- | --- |
| Modele | Low poly, własnoręcznie robione w Blenderze. Na start darmowe paczki low poly, wymieniane stopniowo na własne. |
| Tekstury | Płaskie kolory lub palety (gradient atlas) zamiast tekstur malowanych: tanie w produkcji, spójne, lekkie dla wydajności. |
| Postacie | Rozpoznawalne po sylwetce i jednym znaku charakterystycznym (pies Lokiego, krasnoludzka broda, zbroja Seby, kapelusz Alberta). |
| Efekty | Czytelne z daleka, mocne kolory dla krytyków i Nat 20. |
| Spójność | Jedna paleta kolorów dla całej gry, żeby mieszanka darmowych i własnych assetów nie gryzła się stylem. |

**Suwak intensywności efektów ekranowych** (0–100%) w ustawieniach: kołysanie kamery po używkach przy szybkim ruchu i rosnącym FOV może części graczy szkodzić. Domyślnie 100%.

**Ocena wiekowa:** alkohol i papierosy jako wzmocnienie to w systemach ocen (PEGI, ESRB) zwykle oznaczenie 16+. Przy wydaniu na Steam trzeba to zaznaczyć w ankiecie treści.

## 11. Technologia

Silnik: **Unity 6.6 (6000.6.7f1), C#, URP**. Znam Unity, AI dobrze pisze C# pod Unity, a sklep z darmowymi assetami low poly przyspiesza start.

### Decyzje architektoniczne

| Obszar | Decyzja | Dlaczego |
| --- | --- | --- |
| Co-op | Single na start, ale logika gry oddzielona od wejścia i grafiki od pierwszego dnia | Dorobienie sieci do gry single to najczęstszy powód przepisywania projektu |
| Biblioteka sieciowa | Netcode for GameObjects lub FishNet, wybór przed etapem co-op | Obie darmowe i dojrzałe |
| Dane gry | Bronie, przedmioty, talenty, wrogowie i boosty jako ScriptableObjecty | Balans bez dotykania kodu, łatwe dodawanie contentu |
| Walidacja danych | Sprawdzanie w edytorze i przy starcie gry (brakujące referencje, zduplikowane ID, złe zakresy) z głośnym błędem | Błędy wychodzą od razu, a nie w połowie runu |
| Losowość | Jeden seed na run, osobne strumienie RNG (walka, loot, karty, sklep, skill checki, spawny) | Powtarzalne bugi, synchronizacja w co-op |
| Testy | Testy jednostkowe (EditMode) dla formuł obrażeń, skill checków i skalowania cech | Przy vibekodowaniu chronią przed cichym rozjechaniem się balansu |
| Kontrola wersji | Git z LFS na modele i dźwięki | Standard dla Unity |

### Środowisko pracy

Development na **CachyOS** (Arch). Unity oficjalnie wspiera na Linuxie tylko Ubuntu, ale na Archu działa od lat bez większych problemów.

| Narzędzie | Instalacja | Uwagi |
| --- | --- | --- |
| Unity Hub | `paru -S unityhub` (AUR) | Natywny pakiet zamiast Flatpaka; sandbox Flatpaka utrudnia otwieranie VS Code z Unity |
| .NET SDK | `sudo pacman -S dotnet-sdk` | Wymagany przez rozszerzenie C# |
| VS Code | `paru -S visual-studio-code-bin` | Oficjalny build Microsoftu. Pakiet `code` (Code - OSS) nie obsługuje C# Dev Kit ani rozszerzenia Unity. |
| Rozszerzenia VS Code | „Unity” od Microsoftu | Dociąga C# i C# Dev Kit (darmowy do użytku niekomercyjnego) |
| Pakiet Unity | Visual Studio Editor (`com.unity.ide.visualstudio`) 2.0.20+ | Obsługuje VS Code mimo nazwy; External Script Editor = Visual Studio Code |
| Modele | Blender (natywnie) | |

Ryzyka i zasady:

- Rolling release może zepsuć edytor po aktualizacji: commit projektu przed większym update, snapshoty Btrfs jako cofnięcie systemu.
- Wayland (KDE Plasma): edytor działa przez XWayland; przy problemach sesja X11.
- System plików rozróżnia wielkość liter: spójne nazewnictwo assetów od początku.
- Import wideo w edytorze na Linuxie tylko w formacie VP8.
- Buildy dla znajomych na Windows: moduł Windows Build Support (Mono). IL2CPP dla Windows wymaga budowania na Windowsie.

### Wydajność

Cel: **stabilne 60 FPS przy 300+ przeciwnikach** na średnim PC.

- Object pooling dla wrogów, pocisków, liczb obrażeń i efektów.
- GPU instancing i proste shadery (low poly + paleta kolorów).
- Prosta AI hord: ruch w stronę gracza z flow field zamiast osobnego pathfindingu na każdego wroga.
- Job System i Burst dla ruchu hord, jeśli profiler pokaże potrzebę.
- Limit jednocześnie wyświetlanych liczb obrażeń i efektów.

## 12. Roadmapa

Cel to grywalny co-op ze znajomymi (M2) w około rok, bez sztywnych terminów. Każdy etap kończy się warunkiem przejścia, a nie datą.

| Etap | Zakres | Warunek przejścia |
| --- | --- | --- |
| M0 · Prototyp | 1 postać, 1 arena, ruch (bieg, skok, dash, ślizg), 3 bronie, 5 wrogów, 1 boss, karty level-up, cechy, talenty, używka postaci | Walka daje frajdę |
| M1 · Vertical slice | 4 postacie, 1 biom (3–4 areny), 10 broni, 5 ewolucji, ubrania i trinkety, Nocny, skill checki, hub jako klikalna mapa | Znajomi grają pełny run |
| M2 · Co-op (cel na rok) | 2–4 graczy online, lobby w ośrodku, synchronizacja lootu i skill checków | Co-op jest stabilny |
| M3 · Rozbudowa | Kolejne biomy, nowi znajomi, fabuła i główny zły, ewentualnie Steam | — |

Modele w M0 i M1 to darmowe paczki low poly; własne modele z Blendera wchodzą stopniowo, zaczynając od czwórki postaci.

## 13. Otwarte kwestie

- [ ] Czy układ 1 broń aktywna + 3 pasywne sprawdza się w playtestach?
- [ ] Biom MVP: Las czy Zaspa?
- [ ] Która postać jest grywalna od startu, w jakiej kolejności ratujemy pozostałe?
- [ ] Ile złota przechodzi do ośrodka po śmierci, przelicznik ekwipunku przy rezygnacji.
- [ ] Maksymalny poziom broni i ubrań.
- [ ] Reroll kart: limit czy koszt?
- [ ] Lista na MVP: bronie (ok. 10), ewolucje (ok. 5), ubrania (ok. 20), trinkety (ok. 30), talenty (ok. 15 na klasę + ok. 20 ogólnych).
- [ ] Lista rodzajów skill checków i sytuacji na MVP (ok. 10).
- [ ] Projekt pierwszego bossa.
- [ ] Nazwa i wygląd portalu kończącego arenę.
- [ ] Sterowanie: tylko mysz i klawiatura, czy też pad?
- [x] Kontroler postaci: OpenKCC (MIT) jako warstwa kolizji, logika ruchu własna.
- [ ] Liczby promili (przyrost, spadek, siła buffu w zależności od promili) i efekty urwanego filmu do playtestów.
- [ ] Fabuła i główny zły (kto i dlaczego więzi znajomych).
- [ ] Biblioteka sieciowa do co-op.
- [ ] Tytuł gry (roboczy: Gra o dzikach Alberta).

