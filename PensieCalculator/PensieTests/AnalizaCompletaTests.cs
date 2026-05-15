using PensieLib;

namespace PensieTests;

// ========================================================================================
// DOCUMENT DE TEST COMPLET — PensieCalculator (AnalizaCompletaTests.cs)
//
// Conține:
//   1. Partiționare în clase de echivalență
//   2. Analiza valorilor de frontieră
//   3. Acoperire la nivel de instrucțiune (Statement Coverage)
//   4. Acoperire la nivel de decizie (Branch/Decision Coverage)
//   5. Acoperire la nivel de condiție (Condition Coverage)
//   6. Circuite independente — Basis Path Coverage (McCabe)
//   7. Analiza raportului generat de Stryker (generator de mutanți)
//   8. Teste suplimentare pentru a omorî 2 mutanți neechivalenți supraviețuitori
//
// Cod sursă analizat: PensieLib/PensieLib.cs — CalculatorPensie.CalculPensie(...)
// Semnătură: public static double CalculPensie(int varstaCurenta, int salariuLunar,
//                                              int aniCotizati, double crestereAnualaSalariu)
//
// Constante relevante (din PensieLib.cs):
//   SalariuMediuEconomie  = 7500.0   RON
//   ValoarePunctPensie    = 81.0     RON  (valoarea oficială din lege)
//   VarstaPensionare      = 65       ani
// ========================================================================================

[TestClass]
public class AnalizaCompletaTests
{
    // ====================================================================================
    // 1. PARTIȚIONARE ÎN CLASE DE ECHIVALENȚĂ
    // ====================================================================================
    //
    // Parametri și domeniile lor valide:
    //   varstaCurenta         : int  ≥ 0
    //   salariuLunar          : int  ≥ 0
    //   aniCotizati           : int  ≥ 0
    //   crestereAnualaSalariu : double ≥ 0
    //
    // Clase de echivalență identificate:
    //
    //   CE1: varstaCurenta < 0          → ArgumentException  (intrare invalidă)
    //   CE2: salariuLunar < 0           → ArgumentException  (intrare invalidă)
    //   CE3: aniCotizati < 0            → ArgumentException  (intrare invalidă)
    //   CE4: crestereAnualaSalariu < 0  → ArgumentException  (intrare invalidă)
    //   CE5: varstaCurenta ≥ 65         → aniRamasi ≤ 0, bucla nu rulează
    //   CE6: totalAniCotizare < 15      → return 0 (sub stagiu minim, fără pensie)
    //   CE7: 15 ≤ totalAniCotizare < 35 → pensie redusă (×0.75)
    //   CE8: totalAniCotizare ≥ 35      → pensie completă (×1.00)
    //
    // unde totalAniCotizare = aniCotizati + max(0, VarstaPensionare - varstaCurenta)

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CE1_VarstaNegativa()
    // CE1: varstaCurenta < 0 → throw ArgumentException
    {
        CalculatorPensie.CalculPensie(-1, 5000, 10, 2.0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CE2_SalariuNegativ()
    // CE2: salariuLunar < 0 → throw ArgumentException
    {
        CalculatorPensie.CalculPensie(30, -1, 10, 2.0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CE3_AniCotizatiNegativi()
    // CE3: aniCotizati < 0 → throw ArgumentException
    {
        CalculatorPensie.CalculPensie(30, 5000, -1, 2.0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CE4_CrestereNegativa()
    // CE4: crestereAnualaSalariu < 0 → throw ArgumentException
    {
        CalculatorPensie.CalculPensie(30, 5000, 10, -1.0);
    }

    [TestMethod]
    public void CE5_VarstaPestePensionare()
    // CE5: varstaCurenta > VarstaPensionare (65) → aniRamasi < 0 → bucla nu rulează
    // varstaCurenta=70: aniRamasi=65-70=-5 (≤0), totalAniCotizare=35 ≥ 35 → completă
    // totalPuncte = 35 * (7500/7500) = 35.0 → result = 35.0 * 81 = 2835.0
    {
        double result = CalculatorPensie.CalculPensie(70, 7500, 35, 0.0);
        Assert.AreEqual(35 * 81.0, result, 0.001);
    }

    [TestMethod]
    public void CE6_StagiuSubMinim()
    // CE6: totalAniCotizare < 15 → return 0
    // varstaCurenta=55: aniRamasi=10, totalAniCotizare=4+10=14 < 15 → return 0
    {
        double result = CalculatorPensie.CalculPensie(55, 7500, 4, 0.0);
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void CE7_PensieRedusa()
    // CE7: 15 ≤ totalAniCotizare < 35 → pensie redusă (×0.75)
    // varstaCurenta=45: aniRamasi=20, totalAniCotizare=0+20=20
    // totalPuncte = 0 (trecut) + 20*1.0 (loop) = 20.0 → result = 20 * 81 * 0.75 = 1215.0
    {
        double result = CalculatorPensie.CalculPensie(45, 7500, 0, 0.0);
        Assert.AreEqual(20 * 81.0 * 0.75, result, 0.001);
    }

    [TestMethod]
    public void CE8_PensieCompleta()
    // CE8: totalAniCotizare ≥ 35 → pensie completă (×1.00)
    // varstaCurenta=30: aniRamasi=35, totalAniCotizare=0+35=35
    // totalPuncte = 35*1.0 = 35.0 → result = 35 * 81 = 2835.0
    {
        double result = CalculatorPensie.CalculPensie(30, 7500, 0, 0.0);
        Assert.AreEqual(35 * 81.0, result, 0.001);
    }


    // ====================================================================================
    // 2. ANALIZA VALORILOR DE FRONTIERĂ
    // ====================================================================================
    //
    // Frontiere identificate:
    //
    //   F1: totalAniCotizare = 14 / 15
    //         → condiția „totalAniCotizare < 15" (linia 23)
    //         → 14 → return 0  |  15 → continuă (pensie redusă)
    //
    //   F2: totalAniCotizare = 34 / 35
    //         → condiția „totalAniCotizare >= 35" (linia 43)
    //         → 34 → pensie redusă  |  35 → pensie completă
    //
    //   F3: aniCotizati + i = 35 / 36
    //         → condiția bonusului „aniCotizati + i > 35" (linia 36)
    //         → 35 → fără bonus  |  36 → bonus 5%
    //
    //   F4: aniRamasi = 0 / 1
    //         → limita inferioară a buclei for (linia 30)
    //         → 0 → bucla nu rulează  |  1 → bucla rulează o dată

    [TestMethod]
    public void BVA1_Total14_SubMinim()
    // F1 — 14 < 15 → return 0
    // varstaCurenta=51: aniRamasi=14, totalAniCotizare=0+14=14
    {
        double result = CalculatorPensie.CalculPensie(51, 7500, 0, 0.0);
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void BVA2_Total15_LimitaMinima()
    // F1 — 15 nu < 15 → continuă, pensie redusă
    // varstaCurenta=50: aniRamasi=15, totalAniCotizare=0+15=15
    // totalPuncte = 15*1.0 = 15.0 → result = 15 * 81 * 0.75 = 911.25
    {
        double result = CalculatorPensie.CalculPensie(50, 7500, 0, 0.0);
        Assert.AreEqual(15 * 81.0 * 0.75, result, 0.001);
    }

    [TestMethod]
    public void BVA3_Total34_SubPensieCompleta()
    // F2 — 34 < 35 → pensie redusă
    // varstaCurenta=31: aniRamasi=34, totalAniCotizare=0+34=34
    // totalPuncte = 34*1.0 = 34.0 → result = 34 * 81 * 0.75 = 2065.5
    {
        double result = CalculatorPensie.CalculPensie(31, 7500, 0, 0.0);
        Assert.AreEqual(34 * 81.0 * 0.75, result, 0.001);
    }

    [TestMethod]
    public void BVA4_Total35_LimitaPensieCompleta()
    // F2 — 35 ≥ 35 → pensie completă
    // varstaCurenta=30: aniRamasi=35, totalAniCotizare=0+35=35
    // totalPuncte = 35*1.0 = 35.0 → result = 35 * 81 = 2835.0
    {
        double result = CalculatorPensie.CalculPensie(30, 7500, 0, 0.0);
        Assert.AreEqual(35 * 81.0, result, 0.001);
    }

    [TestMethod]
    public void BVA5_Bonus5ProcenteLa36Ani()
    // F3 — aniCotizati+i=36 > 35 → bonus 5% acordat
    // varstaCurenta=29: aniRamasi=36, totalAniCotizare=36 ≥ 35 (completă)
    // La ultimul an (i=36): 0+36=36 > 35 → totalPuncte = 35*1.0 + 1.0 + 1.0*0.05 = 36.05
    // result = 36.05 * 81 = 2920.05
    {
        double result = CalculatorPensie.CalculPensie(29, 7500, 0, 0.0);
        Assert.AreEqual(36.05 * 81.0, result, 0.01);
    }

    [TestMethod]
    public void BVA6_AniRamasi0_BuclaNuRuleaza()
    // F4 — aniRamasi=0: bucla for nu execută nicio iterație
    // varstaCurenta=65: aniRamasi=0 (≤0), totalAniCotizare=35
    // totalPuncte = 35 * (7500/7500) = 35.0 → result = 35 * 81 = 2835.0
    {
        double result = CalculatorPensie.CalculPensie(65, 7500, 35, 0.0);
        Assert.AreEqual(35 * 81.0, result, 0.001);
    }

    [TestMethod]
    public void BVA7_AniRamasi1_BuclaOSinguralData()
    // F4 — aniRamasi=1: bucla for rulează exact o iterație
    // varstaCurenta=64: aniRamasi=1, totalAniCotizare=35+1=36 ≥ 35 (completă)
    // i=1: aniCotizati+i=36 > 35 → bonus; totalPuncte = 35+1.0+0.05 = 36.05
    // result = 36.05 * 81 = 2920.05
    {
        double result = CalculatorPensie.CalculPensie(64, 7500, 35, 0.0);
        Assert.AreEqual(36.05 * 81.0, result, 0.01);
    }


    // ====================================================================================
    // 3. ACOPERIRE LA NIVEL DE INSTRUCȚIUNE (Statement Coverage)
    // ====================================================================================
    //
    // Criteriu: fiecare instrucțiune executabilă trebuie parcursă cel puțin o dată.
    //
    // Instrucțiuni și testele care le acoperă:
    //
    //   L12: if (varstaCurenta<0 || salariuLunar<0 || aniCotizati<0 || crestere<0)
    //              → CE1, CE2, CE3, CE4, CE5–CE8 (true și false)
    //   L13:   throw new ArgumentException(...)
    //              → CE1, CE2, CE3, CE4
    //   L15: int aniRamasi = VarstaPensionare - varstaCurenta
    //              → CE5–CE8, BVA1–BVA7
    //   L16: int totalAniCotizare = aniCotizati
    //              → CE5–CE8, BVA1–BVA7
    //   L19: if (aniRamasi > 0)
    //              → CE5, BVA6 (false); CE6–CE8, BVA1–BVA5, BVA7 (true)
    //   L20:   totalAniCotizare = aniCotizati + aniRamasi
    //              → CE6–CE8, BVA1–BVA5, BVA7
    //   L23: if (totalAniCotizare < 15)
    //              → CE6, BVA1 (true); CE7, CE8, BVA2–BVA7 (false)
    //   L24:   return 0
    //              → CE6, BVA1
    //   L26: double totalPuncte = aniCotizati * (salariuLunar / SalariuMediuEconomie)
    //              → CE7, CE8, BVA2–BVA7
    //   L29: double salariuLunarSimulat = salariuLunar
    //              → CE7, CE8, BVA2–BVA7
    //   L30: for (int i = 1; i <= aniRamasi; i++)
    //              → CE7, CE8, BVA2–BVA5, BVA7 (rulează); CE5, BVA6 (nu rulează)
    //   L32:   double puncteAn = salariuLunarSimulat / SalariuMediuEconomie
    //              → CE7, CE8, BVA2–BVA5, BVA7
    //   L33:   totalPuncte += puncteAn
    //              → CE7, CE8, BVA2–BVA5, BVA7
    //   L36:   if (aniCotizati + i > 35)
    //              → BVA5, BVA7 (true); CE8, BVA4 (false, max 0+35=35 ≤ 35)
    //   L37:     totalPuncte += puncteAn * 0.05
    //              → BVA5, BVA7
    //   L39:   salariuLunarSimulat *= (1 + crestereAnualaSalariu / 100.0)
    //              → CE7, CE8, BVA2–BVA5, BVA7
    //   L43: if (totalAniCotizare >= 35)
    //              → CE7, BVA2, BVA3 (false); CE8, BVA4–BVA7 (true)
    //   L44:   return totalPuncte * ValoarePunctPensie
    //              → CE8, BVA4–BVA7
    //   L46:   return totalPuncte * ValoarePunctPensie * 0.75
    //              → CE7, BVA2, BVA3
    //
    // Concluzie: 100% statement coverage — acoperit în întregime de testele CE + BVA.
    // Confirmat de raportul Coverlet: 29/29 linii acoperite = 100%.
    // Nu sunt necesare teste suplimentare pentru statement coverage.


    // ====================================================================================
    // 4. ACOPERIRE LA NIVEL DE DECIZIE (Branch / Decision Coverage)
    // ====================================================================================
    //
    // Criteriu: fiecare ramură (TRUE și FALSE) a fiecărei decizii trebuie parcursă.
    //
    //   D1: varstaCurenta<0 || salariuLunar<0 || aniCotizati<0 || crestereAnualaSalariu<0
    //         TRUE  → CE1, CE2, CE3, CE4
    //         FALSE → CE5, CE6, CE7, CE8, BVA1–BVA7
    //
    //   D2: aniRamasi > 0
    //         TRUE  → CE6, CE7, CE8, BVA1–BVA5, BVA7  (varstaCurenta < 65)
    //         FALSE → CE5, BVA6                         (varstaCurenta ≥ 65)
    //
    //   D3: totalAniCotizare < 15
    //         TRUE  → CE6, BVA1  (total = 14)
    //         FALSE → CE7, CE8, BVA2–BVA7
    //
    //   D4: i ≤ aniRamasi  (condiția buclei for)
    //         TRUE  → CE7, CE8, BVA2–BVA5, BVA7  (cel puțin o iterație)
    //         FALSE → CE5, BVA6                    (aniRamasi = 0, nicio iterație)
    //
    //   D5: aniCotizati + i > 35  (bonus 5%)
    //         TRUE  → BVA5, BVA7  (aniCotizati+i = 36)
    //         FALSE → CE8, BVA4   (max i=35, 0+35=35 ≤ 35)
    //
    //   D6: totalAniCotizare >= 35
    //         TRUE  → CE8, BVA4, BVA5, BVA6, BVA7, CE5
    //         FALSE → CE7, BVA2, BVA3
    //
    // Concluzie: 100% decision coverage — acoperit de testele CE + BVA.
    // Confirmat de raportul Coverlet: 24/24 ramuri acoperite = 100%.
    // Nu sunt necesare teste suplimentare pentru decision coverage.


    // ====================================================================================
    // 5. ACOPERIRE LA NIVEL DE CONDIȚIE (Condition Coverage)
    // ====================================================================================
    //
    // Criteriu: fiecare condiție atomică dintr-o decizie trebuie să ia TRUE și FALSE independent.
    // Deciziile D2–D6 sunt simple (o condiție atomică) → condition coverage = decision coverage.
    // Singura decizie compusă este D1 (4 condiții atomice cu short-circuit „||"):
    //
    //   c1: varstaCurenta < 0
    //         TRUE  → CE1 (varstaCurenta = -1)
    //         FALSE → CE2, CE3, CE4, CE5–CE8 (orice parametru valid)
    //
    //   c2: salariuLunar < 0
    //         TRUE  → CE2 (salariuLunar = -1; c1 = FALSE, deci c2 este evaluat)
    //         FALSE → CE3, CE4, CE5–CE8
    //
    //   c3: aniCotizati < 0
    //         TRUE  → CE3 (aniCotizati = -1; c1=FALSE, c2=FALSE, deci c3 este evaluat)
    //         FALSE → CE4, CE5–CE8
    //
    //   c4: crestereAnualaSalariu < 0
    //         TRUE  → CE4 (crestere = -1.0; c1=c2=c3=FALSE, deci c4 este evaluat)
    //         FALSE → CE5–CE8 (parametri pozitivi)
    //
    // Notă: în C#, „||" folosește evaluare short-circuit — dacă c1=TRUE, c2/c3/c4 nu mai
    //        sunt evaluate. Totuși, fiecare condiție atomică atinge TRUE și FALSE, deci
    //        condiția de acoperire este îndeplinită.
    //
    // Concluzie: 100% condition coverage — acoperit de testele CE1–CE4 + orice test valid.
    // Nu sunt necesare teste suplimentare.


    // ====================================================================================
    // 6. CIRCUITE INDEPENDENTE — BASIS PATH COVERAGE (McCabe)
    // ====================================================================================
    //
    // Graf de control al fluxului (CFG):
    //   Arce (e) = 23,  Noduri (n) = 18
    //   Complexitate ciclomatică: V(G) = e − n + 2 = 23 − 18 + 2 = 7
    //   → 7 căi de bază (circuite liniar independente)
    //
    // Decizii: D1, D2, D3, D4 (buclă), D5 (bonus), D6 (completă/redusă)
    //
    //   P1: D1=TRUE                                              → throw ArgumentException
    //   P2: D1=FALSE, D2=TRUE,  D3=TRUE                         → return 0 (cu ani rămași)
    //   P3: D1=FALSE, D2=FALSE, D3=TRUE                         → return 0 (fără ani rămași)
    //   P4: D1=FALSE, D2=FALSE, D3=FALSE, D4=FALSE, D6=TRUE     → pensie completă, fără buclă
    //   P5: D1=FALSE, D2=TRUE,  D3=FALSE, D4=TRUE, D5=FALSE, D6=TRUE  → completă, fără bonus
    //   P6: D1=FALSE, D2=TRUE,  D3=FALSE, D4=TRUE, D5=TRUE,  D6=TRUE  → completă, cu bonus
    //   P7: D1=FALSE, D2=TRUE,  D3=FALSE, D4=TRUE, D5=FALSE, D6=FALSE → redusă
    //
    // Mapare pe teste:
    //   P1 → CE1  (varstaCurenta = -1)
    //   P2 → CE6  (varstaCurenta=55, aniCotizati=4, total=14)
    //   P3 → BP_P3 (jos) — test dedicat
    //   P4 → CE5  (varstaCurenta=70, aniCotizati=35, total=35)
    //   P5 → CE8  (varstaCurenta=30, aniCotizati=0, total=35, crestere=0)
    //   P6 → BVA5 (varstaCurenta=29, aniCotizati=0, bonus la i=36)
    //   P7 → CE7  (varstaCurenta=45, aniCotizati=0, total=20)

    [TestMethod]
    public void BP_P3_AniRamasiNegativ_StagiuSubMinim()
    // P3: D1=FALSE, D2=FALSE (aniRamasi ≤ 0), D3=TRUE (totalAniCotizare < 15) → return 0
    // varstaCurenta=66: aniRamasi = 65-66 = -1 (D2=false)
    // totalAniCotizare = 10 (nu se modifică, D3: 10 < 15 → return 0)
    {
        double result = CalculatorPensie.CalculPensie(66, 7500, 10, 0.0);
        Assert.AreEqual(0.0, result);
    }


    // ====================================================================================
    // 7. ANALIZA RAPORTULUI STRYKER (Generator de Mutanți)
    // ====================================================================================
    //
    // Raport analizat: StrykerOutput/2026-05-14.22-45-03/reports/mutation-report.html
    //
    // ┌──────┬─────────────────────────────────────────────────────────────┬────────────┬──────────────┐
    // │  ID  │ Mutant (ce se schimbă față de original)                     │   Status   │  Tip         │
    // ├──────┼─────────────────────────────────────────────────────────────┼────────────┼──────────────┤
    // │   0  │ Block removal — elimină întregul corp al metodei             │  Ignored   │ —            │
    // │   1  │ Logical: ultimul || → &&                                    │  Killed    │ —            │
    // │   2  │ Negate: !(varstaCurenta<0 || ...)                           │  Killed    │ —            │
    // │   3  │ Logical: al 2-lea || → &&                                   │  Killed    │ —            │
    // │   4  │ Logical: primul || → &&                                     │  Killed    │ —            │
    // │   5  │ varstaCurenta < 0  →  varstaCurenta > 0                    │  Killed    │ —            │
    // │   6  │ varstaCurenta < 0  →  varstaCurenta <= 0                   │  Killed    │ —            │
    // │   7  │ salariuLunar < 0   →  salariuLunar > 0                     │  Killed    │ —            │
    // │   8  │ salariuLunar < 0   →  salariuLunar <= 0                    │  Killed    │ —            │
    // │   9  │ aniCotizati < 0    →  aniCotizati > 0                      │  Killed    │ —            │
    // │  10  │ aniCotizati < 0    →  aniCotizati <= 0                     │  Killed    │ —            │
    // │  11  │ crestereAnualaSalariu < 0  →  > 0                          │  Killed    │ —            │
    // │  12  │ crestereAnualaSalariu < 0  →  <= 0                         │  Killed    │ —            │
    // │  13  │ Statement: elimină throw (înlocuiește cu ;)                 │  Killed    │ —            │
    // │  14  │ String: "Parametrii invalizi." → ""                         │ Survived   │ Echivalent   │
    // │  15  │ Arithmetic: VarstaPensionare - varstaCurenta → +            │  Killed    │ —            │
    // │  16  │ aniRamasi > 0  →  aniRamasi < 0                            │  Killed    │ —            │
    // │  17  │ aniRamasi > 0  →  aniRamasi >= 0                           │ Survived   │ Echivalent   │
    // │  18  │ Negate: !(aniRamasi > 0)                                   │  Killed    │ —            │
    // │  19  │ aniCotizati + aniRamasi  →  aniCotizati - aniRamasi        │  Killed    │ —            │
    // │  20  │ totalAniCotizare < 15  →  > 15                             │  Killed    │ —            │
    // │  21  │ totalAniCotizare < 15  →  <= 15                            │  Killed    │ —            │
    // │  22  │ Negate: !(totalAniCotizare < 15)                           │  Killed    │ —            │
    // │  23  │ aniCotizati * (sal/SME)  →  aniCotizati / (sal/SME)        │ Survived   │ Neechivalent │
    // │  24  │ salariuLunar / SME  →  salariuLunar * SME                  │  Killed    │ —            │
    // │  25  │ i <= aniRamasi  →  i > aniRamasi  (loop infinit)           │  Timeout   │ Omorât       │
    // │  26  │ i <= aniRamasi  →  i < aniRamasi                           │  Killed    │ —            │
    // │  27  │ i++  →  i--  (loop infinit)                                │  Timeout   │ Omorât       │
    // │  28  │ Block removal — elimină corpul buclei                       │  Ignored   │ —            │
    // │  29  │ sLS / SME  →  sLS * SME                                    │  Killed    │ —            │
    // │  30  │ totalPuncte += puncteAn  →  -=                             │  Killed    │ —            │
    // │  31  │ aniCotizati + i > 35  →  < 35                              │  Killed    │ —            │
    // │  32  │ aniCotizati + i > 35  →  >= 35                             │  Killed    │ —            │
    // │  33  │ Negate: !(aniCotizati + i > 35)                            │  Killed    │ —            │
    // │  34  │ aniCotizati + i  →  aniCotizati - i                        │  Killed    │ —            │
    // │  35  │ totalPuncte += puncteAn*0.05  →  -=                        │  Killed    │ —            │
    // │  36  │ puncteAn * 0.05  →  puncteAn / 0.05                        │  Killed    │ —            │
    // │  37  │ salariuLunarSimulat *= (...)  →  salariuLunarSimulat /= (…) │ Survived   │ Neechivalent │
    // │  38  │ 1 + crestere/100  →  1 - crestere/100                      │ Survived   │ Neechivalent │
    // │  39  │ crestere / 100.0  →  crestere * 100.0                      │ Survived   │ Neechivalent │
    // │  40  │ totalAniCotizare >= 35  →  < 35                            │  Killed    │ —            │
    // │  41  │ totalAniCotizare >= 35  →  > 35                            │  Killed    │ —            │
    // │  42  │ Negate: !(totalAniCotizare >= 35)                          │  Killed    │ —            │
    // │  43  │ totalPuncte * ValoarePunctPensie  →  /                     │  Killed    │ —            │
    // │  44  │ totalPuncte * ValoarePunctPensie * 0.75  →  / 0.75         │  Killed    │ —            │
    // │  45  │ totalPuncte * ValoarePunctPensie (L46)  →  /               │  Killed    │ —            │
    // └──────┴─────────────────────────────────────────────────────────────┴────────────┴──────────────┘
    //
    // SUMAR:
    //   Total mutanți generați : 46
    //   Ignorați               :  2  (ID:0, ID:28 — block removal ignorat de Stryker)
    //   Relevanți              : 44
    //   Killed                 : 36
    //   Timeout (= omorâți)    :  2  (ID:25, ID:27 — buclă infinită detectată)
    //   Survived               :  6
    //     ↳ Echivalenți        :  2  (ID:14, ID:17)
    //     ↳ Neechivalenți      :  4  (ID:23, ID:37, ID:38, ID:39)
    //
    //   Scor mutații (excl. echivalenți și ignorați):
    //     (36 killed + 2 timeout) / (44 - 2 echivalenți) = 38 / 42 ≈ 90.5%
    //
    // MOTIVARE MUTANȚI ECHIVALENȚI:
    //
    //   ID:14 — mesajul excepției este schimbat din "Parametrii invalizi." în "".
    //           Niciun test nu verifică textul mesajului (doar tipul excepției).
    //           Comportamentul extern este identic → ECHIVALENT, nedetectabil prin teste.
    //
    //   ID:17 — condiția „aniRamasi > 0" devine „aniRamasi >= 0".
    //           Când aniRamasi=0: blocul if execută totalAniCotizare = aniCotizati + 0,
    //           care este identic cu valoarea inițială (totalAniCotizare = aniCotizati).
    //           Rezultatul calculului rămâne neschimbat → ECHIVALENT, nedetectabil.
    //
    // MOTIVARE MUTANȚI NEECHIVALENȚI SUPRAVIEȚUITORI:
    //
    //   ID:23 — „aniCotizati * (sal/SME)" → „aniCotizati / (sal/SME)".
    //           Supraviețuiește deoarece TOATE testele folosesc salariuLunar=7500=SME,
    //           deci sal/SME = 1.0, iar 35*1.0 = 35/1.0 = 35 (indistingubil).
    //           Detectabil dacă salariuLunar ≠ 7500 → rezolvat de MK1 (jos).
    //
    //   ID:37 — „salariuLunarSimulat *= (...)" → „salariuLunarSimulat /= (...)".
    //   ID:38 — „1 + crestere/100" → „1 − crestere/100".
    //   ID:39 — „crestere / 100.0" → „crestere * 100.0".
    //           Toți trei supraviețuiesc deoarece TOATE testele cu buclă activă folosesc
    //           crestereAnualaSalariu=0.0, deci factorul multiplicativ este (1+0)=1.0,
    //           iar *= 1.0, /= 1.0, (1-0), (0*100) produc același rezultat.
    //           Detectabili dacă crestere > 0 și bucla rulează ≥ 2 iterații → rezolvat de MK2.


    // ====================================================================================
    // 8. TESTE SUPLIMENTARE PENTRU A OMORÎ 2 MUTANȚI NEECHIVALENȚI
    // ====================================================================================

    [TestMethod]
    public void MK1_OmoareMutant23_ArithmeticInmultireVsDivizare()
    // Țintă: Mutant ID:23
    //   Original:  totalPuncte = aniCotizati * (salariuLunar / SalariuMediuEconomie)
    //   Mutant 23: totalPuncte = aniCotizati / (salariuLunar / SalariuMediuEconomie)
    //
    // Strategie: salariuLunar = 3000 ≠ 7500 → x = 3000/7500 = 0.4 ≠ 1.0
    //            varstaCurenta = 65 → aniRamasi = 0 (bucla nu rulează, izolăm linia L26)
    //
    // Calcul ORIGINAL:
    //   totalPuncte = 35 * (3000/7500) = 35 * 0.4 = 14.0
    //   totalAniCotizare = 35 ≥ 35 → pensie completă
    //   result = 14.0 * 81 = 1134.0
    //
    // Calcul MUTANT 23:
    //   totalPuncte = 35 / (3000/7500) = 35 / 0.4 = 87.5
    //   result = 87.5 * 81 = 7087.5  ≠ 1134.0 → test EȘUEAZĂ pe mutant → mutant omorât ✓
    {
        double result = CalculatorPensie.CalculPensie(65, 3000, 35, 0.0);
        Assert.AreEqual(1134.0, result, 0.001);
    }

    [TestMethod]
    public void MK2_OmoareMutant37_SalariuSimulatInmultireVsDivizare()
    // Țintă: Mutanți ID:37, ID:38, ID:39  (toți trei vizează aceeași linie L39)
    //   Original:  salariuLunarSimulat *= (1 + crestereAnualaSalariu / 100.0)
    //   Mutant 37: salariuLunarSimulat /= (1 + crestereAnualaSalariu / 100.0)
    //   Mutant 38: salariuLunarSimulat *= (1 - crestereAnualaSalariu / 100.0)
    //   Mutant 39: salariuLunarSimulat *= (1 + crestereAnualaSalariu * 100.0)
    //
    // Strategie: crestereAnualaSalariu = 10.0 > 0  și  aniRamasi = 2 ≥ 2 iterații
    //            (factorul multiplicativ devine 1.10 ≠ 1.0, diferența se propagă din i=2)
    //
    // Calcul ORIGINAL  (varstaCurenta=63, salariuLunar=7500, aniCotizati=35, crestere=10.0):
    //   aniRamasi = 65-63 = 2, totalAniCotizare = 35+2 = 37 ≥ 35 → completă
    //   totalPuncte inițial = 35 * (7500/7500) = 35.0
    //
    //   Iterația i=1:
    //     salariuLunarSimulat = 7500  →  puncteAn = 7500/7500 = 1.0
    //     totalPuncte = 35.0 + 1.0 = 36.0
    //     35+1=36 > 35 → bonus: totalPuncte = 36.0 + 1.0*0.05 = 36.05
    //     salariuLunarSimulat *= 1.10 = 8250.0
    //
    //   Iterația i=2:
    //     salariuLunarSimulat = 8250  →  puncteAn = 8250/7500 = 1.1
    //     totalPuncte = 36.05 + 1.1 = 37.15
    //     35+2=37 > 35 → bonus: totalPuncte = 37.15 + 1.1*0.05 = 37.15 + 0.055 = 37.205
    //     salariuLunarSimulat *= 1.10 = 9075.0
    //
    //   result = 37.205 * 81 = 3013.605
    //
    // Calcul MUTANT 37 (/= în loc de *=):
    //   i=1: sLS = 7500/1.1 ≈ 6818.18
    //   i=2: puncteAn ≈ 0.9091, totalPuncte ≈ 37.004
    //   result ≈ 37.004 * 81 ≈ 2997.3  ≠ 3013.605 → mutant 37 omorât ✓
    //
    // Calcul MUTANT 38 (1 - crestere/100):
    //   i=1: sLS = 7500 * 0.90 = 6750
    //   i=2: puncteAn = 0.9, totalPuncte ≈ 36.995
    //   result ≈ 36.995 * 81 ≈ 2996.6  ≠ 3013.605 → mutant 38 omorât ✓
    //
    // Calcul MUTANT 39 (crestere * 100):
    //   i=1: sLS = 7500 * (1 + 10*100) = 7500 * 1001 = 7507500
    //   result >> 3013.605 → mutant 39 omorât ✓
    {
        double result = CalculatorPensie.CalculPensie(63, 7500, 35, 10.0);
        Assert.AreEqual(3013.605, result, 0.01);
    }
}
