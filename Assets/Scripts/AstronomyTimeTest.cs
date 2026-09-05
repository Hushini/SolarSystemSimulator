using System;
using UnityEngine;

public class AstronomyTimeTest : MonoBehaviour
{
    void Start()
    {
        // Test 1: epoka J2000 powinna dać dokładnie 2451545.0
        DateTime j2000 = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        Debug.Log($"JD dla J2000 (oczekiwane 2451545.0): {AstronomyTime.ToJulianDate(j2000)}");

        // Test 2: liczba stuleci od J2000 dla tej samej daty powinna wynosić 0
        Debug.Log($"T dla J2000 (oczekiwane 0): {AstronomyTime.CenturiesSinceJ2000(j2000)}");

        // Test 3: dzisiejsza data i godzina
        DateTime now = DateTime.UtcNow;
        Debug.Log($"Dzis ({now:yyyy-MM-dd HH:mm} UTC): " +
                  $"JD = {AstronomyTime.ToJulianDate(now)}, " +
                  $"T = {AstronomyTime.CenturiesSinceJ2000(now):F6}");
    }
}