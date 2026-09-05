using System;

/*
    Narzędzia do obliczeń astronomicznych związanych z czasem.
    Zamienia datę kalendarzową na datę juliańską (JD) oraz na liczbę stuleci juliańskich od epoki J2000.0.
*/
public static class AstronomyTime
{
    // Data juliańska epoki J2000.0 (1 stycznia 2000 godzina 12:00 UTC)
    public const double J2000 = 2451545.0;

    // Liczba dni w jednym stuleciu juliańskim
    public const double DaysPerCentury = 36525.0;

    /*
        Oblicza datę juliańską dla podanej daty i godziny w UTC.
        Zakłada kalendarz gregoriański (zakres: lata 1800–2050).
    */
    public static double ToJulianDate(DateTime dateUtc)
    {
        int year = dateUtc.Year;
        int month = dateUtc.Month;

        // Ułamek dnia wynikający z godziny, minut i sekund
        double dayFraction = (dateUtc.Hour + dateUtc.Minute / 60.0 + dateUtc.Second / 3600.0) / 24.0;
        double day = dateUtc.Day + dayFraction;

        // Styczeń i luty traktujemy jako 13. i 14. miesiąc roku poprzedniego
        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        // Poprawka kalendarza gregoriańskiego (reguła lat przestępnych)
        int a = year / 100;
        int b = 2 - a + a / 4;

        double jd = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + b - 1524.5;

        return jd;
    }

    /*
        Zwraca liczbę stuleci juliańskich od epoki J2000.0.
        Wartość T jest argumentem wzorów na elementy orbitalne planet.
    */
    public static double CenturiesSinceJ2000(DateTime dateUtc)
    {
        double jd = ToJulianDate(dateUtc);
        return (jd - J2000) / DaysPerCentury;
    }
}
