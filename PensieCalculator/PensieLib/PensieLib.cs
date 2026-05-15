namespace PensieLib;

public static class CalculatorPensie
{
    private const double SalariuMediuEconomie = 7500.0;  // RON, 
    private const double ValoarePunctPensie = 81.0;   // Valoarea oficială din lege in RON
    private const int VarstaPensionare = 65;

        public static double CalculPensie(int varstaCurenta, int salariuLunar, int aniCotizati, double crestereAnualaSalariu)
            {
                //conditie compusa
                if (varstaCurenta < 0 || salariuLunar < 0 || aniCotizati < 0 || crestereAnualaSalariu <0) 
                    throw new ArgumentException("Parametrii invalizi.");
                
                int aniRamasi = VarstaPensionare - varstaCurenta;
                int totalAniCotizare = aniCotizati;
                
                //if fara else: varsta mai mare decat varsta de pensionare
                if (aniRamasi > 0)
                     totalAniCotizare = aniCotizati + aniRamasi;

                // if fara else: sub stagiul minim nu se acorda pensie
                if (totalAniCotizare < 15)
                    return 0;
                // puncte estimate din trecut (aproximare cu salariul curent)
                double totalPuncte = aniCotizati * (salariuLunar / SalariuMediuEconomie);

                // loop — simuleaza fiecare an ramas pana la pensionare
                double salariuLunarSimulat = salariuLunar;
                for (int i = 1; i <= aniRamasi; i++)
                    {
                    double puncteAn = salariuLunarSimulat / SalariuMediuEconomie;
                    totalPuncte += puncteAn;
                    
                    // if fara else — bonus 5% pentru fiecare an de cotizare peste 35
                    if (aniCotizati + i > 35)
                        totalPuncte += puncteAn * 0.05;

                    salariuLunarSimulat *= (1 + crestereAnualaSalariu / 100.0);
                    }
                    
            // if cu else: pensie completa sau redusa
            if (totalAniCotizare >= 35)
                return totalPuncte * ValoarePunctPensie;
            else
                return totalPuncte * ValoarePunctPensie * 0.75;
          }
            
}
