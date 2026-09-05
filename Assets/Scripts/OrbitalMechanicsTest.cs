using System;
using UnityEngine;

public class OrbitalMechanicsTest : MonoBehaviour
{
    void Start()
    {
        // Test 1: odległość każdej planety od Słońca dzisiaj.
        // Powinna być zbliżona do wielkiej półosi a danej planety.
        double T = AstronomyTime.CenturiesSinceJ2000(DateTime.UtcNow);
        Debug.Log("Odleglosci planet od Slonca [AU]");
        foreach (Planet p in Enum.GetValues(typeof(Planet)))
        {
            Vector3 pos = OrbitalMechanics.GetHeliocentricPosition(p, T);
            Debug.Log($"{p}: odleglosc = {pos.magnitude:F3} AU");
        }

        // Test 2: pozycja Ziemi w epoce J2000.
        Vector3 earthJ2000 = OrbitalMechanics.GetHeliocentricPosition(Planet.Earth, 0.0);
        Debug.Log($"Ziemia w J2000: pozycja = {earthJ2000}, " +
                  $"odleglosc = {earthJ2000.magnitude:F4} AU (około 0.983)");
    }
}