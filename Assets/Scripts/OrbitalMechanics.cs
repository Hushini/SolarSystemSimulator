using System;
using UnityEngine;

/*
    Silnik obliczeń orbitalnych. Wyznacza heliocentryczną pozycję planety
    oraz punkty jej pełnej orbity. Metoda wg JPL:
    https://ssd.jpl.nasa.gov/planets/approx_pos.html
*/
public static class OrbitalMechanics
{
    private const double Deg2Rad = Math.PI / 180.0;
    private const double Rad2Deg = 180.0 / Math.PI;

    public static Vector3 GetHeliocentricPosition(Planet planet, DateTime dateUtc)
    {
        double T = AstronomyTime.CenturiesSinceJ2000(dateUtc);
        return GetHeliocentricPosition(planet, T);
    }

    public static Vector3 GetHeliocentricPosition(Planet planet, double T)
    {
        OrbitalElements el = PlanetaryData.GetElements(planet).GetElementsAt(T);

        double omega = el.longitudeOfPerihelion - el.longitudeOfAscendingNode;
        double M = NormalizeDegrees180(el.meanLongitude - el.longitudeOfPerihelion);
        double E = SolveKepler(M, el.eccentricity);

        double a = el.semiMajorAxis;
        double e = el.eccentricity;
        double eRad = E * Deg2Rad;
        double xOrbit = a * (Math.Cos(eRad) - e);
        double yOrbit = a * Math.Sqrt(1.0 - e * e) * Math.Sin(eRad);

        return OrbitalToEcliptic(xOrbit, yOrbit, omega, el.longitudeOfAscendingNode, el.inclination);
    }

    /*
        Zwraca punkty pełnej orbity planety (w AU, współrzędne ekliptyki).
        Powstają przez przejście anomalii mimośrodowej E przez pełne 360 stopni.
    */
    public static Vector3[] GetOrbitPoints(Planet planet, double T, int segments)
    {
        OrbitalElements el = PlanetaryData.GetElements(planet).GetElementsAt(T);
        double a = el.semiMajorAxis;
        double e = el.eccentricity;
        double omega = el.longitudeOfPerihelion - el.longitudeOfAscendingNode;

        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            double E = (2.0 * Math.PI) * i / segments;   // anomalia mimośrodowa 0,..,2 pi
            double xOrbit = a * (Math.Cos(E) - e);
            double yOrbit = a * Math.Sqrt(1.0 - e * e) * Math.Sin(E);
            points[i] = OrbitalToEcliptic(xOrbit, yOrbit, omega,
                                          el.longitudeOfAscendingNode, el.inclination);
        }
        return points;
    }

    // Obraca pozycję z płaszczyzny orbity do płaszczyzny ekliptyki J2000.
    private static Vector3 OrbitalToEcliptic(double xOrbit, double yOrbit,
                                             double omegaDeg, double nodeDeg, double inclDeg)
    {
        double w = omegaDeg * Deg2Rad;
        double O = nodeDeg * Deg2Rad;
        double I = inclDeg * Deg2Rad;

        double cosW = Math.Cos(w), sinW = Math.Sin(w);
        double cosO = Math.Cos(O), sinO = Math.Sin(O);
        double cosI = Math.Cos(I), sinI = Math.Sin(I);

        double xEcl = (cosW * cosO - sinW * sinO * cosI) * xOrbit
                    + (-sinW * cosO - cosW * sinO * cosI) * yOrbit;
        double yEcl = (cosW * sinO + sinW * cosO * cosI) * xOrbit
                    + (-sinW * sinO + cosW * cosO * cosI) * yOrbit;
        double zEcl = (sinW * sinI) * xOrbit
                    + (cosW * sinI) * yOrbit;

        return new Vector3((float)xEcl, (float)yEcl, (float)zEcl);
    }

    private static double SolveKepler(double M, double e)
    {
        double eStar = Rad2Deg * e;
        double E = M + eStar * Math.Sin(M * Deg2Rad);

        double deltaE;
        int safety = 100;
        do
        {
            double deltaM = M - (E - eStar * Math.Sin(E * Deg2Rad));
            deltaE = deltaM / (1.0 - e * Math.Cos(E * Deg2Rad));
            E += deltaE;
            safety--;
        }
        while (Math.Abs(deltaE) > 1e-6 && safety > 0);

        return E;
    }

    private static double NormalizeDegrees180(double angle)
    {
        angle %= 360.0;
        if (angle > 180.0) angle -= 360.0;
        else if (angle < -180.0) angle += 360.0;
        return angle;
    }
}