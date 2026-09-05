using UnityEngine;


public enum Planet
{
    Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune
}

// Sześć elementów orbity Keplera.
public struct OrbitalElements
{
    public double semiMajorAxis;            // a  - półoś wielka [AU]
    public double eccentricity;             // e  - mimośród [bez jednostki]
    public double inclination;              // I  - nachylenie orbity [stopnie]
    public double meanLongitude;            // L  - średnia długość [stopnie]
    public double longitudeOfPerihelion;    // ϖ  - długość peryhelium [stopnie]
    public double longitudeOfAscendingNode; // Ω  - długość węzła wstępującego [stopnie]
}

/*
    Elementy orbitalne Keplera planety wraz z tempem ich zmian na stulecie.
    Wartości dla epoki J2000.0; źródło: JPL (Standish & Williams 1992).
*/
public class KeplerianElements
{
    // Wartości w epoce J2000.0
    private readonly double a0, e0, I0, L0, longPeri0, longNode0;
    // Tempo zmian na jedno stulecie juliańskie
    private readonly double aRate, eRate, IRate, LRate, longPeriRate, longNodeRate;

    public KeplerianElements(
        double a0, double aRate,
        double e0, double eRate,
        double I0, double IRate,
        double L0, double LRate,
        double longPeri0, double longPeriRate,
        double longNode0, double longNodeRate)
    {
        this.a0 = a0; this.aRate = aRate;
        this.e0 = e0; this.eRate = eRate;
        this.I0 = I0; this.IRate = IRate;
        this.L0 = L0; this.LRate = LRate;
        this.longPeri0 = longPeri0; this.longPeriRate = longPeriRate;
        this.longNode0 = longNode0; this.longNodeRate = longNodeRate;
    }

    /*
        Oblicza elementy orbity dla chwili oddalonej o T stuleci od J2000.0.
        Wzór: element = wartość_J2000 + tempo * T.
    */
    public OrbitalElements GetElementsAt(double centuriesSinceJ2000)
    {
        double T = centuriesSinceJ2000;
        return new OrbitalElements
        {
            semiMajorAxis            = a0        + aRate        * T,
            eccentricity             = e0        + eRate        * T,
            inclination              = I0        + IRate        * T,
            meanLongitude            = L0        + LRate        * T,
            longitudeOfPerihelion    = longPeri0 + longPeriRate * T,
            longitudeOfAscendingNode = longNode0 + longNodeRate * T
        };
    }
}
