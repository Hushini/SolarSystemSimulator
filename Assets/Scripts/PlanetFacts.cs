using System.Collections.Generic;

// Tabela danych fizycznych i opisowych ośmiu planet.
    
public static class PlanetFacts
{
    private static readonly Dictionary<Planet, PlanetInfo> table =
        new Dictionary<Planet, PlanetInfo>
    {
        { Planet.Mercury, new PlanetInfo {
            polishName = "Merkury",
            massKg = 3.30e23,
            radiusKm = 2439.5,
            surfaceGravity = 3.7,
            orbitalPeriodDays = 88.0,
            rotationPeriodHours = 1407.6,
            distanceFromSunMln = 57.9,
            moons = 0,
            description = "Najmniejsza i najbliższa Słońcu planeta. Niemal pozbawiona " +
                          "atmosfery, jej powierzchnia jest gęsto pokryta kraterami, " +
                          "a temperatury skrajnie się wahają."
        }},
        { Planet.Venus, new PlanetInfo {
            polishName = "Wenus",
            massKg = 4.87e24,
            radiusKm = 6052.0,
            surfaceGravity = 8.9,
            orbitalPeriodDays = 224.7,
            rotationPeriodHours = -5832.5,
            distanceFromSunMln = 108.2,
            moons = 0,
            description = "Druga planeta od Słońca, podobnej wielkości co Ziemia. Gęsta " +
                          "atmosfera dwutlenku węgla wywołuje silny efekt cieplarniany — " +
                          "to najgorętsza planeta Układu."
        }},
        { Planet.Earth, new PlanetInfo {
            polishName = "Ziemia",
            massKg = 5.97e24,
            radiusKm = 6378.0,
            surfaceGravity = 9.8,
            orbitalPeriodDays = 365.2,
            rotationPeriodHours = 23.9,
            distanceFromSunMln = 149.6,
            moons = 1,
            description = "Trzecia planeta od Słońca i jedyna znana z występowaniem życia. " +
                          "Powierzchnię w większości pokrywa woda w stanie ciekłym."
        }},
        { Planet.Mars, new PlanetInfo {
            polishName = "Mars",
            massKg = 6.42e23,
            radiusKm = 3396.0,
            surfaceGravity = 3.7,
            orbitalPeriodDays = 687.0,
            rotationPeriodHours = 24.6,
            distanceFromSunMln = 228.0,
            moons = 2,
            description = "Czwarta planeta, zwana Czerwoną Planetą ze względu na bogatą " +
                          "w tlenki żelaza powierzchnię. Ma cienką atmosferę i dwa " +
                          "niewielkie księżyce."
        }},
        { Planet.Jupiter, new PlanetInfo {
            polishName = "Jowisz",
            massKg = 1.898e27,
            radiusKm = 71492.0,
            surfaceGravity = 23.1,
            orbitalPeriodDays = 4331.0,
            rotationPeriodHours = 9.9,
            distanceFromSunMln = 778.5,
            moons = 63,
            description = "Największa planeta Układu Słonecznego, gazowy olbrzym. " +
                          "Charakterystyczna Wielka Czerwona Plama to ogromny, trwający " +
                          "od stuleci sztorm."
        }},
        { Planet.Saturn, new PlanetInfo {
            polishName = "Saturn",
            massKg = 5.68e26,
            radiusKm = 60268.0,
            surfaceGravity = 9.0,
            orbitalPeriodDays = 10747.0,
            rotationPeriodHours = 10.7,
            distanceFromSunMln = 1432.0,
            moons = 60,
            description = "Gazowy olbrzym znany ze spektakularnego systemu pierścieni " +
                          "zbudowanych z lodu i skał. Ma najniższą średnią gęstość " +
                          "spośród planet."
        }},
        { Planet.Uranus, new PlanetInfo {
            polishName = "Uran",
            massKg = 8.68e25,
            radiusKm = 25559.0,
            surfaceGravity = 8.7,
            orbitalPeriodDays = 30589.0,
            rotationPeriodHours = -17.2,
            distanceFromSunMln = 2867.0,
            moons = 27,
            description = "Lodowy olbrzym obracający się niemal „na boku - jego oś " +
                          "obrotu jest silnie nachylona. Ma bladoniebieską barwę " +
                          "pochodzącą od metanu w atmosferze."
        }},
        { Planet.Neptune, new PlanetInfo {
            polishName = "Neptun",
            massKg = 1.02e26,
            radiusKm = 24764.0,
            surfaceGravity = 11.0,
            orbitalPeriodDays = 59800.0,
            rotationPeriodHours = 16.1,
            distanceFromSunMln = 4515.0,
            moons = 13,
            description = "Najdalsza planeta Układu Słonecznego, lodowy olbrzym " +
                          "o intensywnie niebieskiej barwie. Wieją na nim najsilniejsze " +
                          "wiatry w całym Układzie."
        }},
    };

    // Zwraca dane wskazanej planety.
    public static PlanetInfo Get(Planet planet) => table[planet];
}