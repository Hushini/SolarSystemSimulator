using System;
using UnityEngine;

public class PlanetaryDataTest : MonoBehaviour
{
    void Start()
    {
        // Elementy Ziemi w epoce J2000 (T = 0), muszą dokładnie zgadzać się z tabelą JPL
        KeplerianElements earth = PlanetaryData.GetElements(Planet.Earth);
        OrbitalElements atJ2000 = earth.GetElementsAt(0.0);
        Debug.Log($"Ziemia w J2000: a={atJ2000.semiMajorAxis} AU (oczek. 1.00000261), " +
                  $"e={atJ2000.eccentricity} (oczek. 0.01671123)");

        // Elementy Ziemi dzisiaj
        double T = AstronomyTime.CenturiesSinceJ2000(DateTime.UtcNow);
        OrbitalElements today = earth.GetElementsAt(T);
        Debug.Log($"Ziemia dzis (T={T:F4}): a={today.semiMajorAxis:F8} AU, " +
                  $"L={today.meanLongitude:F2} stopni");
    }
}