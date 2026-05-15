# Calculator de Pensie - Documentatie Testare

## 1. Descrierea aplicatiei

Aplicatia implementeaza functia `CalculPensie` din clasa `CalculatorPensie` (C#, .NET 8), care estimeaza pensia lunara a unui utilizator pe baza sistemului romanesc de puncte de pensie. Astfel functia afla numarul de ani cotizati total si, daca indeplinesc conditiile de vechime, simuleaza o pensie lunara imediat dupa varsta de 65 de ani [2].

Nota: aplicatia este inspirata din algoritmul de calcul al pensiei in Romania si a fost adaptata incat sa corespunda cerintei din documentul cu Teme Proiect Testarea sistemelor software. Astfel produsul final nu este un algoritm realist de calculare a pensiei in Romania.

Pe parcursul proiectului am utilizat in comun suportul de curs coroborat cu utilizarea tool-ului AI Claude (Anthropic) pentru ajutor in intelegerea aprofundata a anumitor erori, concepte si procese. De asemenea acest tool AI a fost folosit pentru identificarea comenzilor `bash` necesare pentru instalarea si utilizarea diferitelor tool-uri (vezi mai jos la sectiunea Configurare hardware/software).

Proiectul nu foloseste masina virtuala.

**Semnatura functiei:**

`public static double CalculPensie(int varstaCurenta, int salariuLunar,int aniCotizati, double crestereAnualaSalariu)` - 4 intrari

**Parametri de intrare:**
| Parametru | Tip | Descriere |
|---|---|---|
| `varstaCurenta` | `int` | Varsta curenta a utilizatorului (ani) |
| `salariuLunar` | `int` | Salariul lunar brut actual (RON) |
| `aniCotizati` | `int` | Ani de cotizare deja acumulati |
| `crestereAnualaSalariu` | `double` | Cresterea anuala estimata a salariului (%) |

**Constante utilizate:**
- `SalariuMediuEconomie = 7500.0` RON
- `ValoarePunctPensie = 81.0` RON
- `VarstaPensionare = 65` ani

**Logica functiei:**
1. Validare parametri - arunca `ArgumentException` pentru valori negative
2. Calculeaza `aniRamasi = 65 - varstaCurenta`
3. Daca `aniRamasi > 0`, adauga anii ramasi la stagiul de cotizare
4. Daca `totalAniCotizare < 15`, returneaza 0 (sub stagiu minim)
5. Simuleaza acumularea de puncte de pensie an cu an (cu bonus 5% pentru fiecare an peste 35)
6. Returneaza pensia completa daca `totalAniCotizare >= 35`, sau redusa (x0.75) altfel

## 2. Configurare hardware/software

| Element | Detalii |
|---|---|
| **Sistem de operare** | Linux (Ubuntu) 24.04.4 LTS |
| **Runtime** | .NET 8.0.126 SDK |
| **IDE** | Visual Studio Code |
| **Framework de testare** | MSTest 3.0.4 (Microsoft.VisualStudio.TestTools.UnitTesting) |
| **Coverage** | Coverlet 6.0.0 (`coverlet.collector`) + ReportGenerator 5.5.10 |
| **Mutation testing** | Stryker.NET 4.14.1 (`dotnet-stryker`) |
| **Model** | Acer Nitro ANV16-42 |
| **CPU** | AMD Ryzen 5 240 (6 nuclee, 12 thread-uri) cu Radeon 760M integrat |
| **RAM** | 16 GB |
| **GPU 1** | NVIDIA RTX 5050 (dedicat) |

**Structura proiectului:**
```
PensieCalculator/
|── PensieCalculator.sln (fișier de organizare care tine impreună mai multe proiecte)
|── PensieLib/
    |── PensieLib.cs (functia propriu-zisa)
|── PensieTests/
    |── PensieTests.cs (fisierul de test)
    |── AnalizaCompletaTests.cs (fisierul de test realizat de tool-ul AI)
    |── StrykerOutput/
```

**Rulare teste:**
```bash
cd PensieCalculator
dotnet test
```

**Generare raport coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

**Rulare mutation testing:**
```bash
cd PensieTests
dotnet stryker
```

## 3. Graf de control al fluxului (CFG)

Graful de control al fluxului a fost construit in [mermaid.live](https://mermaid.live) pe baza codului sursa, cu noduri corespunzatoare fiecarei decizii si instructiuni relevante.

![Control Flow Graph](Control%20Flow%20Graph.png)

**Decizii identificate:**
| Decizie | Conditie |
|---|---|
| D1 | `varstaCurenta < 0 || salariuLunar < 0 || aniCotizati < 0 || crestereAnualaSalariu < 0` |
| D2 | `aniRamasi > 0` |
| D3 | `totalAniCotizare < 15` |
| D4 | `i <= aniRamasi` (conditia loop-ului) |
| D5 | `aniCotizati + i > 35` (bonus) |
| D6 | `totalAniCotizare >= 35` |

**Complexitate ciclomatica:**
```
V(G) = e - n + 2 = 23 - 18 + 2 = 7 [1]
```

## 4. Strategii de testare

### 4.1 Partitionare in clase de echivalenta

Clasele de echivalenta identificate:

PARTITIONARE IN CLASE DE ECHIVALENTA - partitionarea domeniului problemei (datele de intrare) în partitii de echivalenta sau clase de echivalenta astfel incat, din punctul de vedere al specificatiei datele dintr-o clasa sunt tratate în mod identic [1].
Clase de echivalenta identificate:
- CE1: varstaCurenta < 0          - exceptie
- CE2: salariuLunar < 0           - exceptie
- CE3: aniCotizati < 0            - exceptie
- CE4: crestereAnualaSalariu < 0  - exceptie
- CE5: varstaCurenta >= 65        - aniRamasi <= 0, loop-ul nu ruleaza
- CE6: totalAniCotizare < 15      - return 0
- CE7: 15 <= totalAniCotizare < 35 - pensie redusa
- CE8: totalAniCotizare >= 35     - pensie completa

**Teste scrise:** EP1-EP9 (9 teste)

### 4.2 Analiza valorilor de frontiera

Frontierele identificate:

| Frontiera | Valori testate | Conditie asociata |
|---|---|---|
| F1 | `totalAniCotizare = 14 / 15` | `totalAniCotizare < 15` |
| F2 | `totalAniCotizare = 34 / 35` | `totalAniCotizare >= 35` (stagiu) |
| F3 | `aniCotizati + i = 35 / 36` | `> 35` (bonus) |
| F4 | `aniRamasi = 0 / 1` | limita inferioara loop |

**Teste scrise:** BVA1-BVA6 (6 teste)

BVA6 (`aniRamasi = 1`) a fost adaugat explicit pentru a testa loop-ul cu o singura iteratie - cazul `aniRamasi = 0` era deja acoperit de EP5.

### 4.3 Acoperire la nivel de instructiune (Statement Coverage)

Fiecare linie de cod executata cel putin o data.
Toate liniile sunt acoperite de testele anterioare:
- throw ArgumentException         - EP1-EP4
- if (aniRamasi > 0)              - EP5, EP6 (false); EP7, EP8, EP9 (true)
- if (totalAniCotizare < 15)      - EP7, BVA1 (true); EP8, EP9 (false)
- totalPuncte += puncteAn * 0.05  - BVA4
- return pensie completa          - EP9, BVA3
- return pensie redusa            - EP8, BVA5

**Rezultat:** 100% statement coverage, confirmat de raportul Coverlet.

![Raport coverage](Raport%20coverage%20instructiuni%2C%20ramuri.png)


### 4.4 Acoperire la nivel de decizie (Branch Coverage)

Fiecare ramura (TRUE si FALSE) a fiecarei decizii este parcursa:

| Decizie | TRUE | FALSE |
|---|---|---|
| D1 | EP1, EP2, EP3, EP4 | EP5, EP6, ... |
| D2 | EP7, EP8, EP9, BVA1-BVA6 | EP5 (varsta=65), EP6 (varsta=66) |
| D3 | EP7, BVA1 | EP8, EP9, BVA2-BVA6 |
| D4 | EP8, EP9, BVA2-BVA5, BVA6 | EP5, EP6 (aniRamasi <= 0) |
| D5 | BVA4, BVA6 | EP9, BVA3 |
| D6 | EP9, BVA3, BVA4, BVA6, EP5, EP6 | EP8, BVA2, BVA5 |

**Rezultat:** 100% branch coverage, confirmat de raportul Coverlet.

### 4.5 Acoperire la nivel de conditie (Condition Coverage)

Singura decizie compusa [1] este D1 (4 conditii atomice legate prin `||` cu evaluare short-circuit):

Fiecare conditie individuala dintr-o decizie trebuie sa ia si true si false [1].
Singura conditie compusa D1: c1:varstaCurenta<0 || c2:salariuLunar<0 || c3:aniCotizati<0 || c4:crestereAnualaSalariu<0
Restul deciziilor (D2-D6) sunt simple — condition coverage = branch coverage pentru ele.
C# evalueaza "||" cu short-circuit — daca c1=true, c2/c3/c4 nu mai sunt evaluate.
Astfel:
        
|Test                 | c1  |    c2     |    c3     |    c4    |
|---|---|---|---|---|
|EP1 (varsta=-2)      | T   | neevaluat | neevaluat | neevaluat|
|EP2 (salariu=-10000) | F   | T         | neevaluat | neevaluat| 
|EP3 (ani=-10)        | F   | F         | T         | neevaluat|
|EP4 (crestere=-2)    | F   | F         | F         | T        |
|Orice test valid     | F   | F         | F         | F        |

**Rezultat:** 100% condition coverage - acoperit de testele existente, fara teste noi necesare.

### 4.6 Acoperire la nivel de circuite independente (Basis Path)

**V(G) = 7** - 7 circuite liniar independente:

Deciziile din cod:
- D1: parametri invalizi?
- D2: aniRamasi > 0?
- D3: totalAniCotizare < 15?
- D4: i <= aniRamasi? (loop)
- D5: aniCotizati + i > 35? (bonus)
- D6: totalAniCotizare >= 35?

Cele 7 circuite independente sunt astfel:
- P1: D1=true - exceptie
- P2: D1=false, D2=true, D3=true - return 0
- P3: D1=false, D2=false, D3=true - return 0
- P4: D1=false, D2=false, D3=false, D4=false, D6=true - pensie completa fara loop
- P5: D1=false, D2=true, D3=false, D4=true, D5=false, D6=true - pensie completa fara bonus
- P6: D1=false, D2=true, D3=false, D4=true, D5=true, D6=true - pensie completa cu bonus
- P7: D1=false, D2=true, D3=false, D4=true, D5=false, D6=false - pensie redusa 
        
Pentru:
- P1 - EP1, 
- P2 - EP7, 
- P3 - BP1,
- P4 - EP5, 
- P5 - EP9, 
- P6 - BVA4, 
- P7 - EP8.

Testul BP1 a fost adaugat special pentru P3 - singurul circuit fara acoperire din testele EP/BVA:

```csharp
// varstaCurenta=66: aniRamasi=-1 (D2=false), totalAniCotizare=10 (D3: 10<15, return 0)
CalculatorPensie.CalculPensie(66, 7500, 10, 0.0) // expected: 0
```
Captura de ecran cu rularea testelor proprii:

![Rulare teste](dotnet%20test%20user.png)


## 5. Testare prin mutanti (Stryker.NET)

**Raport Stryker:** 
- Inainte de eliminare a mutantilor neechivalenti `PensieTests/StrykerOutput/2026-05-15.18-10-28/reports/mutation-report.html`
- Dupa eliminarea mutantilor neechivalenti `PensieTests/StrykerOutput/2026-05-15.18-10-50/reports/mutation-report.html`

![Raport Stryker inainte](stryker%20test%201.png)

![Raport Stryker dupa](stryker%20test%202.png)


### Rezultate

La prima rularea a tool-ului Stryker.NET, au fost generati 44 de mutanti din care 8 au supravietuit, 2 au intrat in loop infinit si au fost considerati "omorati", iar restul de 34 au fost eliminati cu succes (81.82%).

### Mutanti supravietuitori - analiza

#### Echivalenti (nedetectabili) - comportament identic cu originalul pe orice input - nu putem scrie niciun test care sa il detecteze astfel raman "in viata" pentru totdeauna 
        
Mutant3: Exception("")
Ce schimba: schimba mesajul
Echivalent?: Echivalent (nu testam mesajul)
        
Mutant4: aniRamasi >= 0
Ce schimba: > devine >=
Echivalent?: Echivalent (aniRamasi=0 aduna 0, rezultat identic)
        
Acesti mutanti nu pot fi omorati prin niciun test - sunt **echivalenti cu programul original**.

#### Neechivalenti (detectabili prin teste dedicate) - comportament diferit de original - putem scrie un test care il detecteaza si il "omoram"
      
Mutant1: varstaCurenta <= 0
Ce schimba: < devine <= 
Echivalent?: Neechivalent
        
Mutant2 salariuLunar <= 0
Ce schimba: < devine <=
Echivalent?: Neechivalent
        
Mutant5: aniCotizati / (salariuLunar / SalariuMediuEconomie)
Ce schimba: * devine /
Echivalent?: Neechivalent
        
Mutant6: salariuLunarSimulat /= (1 + crestere / 100)
Ce schimba: *= devine /=
Echivalent?: Neechivalent
        
Mutant7: salariuLunarSimulat *= (1 - crestere / 100)
Ce schimba: + devine -
Echivalent?: Neechivalent
        
Mutant8: salariuLunarSimulat *= (1 + crestere * 100)
Ce schimba: /100 devine *100
Echivalent?: Neechivalent

Am creat 4 teste aditionale pentru a diferentia mutanti neechivalenti astfel:

    [TestMethod]
    public void Mutant1_Varsta0() 
    {
        // Original (< 0): 0 nu e negativ - nu arunca exceptie, returneaza 0
        // Mutant (<= 0): 0 <= 0 - ar arunca exceptie - test pica - mutant omorat

        double result = CalculatorPensie.CalculPensie(0, 10000, 10, 2.0);
        Assert.IsTrue(result>=0);
    }

    [TestMethod]
    public void Mutant2_Salariu0() 
    {
        // Original (< 0): 0 nu e negativ - nu arunca exceptie, returneaza 0
        // Mutant (<= 0): 0 <= 0 - ar arunca exceptie - test pica - mutant omorat

        double result = CalculatorPensie.CalculPensie(60, 0, 10, 2.0);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void Mutant5_salariuLunarSimulat() 
    {
        // varstaCurenta=55, aniCotizati=10, salariuLunar=3750 (jumatate din medie)
        // Original: totalPuncte = 10 * (3750/7500) = 10 * 0.5 = 5
        // Mutant:   totalPuncte = 10 / (3750/7500) = 10 / 0.5 = 20
        // rezultat final diferit - test pica - mutant omorat

            double result = CalculatorPensie.CalculPensie(55, 3750, 10, 0.0);
            Assert.AreEqual(10 * 81 * 0.75, result, 0.01);
    }

     [TestMethod]
    public void Mutant6_7_8_salariuLunarSimulat() 
    {
        // varstaCurenta = 63 pentru a intra de doua ori in loop si pentru a incrementa o data salariul simulat
        // aniCotizati = 13 petnru a ajunge la minimul de 15 ani
        // salariuLunar = 7500 pentru calcul usor
        // Original: totalPuncte = 15.01
        // Mutant6: totalPuncte = 14 + 1/1.01
        // Mutant7: totalPuncte = 14 + 0.99
        // Mutant8: totalPuncte = 14 + 101
        // rezultat final diferit - test pica - mutanti omorati

            double result = CalculatorPensie.CalculPensie(63, 7500, 13, 1.0);
            Assert.AreEqual(15.01 * 81 * 0.75, result, 0.01);
    }
    
Scorul final a fost de 95.45%.

## 6. Raport comparatie: teste proprii vs. teste generate de AI

Instrumentul AI utilizat pentru generarea suitei de referinta: **Claude** (Anthropic).

Fisierele comparate:
- `PensieTests/PensieTests.cs` - suita proprie, 20 teste
- `AnalizaCompletaTests.cs` - suita generata de AI, 18 teste

Captura de ecran cu rularea ambelor suite (teste proprii + teste generate de AI):

![Rulare teste AI si proprii](dotnet%20test%20tot.png)

### 6.0 Exemple de interactiune cu AI

**Prompt 1 - Condition coverage si short-circuit:**

Ce inseamna condition coverage si de ce nu e suficient un singur test cu toti parametrii negativi?

**Raspuns Claude:**
Condition coverage cere ca fiecare conditie atomica (fiecare comparatie separata) sa fie evaluata atat true cat si false cel putin o data. Din cauza short-circuit evaluation in C#, daca primul termen `varstaCurenta < 0` este true, restul nu se mai evalueaza deloc. Asadar ai nevoie de teste separate: unul cu doar primul parametru negativ (declanseaza exceptia din primul termen), unul cu primii trei pozitivi si ultimul negativ (forteaza evaluarea tuturor termenilor), si unul cu toti pozitivi (intreaga conditie false). Altfel unele conditii atomice nu sunt niciodata evaluate si coverage-ul raportat de Coverlet va fi sub 100% pe ramuri.

**Prompt 2 - Instalare Coverlet si ReportGenerator:**

Cum rulez coverage cu Coverlet si generez un raport HTML? Ce comenzi bash folosesc?

**Raspuns Claude:**
```
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```
Deschide `coveragereport/index.html` in browser pentru raportul vizual. Daca `reportgenerator` nu e recunoscut dupa instalare, adauga `~/.dotnet/tools` la PATH cu: `export PATH="$PATH:$HOME/.dotnet/tools"`.

### 6.1 Comparatie structurala

| Criteriu | Teste proprii (PensieTests.cs) | Teste AI (AnalizaCompletaTests.cs) |
|---|---|---|
| **Numar total de teste** | 20 | 18 |
| **Conventie de denumire** | EP1-EP9, BVA1-BVA6, BP1, Mutant... | CE1-CE8, BVA1-BVA7, BP_P3, MK1-MK2 |
| **Clase de echivalenta** | 9 teste (EP1-EP9) | 8 teste (CE1-CE8) |
| **Valori de frontiera** | 6 teste (BVA1-BVA6) | 7 teste (BVA1-BVA7) |
| **Circuite independente** | 1 test dedicat (BP1) | 1 test dedicat (BP_P3) |
| **Teste pentru mutanti** | 4 teste | 2 teste |
| **Tabel mutanti documentati** | 8 mutanti | 46 mutanti (tabel complet) |

### 6.2 Diferente pe strategii

**Partitionare in clase de echivalenta**

Ambele suite identifica aceleasi 8 clase. Diferenta este la CE5 (varsta >= 65):
- Testele proprii au 2 teste: EP5 (varstaCurenta=65, aniRamasi=0 exact) si EP6 (varstaCurenta=66, aniRamasi negativ) - acopera ambele sub-cazuri ale clasei.
- Suita AI are un singur test CE5 (varstaCurenta=70) - acopera clasa dar nu distinge intre aniRamasi=0 si aniRamasi negativ.

**Analiza valorilor de frontiera**

Ambele suite acopera aceleasi 4 frontiere. Diferenta este la frontiera F4 (aniRamasi = 0 / 1):
- Suita AI are doua teste explicite: BVA6 (aniRamasi=0, bucla nu ruleaza) si BVA7 (aniRamasi=1, bucla ruleaza o data).
- Testele proprii au BVA6 (aniRamasi=1), iar aniRamasi=0 este acoperit in EP5 fara a fi marcat explicit ca frontiera BVA.

**Circuite independente**

Ambele suite au un singur test dedicat pentru P3, cu parametri identici (varstaCurenta=66, aniCotizati=10, rezultat 0). Celelalte 6 circuite sunt acoperite de testele EP si BVA in ambele suite.

**Testare prin mutanti**

Aceasta este cea mai mare diferenta intre cele doua suite.

Testele proprii au 4 teste:
- Mutant1 (varsta <= 0) verifica ca programul nu arunca exceptie pentru varstaCurenta=0, folosind Assert.IsTrue(result >= 0) - asertiunea nu verifica valoarea returnata.
- Mutant2 (salariu <= 0) verifica ca pentru salariuLunar=0 rezultatul este 0, folosind Assert.AreEqual(0, result) - asertiune precisa.
- Mutant5 (inmultire vs. impartire, salariuLunar=3750) omoara mutantul, dar bucla ruleaza si ea, deci linia testata nu este izolata complet.
- Mutant6_7_8 (factor crestere, aniCotizati=13, crestere=1.0) omoara toti trei mutantii cu 2 iteratii de bucla.

Suita AI are 2 teste:
- MK1 (varstaCurenta=65, salariuLunar=3000) omoara mutantul de inmultire vs. impartire cu aniRamasi=0 - bucla nu ruleaza, linia de initializare a punctelor este izolata complet. Valoarea asteptata calculata explicit: 1134.0.
- MK2 (aniCotizati=35, crestere=10.0) omoara mutantii de factor crestere cu valoare asteptata calculata pas cu pas: 3013.605.

| Mutant (denumire proprie) | Acoperit in teste proprii | Acoperit in suita AI |
|---|---|---|
| Mutant3 - mesaj exceptie | Echivalent, netestat | Echivalent, netestat |
| Mutant4 - aniRamasi >= 0 | Echivalent, netestat | Echivalent, netestat |
| Mutant5 - inmultire vs. impartire | Mutant5 - AreEqual cu toleranta | MK1 - valoare exacta 1134.0 |
| Mutant6/7/8 - factor crestere | Mutant6_7_8 - AreEqual cu toleranta | MK2 - valoare exacta 3013.605 |
| Mutant1 - varsta <= 0 | Assert.IsTrue slab | Omorat implicit de CE1 (varsta=-1) |
| Mutant2 - salariu <= 0 | Assert.AreEqual(0, result) | Omorat implicit de CE2 (salariu=-1) |

### 6.3 Diferente in stilul de documentare

Testele proprii documenteaza strategiile ca blocuri de comentarii deasupra testelor, cu explicatia logicii generale. Fiecare test are un comentariu scurt cu parametrii si ce se testeaza.

Suita AI documenteaza fiecare test cu calcule complete pas cu pas in comentarii, inclusiv valorile intermediare si cum difera rezultatul pentru fiecare varianta de mutant. Tabelul de mutanti contine toti cei 46 de mutanti generati de Stryker cu statusul si clasificarea fiecaruia.

### 6.4 Concluzie

Ambele suite ating 100% statement, branch si condition coverage si acopera toate cele 7 circuite independente. Ambele folosesc ValoarePunctPensie=81.0 RON. Scorul de mutanti este similar.

Diferentele principale sunt:
- **Numar de teste**: testele proprii au 20 de teste, suita AI are 18
- **Acoperirea CE5**: testele proprii sunt mai detaliate (2 teste vs. 1)
- **Frontiera F4**: suita AI o acopera explicit in BVA, testele proprii o acopera implicit prin EP5
- **Testele pentru mutanti**: suita AI foloseste asertiuni mai precise si izoleaza mai bine linia testata
- **Documentarea mutantilor**: suita AI documenteaza toti 46 de mutanti, testele proprii doar 8

---

## 7. Referinte bibliografice

1. Suport de curs Testarea sistemelor software - materiale laborator 

2. Casa nationala de Pensii Publice. https://www.cnpp.ro/calculul-pensiei, Data accesarii: 12 Mai 2026

3. Anthropic. *Claude - AI assistant*. https://claude.ai, Data generarii: 14 Mai 2026

4. Mermaid live. https://mermaid.live, Data accesarii: 12 Mai 2026

