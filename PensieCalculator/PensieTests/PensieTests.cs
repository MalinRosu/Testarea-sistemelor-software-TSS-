using PensieLib;

namespace PensieTests;

[TestClass]
public class CalculatorPensieTests
{

    // PARTITIONARE IN CLASE DE ECHIVALENTA
    // Clase de echivalenta identificate:
    //   CE1: varstaCurenta < 0          - exceptie
    //   CE2: salariuLunar < 0           - exceptie
    //   CE3: aniCotizati < 0            - exceptie
    //   CE4: crestereAnualaSalariu < 0  - exceptie
    //   CE5: varstaCurenta >= 65        - aniRamasi <= 0, loop-ul nu ruleaza
    //   CE6: totalAniCotizare < 15      - return 0
    //   CE7: 15 <= totalAniCotizare < 35 - pensie redusa
    //   CE8: totalAniCotizare >= 35     - pensie completa


    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void EP1_VarstaNegativa() // CE1
    {
        CalculatorPensie.CalculPensie(-2, 10000, 10, 2.0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void EP2_SalariuNegativ() // CE2
    {
        CalculatorPensie.CalculPensie(20, -10000, 10, 2.0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void EP3_AniCotizatiNegativi() // CE3
    {
        CalculatorPensie.CalculPensie(20, 10000, -10, 2.0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void EP4_CrestereNegativa() // CE4
    {
        CalculatorPensie.CalculPensie(20, 10000, 10, -2.0);
    }

    [TestMethod]
    public void EP5_VarstaPensionareExacta_AniRamasi0() // CE5 - aniRamasi=0, loop-ul nu ruleaza
    {
        // varstaCurenta=65: aniRamasi=0, totalAniCotizare=35
        double result = CalculatorPensie.CalculPensie(65, 7500, 35, 0.0);
        Assert.AreEqual(35 * 81, result);
    }

    [TestMethod]
    public void EP6_PestePensionare_AniRamasiNegativ() // CE5 - aniRamasi<0, loop-ul nu ruleaza
    {
        // varstaCurenta=66: aniRamasi=-1, if(aniRamasi>0) fals - totalAniCotizare=35
        double result = CalculatorPensie.CalculPensie(66, 7500, 35, 0.0);
        Assert.AreEqual(35 * 81, result);
    }

    [TestMethod]
    public void EP7_StagiuSubMinim() // CE6
    {
        // varstaCurenta=55, aniCotizati=4: 4+(65-55)=14 < 15
        double result = CalculatorPensie.CalculPensie(55, 5000, 4, 0.0);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void EP8_StagiuPartial() // CE7
    {
        // varstaCurenta=45: 0+(65-45)=20, 15<=20<35 - pensie redusa
        double result = CalculatorPensie.CalculPensie(45, 7500, 0, 0.0);
        Assert.AreEqual(20 * 81 * 0.75, result);
    }

    [TestMethod]
    public void EP9_StagiuComplet() // CE8
    {
        // varstaCurenta=30: 0+(65-30)=35 >= 35 - pensie completa
        double result = CalculatorPensie.CalculPensie(30, 7500, 0, 0.0);
        Assert.AreEqual(35 * 81, result);
    }


    // ANALIZA VALORILOR DE FRONTIERA
    // Frontiere identificate:
    //   totalAniCotizare = 14 / 15  - conditia < 15
    //   totalAniCotizare = 34 / 35  - conditia >= 35 (stagiu)
    //   aniCotizati + i  = 35 / 36  - conditia > 35  (bonus)
    //   aniRamasi = 0 / 1           - limita inferioara a buclei for
    //                                  aniRamasi=0 - testat in EP5
    //                                  aniRamasi=1 - testat in BVA6
    

    [TestMethod]
    public void BVA1_TotalAni14_SubMinim() // frontiera: 14 < 15 - return 0
    {
        // varstaCurenta=51: 0+(65-51)=14
        double result = CalculatorPensie.CalculPensie(51, 7500, 0, 0.0);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void BVA2_TotalAni15_ExactMinim() // frontiera: 15 nu < 15 - continua
    {
        // varstaCurenta=50: 0+(65-50)=15
        double result = CalculatorPensie.CalculPensie(50, 7500, 0, 0.0);
        Assert.AreEqual(15 * 81 * 0.75, result);
    }

    [TestMethod]
    public void BVA3_TotalAni35_PensieCompleta() // frontiera: 35 >= 35 - completa
    {
        // varstaCurenta=30: 0+(65-30)=35
        double result = CalculatorPensie.CalculPensie(30, 7500, 0, 0.0);
        Assert.AreEqual(35 * 81, result);
    }

    [TestMethod]
    public void BVA4_TotalAni36_CuBonus() // frontiera: la i=36, 36 > 35 - bonus
    {
        // varstaCurenta=29: 0+(65-29)=36, bonus la i=36
        // totalPuncte = 36 + 1*0.05 = 36.05
        double result = CalculatorPensie.CalculPensie(29, 7500, 0, 0.0);
        Assert.AreEqual(36.05 * 81, result, 0.01);
    }

    [TestMethod]
    public void BVA5_TotalAni34_PensieRedusa() // frontiera: 34 < 35 - redusa
    {
        // varstaCurenta=31: 0+(65-31)=34
        double result = CalculatorPensie.CalculPensie(31, 7500, 0, 0.0);
        Assert.AreEqual(34 * 81 * 0.75, result);
    }

    [TestMethod]
    public void BVA6_AniRamasi1_Loop1() // frontiera: aniRamasi=1 - loop-ul ruleaza exact o data
    {
        // varstaCurenta=64: aniRamasi=1, totalAniCotizare=36
        // totalPuncte = 35 + 1 + 0.05(bonus) = 36.05
        double result = CalculatorPensie.CalculPensie(64, 7500, 35, 0.0);
        Assert.AreEqual(36.05 * 81, result, 0.01);
    }

  
    // ACOPERIRE LA NIVEL DE INSTRUCTIUNE (Statement Coverage)
    // Fiecare linie de cod executata cel putin o data.
    // Toate liniile sunt acoperite de testele anterioare:
    //   throw ArgumentException         - EP1-EP4
    //   if (aniRamasi > 0)              - EP5, EP6 (false); EP7, EP8, EP9 (true)
    //   if (totalAniCotizare < 15)      - EP7, BVA1 (true); EP8, EP9 (false)
    //   totalPuncte += puncteAn * 0.05  - BVA4
    //   return pensie completa          - EP9, BVA3
    //   return pensie redusa            - EP8, BVA5
    // Concluzie: 100% statement coverage (vezi raport Coverlet)


    // ACOPERIRE LA NIVEL DE DECIZIE (Branch Coverage)
    // 6 decizii, fiecare trebuie sa ia si true si false:
    //
    //   D1: varstaCurenta<0 || salariuLunar<0 || aniCotizati<0 || crestereAnualaSalariu<0
    //       true  - EP1, EP2, EP3, EP4
    //       false - EP5, EP6, EP7, ...
    //
    //   D2: aniRamasi > 0
    //       true  - EP7, EP8, EP9, BVA1, BVA2, BVA3, BVA4, BVA5, BVA6
    //       false - EP5 (varsta=65), EP6 (varsta=66)
    //
    //   D3: totalAniCotizare < 15
    //       true  - EP7 (total=14), BVA1 (total=14)
    //       false - EP8, EP9, BVA2, BVA3, BVA4, BVA5
    //
    //   D4: i <= aniRamasi (conditia buclei)
    //       true  - EP8, EP9, BVA2, BVA3, BVA4, BVA5, BVA6 (loop-ul ruleaza)
    //       false - EP5, EP6 (loop-ul nu ruleaza, aniRamasi=0 sau negativ)
    //
    //   D5: aniCotizati + i > 35 (bonus)
    //       true  - BVA4 (la i=36, 0+36=36 > 35)
    //       false - EP9, BVA3 (max i=35, 0+35=35 nu > 35)
    //
    //   D6: totalAniCotizare >= 35
    //       true  - EP9, BVA3, BVA4, BVA6, EP5, EP6
    //       false - EP8, BVA2, BVA5
    //
    // Concluzie: toate ramurile sunt acoperite de testele existente


    // ACOPERIRE LA NIVEL DE CONDITIE (Condition Coverage)
    // Fiecare conditie individuala dintr-o decizie trebuie sa ia si true si false
    // Singura conditie compusa D1: c1:varstaCurenta<0 || c2:salariuLunar<0 || c3:aniCotizati<0 || c4:crestereAnualaSalariu<0
    // Restul deciziilor (D2-D6) sunt simple — condition coverage = branch coverage pentru ele.
    // C# evalueaza "||" cu short-circuit — daca c1=true, c2/c3/c4 nu mai sunt evaluate.
    // Astfel:
    /*  Test                 │ c1  │    c2     │    c3     │    c4   
        EP1(varsta=-2)       │ T   │ neevaluat │ neevaluat │ neevaluat 
        EP2 (salariu=-10000) │ F   │ T         │ neevaluat │ neevaluat 
        EP3 (ani=-10)        │ F   │ F         │ T         │ neevaluat
        EP4 (crestere=-2)    │ F   │ F         │ F         │ T   
        Orice test valid     │ F   │ F         │ F         │ F   
    */
    //Toate conditiile iau si true si false — nu sunt necesare teste noi.

  
    // ACOPERIRE LA NIVEL DE CIRCUITE INDEPENDENTE (Basis Path)
    // V(G) = e − n + 2,
    // unde avem un graf complet conectat G cu e arce și n noduri, iar V(G) - numărul de circuite linear independente
    // In cazul nostru (conform CFG), e = 23, n = 18, V(G) = 23-18+2 = 7
    /*   Deciziile din cod:
        - D1: parametri invalizi?
        - D2: aniRamasi > 0?
        - D3: totalAniCotizare < 15?
        - D4: i <= aniRamasi? (loop)
        - D5: aniCotizati + i > 35? (bonus)
        - D6: totalAniCotizare >= 35?

        Cele 7 circuite independente sunt astfel:
        P1: D1=true - exceptie
        P2: D1=false, D2=true, D3=true - return 0
        P3: D1=false, D2=false, D3=true - return 0
        P4: D1=false, D2=false, D3=false, D4=false, D6=true - pensie completa fara loop
        P5: D1=false, D2=true, D3=false, D4=true, D5=false, D6=true - pensie completa fara bonus
        P6: D1=false, D2=true, D3=false, D4=true, D5=true, D6=true - pensie completa cu bonus
        P7: D1=false, D2=true, D3=false, D4=true, D5=false, D6=false - pensie redusa 
        
        Pentru:
        P1 - EP1, 
        P2 - EP7, 
        P3 - BP1 (mai jos)
        P4 - EP5, 
        P5 - EP9, 
        P6 - BVA4, 
        P7 - EP8
    */
    [TestMethod]
    public void BP1_P3_AniRamasiNegativ_StagiuSubMinim() // parametri valizi, aniRamasi<0, totalAniCotizare < 15
    {
        // varstaCurenta=66: aniRamasi=-1, totalAniCotizare=10
        double result = CalculatorPensie.CalculPensie(66, 7500, 10, 0.0);
        Assert.AreEqual(0, result);
    }

    // Mutanti
    /*
        Neechivalent — comportament diferit de original - putem scrie un test care il detecteaza si il "omoram"
        Echivalent — comportament identic cu originalul pe orice input - nu putem scrie niciun test care sa il detecteze astfel raman "in viata" pentru totdeauna
        
        Mutant1: varstaCurenta <= 0
        Ce schimba: < devine <=  
        Echivalent?: Neechivalent
        
        Mutant2 salariuLunar <= 0
        Ce schimba: < devine <=
        Echivalent?: Neechivalent
        
        Mutant3: Exception("")
        Ce schimba: schimba mesajul
        Echivalent?: Echivalent (nu testam mesajul)
        
        Mutant4: aniRamasi >= 0
        Ce schimba: > devine >=
        Echivalent?: Echivalent (aniRamasi=0 aduna 0, rezultat identic)
        
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

        De asemenea 2 mutanti au cauzat timeout (loop infinit) si sunt considerati omorati automat de Stryker.
    */

   // Adugam teste care "omoara" cei 6 mutanti neechivalenti:

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
}
