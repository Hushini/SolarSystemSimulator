using UnityEngine;

// Dane fizyczne i opisowe pojedynczej planety.

public class PlanetInfo
{
    public string polishName;          // nazwa polska
    public double massKg;              // masa [kg]
    public double radiusKm;            // promień średni [km]
    public double surfaceGravity;      // przyspieszenie grawitacyjne przy powierzchni [m/s^2]
    public double orbitalPeriodDays;   // okres obiegu wokół Słońca [dni]
    public double rotationPeriodHours; // okres obrotu wokół osi [godziny]; ujemny = obrót wsteczny
    public double distanceFromSunMln;  // średnia odległość od Słońca [mln km]
    public int moons;                  // liczba znanych księżyców
    public string description;         // krótki opis
}
