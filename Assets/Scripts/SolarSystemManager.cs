using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/*
    Tworzy Słońce, planety, pierścień Saturna i linie orbit z modeli z blendera,
    tworzy animacje ruchu planet i przełącza wygląd realistyczny / low-poly.
*/
public class SolarSystemManager : MonoBehaviour
{
    [Header("Skalowanie")]
    public float distanceScale = 10f;

    [Tooltip("Rozmiar planety = promień [km] * ten współczynnik. " +
             "Po normalizacji wartości są w jednostkach świata.")]
    public float planetSizeScale = 0.0001f;

    [Tooltip("Średnica kuli Słońca w jednostkach świata.")]
    public float sunVisualSize = 4f;

    [Header("Czas")]
    public string startDateUtc = "";
    public double timeMultiplier = 1.0;
    public bool isPaused = false;

    [Header("Orbity")]
    public bool showOrbits = true;
    public float orbitWidth = 0.3f;

    [Header("Wyglad")]
    [Tooltip("Tryb low-poly (przelacznik: klawisz L).")]
    public bool useLowPoly = false;

    private const int OrbitSegments = 128;

    private DateTime simulationTime;
    private readonly List<PlanetBody> planets = new List<PlanetBody>();
    private readonly List<GameObject> orbitLines = new List<GameObject>();
    private readonly Dictionary<PlanetBody, double> rotationAngles = new Dictionary<PlanetBody, double>();

    private Mesh realisticMesh;
    private Mesh lowPolyMesh;
    private GameObject sunObject;
    private GameObject ringObject;
    private PlanetBody saturnBody;

    // 8 osobnych modeli low-poly (po jednym na planetę), wczytywanych z Resources/Models
    private readonly Dictionary<Planet, GameObject> lowPolyModels = new Dictionary<Planet, GameObject>();

    private GameObject sunLowPolyModel;   // osobny model low-poly Słońca (Models/SunLowPoly)

    public DateTime SimulationTime => simulationTime;

    void Start()
    {
        SkyboxBuilder.Apply();
        
        GameSettings.Load();
        useLowPoly  = GameSettings.startLowPoly;
        showOrbits  = GameSettings.showOrbits;
        
        LoadAssets();
        simulationTime = ParseStartDate();
        CreateOrbits();
        BuildBodies();
        UpdatePositions();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) isPaused = !isPaused;
        if (Input.GetKeyDown(KeyCode.O)) showOrbits = !showOrbits;
        if (Input.GetKeyDown(KeyCode.L)) SetVisualMode(!useLowPoly);

        if (!isPaused)
            simulationTime = simulationTime.AddSeconds(timeMultiplier * Time.deltaTime);

        UpdatePositions();
        ApplyOrbitVisibility();
    }

    private void LoadAssets()
    {
        GameObject realisticPrefab = Resources.Load<GameObject>("Models/PlanetSphere");
        GameObject lowPolyPrefab = Resources.Load<GameObject>("Models/PlanetLowPoly");

        if (realisticPrefab != null)
            realisticMesh = realisticPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;
        if (lowPolyPrefab != null)
            lowPolyMesh = lowPolyPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;

        // osobne modele low-poly per planeta: Models/EarthLowPoly, Models/MarsLowPoly, ...
        lowPolyModels.Clear();
        foreach (Planet p in Enum.GetValues(typeof(Planet)))
        {
            GameObject lp = Resources.Load<GameObject>("Models/" + p.ToString() + "LowPoly");
            if (lp != null) lowPolyModels[p] = lp;
            else Debug.LogWarning("Brak modelu low-poly: Models/" + p + "LowPoly");
        }

        // osobny model low-poly Słońca
        sunLowPolyModel = Resources.Load<GameObject>("Models/SunLowPoly");
        if (sunLowPolyModel == null) Debug.LogWarning("Brak modelu low-poly: Models/SunLowPoly");

        if (realisticMesh == null || lowPolyMesh == null)
            Debug.LogError("SolarSystemManager: brak modeli w Resources/Models " +
                           "(PlanetSphere i/lub PlanetLowPoly).");
    }

    public void SetVisualMode(bool lowPoly)
    {
        useLowPoly = lowPoly;
        BuildBodies();
        UpdatePositions();
    }

    private void BuildBodies()
    {
        if (sunObject != null) Destroy(sunObject);
        if (ringObject != null) Destroy(ringObject);
        foreach (PlanetBody b in planets)
            if (b != null) Destroy(b.gameObject);
        planets.Clear();
        rotationAngles.Clear();
        saturnBody = null;

        Mesh mesh = useLowPoly ? lowPolyMesh : realisticMesh;
        if (mesh == null) return;

        // bazowy rozmiar siatki (cokolwiek FBX zaimportował)
        float meshRadius = mesh.bounds.extents.x;
        float meshDiameter = meshRadius * 2f;
        if (meshDiameter <= 0f) meshDiameter = 1f;

        CreateSun(mesh, meshDiameter);

        foreach (Planet p in Enum.GetValues(typeof(Planet)))
        {
            // wybór siatki i materiałów:
            // tryb low-poly: osobny model danej planety (własna siatka + kolory/materiały z Blendera)
            // w przeciwnym razie (albo brak modelu): wspólna siatka + tekstura/kolor z PlanetMaterial
            Mesh pMesh = mesh;
            Material[] pMats;

            if (useLowPoly && lowPolyModels.TryGetValue(p, out GameObject lp) && lp != null)
            {
                MeshFilter mf = lp.GetComponentInChildren<MeshFilter>();
                MeshRenderer mr = lp.GetComponentInChildren<MeshRenderer>();
                if (mf != null && mf.sharedMesh != null) pMesh = mf.sharedMesh;
                pMats = (mr != null && mr.sharedMaterials.Length > 0)
                      ? mr.sharedMaterials
                      : new[] { PlanetMaterial(p) };
            }
            else
            {
                pMats = new[] { PlanetMaterial(p) };
            }

            // rozmiar bazowy tej konkretnej siatki (różne modele mogą mieć różną skalę)
            float pMeshRadius = pMesh.bounds.extents.x;
            float pMeshDiameter = pMeshRadius * 2f;
            if (pMeshDiameter <= 0f) pMeshDiameter = 1f;

            // żądana średnica świata: promień planety [km] * współczynnik * 2
            float worldDiameter = (float)(PlanetFacts.Get(p).radiusKm * planetSizeScale * 2.0);
            // normalizacja: localScale tak, by realny rozmiar = worldDiameter
            float scale = worldDiameter / pMeshDiameter;

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = p.ToString();
            obj.transform.SetParent(transform);
            obj.transform.localScale = Vector3.one * scale;
            obj.GetComponent<MeshFilter>().sharedMesh = pMesh;

            SphereCollider sc = obj.GetComponent<SphereCollider>();
            sc.radius = pMeshRadius;
            sc.center = pMesh.bounds.center;

            obj.GetComponent<Renderer>().sharedMaterials = pMats;

            PlanetBody body = obj.AddComponent<PlanetBody>();
            body.planet = p;
            planets.Add(body);
            rotationAngles[body] = 0.0;

            if (p == Planet.Saturn)
            {
                saturnBody = body;
                CreateRing(worldDiameter * 0.5f);   // promień Saturna w świecie
            }
        }
    }

    private void CreateSun(Mesh mesh, float meshDiameter)
    {
        // domyślnie: wspólna kula + materiał Słońca (tryb realistyczny / brak modelu)
        Mesh sunMesh = mesh;
        float sunMeshDiameter = meshDiameter;
        Material[] sunMats = new[] { SunMaterial() };

        // tryb low-poly: użyj osobnego modelu Słońca (własna siatka + materiały z blendera)
        if (useLowPoly && sunLowPolyModel != null)
        {
            MeshFilter mf = sunLowPolyModel.GetComponentInChildren<MeshFilter>();
            MeshRenderer mr = sunLowPolyModel.GetComponentInChildren<MeshRenderer>();
            if (mf != null && mf.sharedMesh != null)
            {
                sunMesh = mf.sharedMesh;
                float d = sunMesh.bounds.extents.x * 2f;
                if (d > 0f) sunMeshDiameter = d;
            }
            if (mr != null && mr.sharedMaterials.Length > 0)
                sunMats = mr.sharedMaterials;
        }

        GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sun.name = "Sun";
        sun.transform.SetParent(transform);
        sun.transform.position = Vector3.zero;
        sun.transform.localScale = Vector3.one * (sunVisualSize / sunMeshDiameter);
        sun.GetComponent<MeshFilter>().sharedMesh = sunMesh;
        sun.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        sun.GetComponent<Renderer>().sharedMaterials = sunMats;
        sunObject = sun;
    }

    private void CreateRing(float saturnWorldRadius)
    {
        Material ringMat = Resources.Load<Material>("Materials/SaturnRing");

        GameObject ring = new GameObject("Saturn Ring");
        ring.transform.SetParent(transform);
        ring.transform.rotation = Quaternion.Euler(0f, 0f, 26.7f);

        SaturnRing sr = ring.AddComponent<SaturnRing>();
        sr.innerRadius = saturnWorldRadius * 1.4f;
        sr.outerRadius = saturnWorldRadius * 2.4f;
        sr.segments = 96;
        sr.ringMaterial = ringMat;

        ringObject = ring;
    }

    private Material PlanetMaterial(Planet p)
    {
        if (useLowPoly)
            return LitColor(GetPlaceholderColor(p));

        Texture tex = Resources.Load<Texture2D>("Textures/" + p.ToString().ToLower());
        return tex != null ? LitTexture(tex) : LitColor(GetPlaceholderColor(p));
    }

    private Material SunMaterial()
    {
        if (useLowPoly)
            return UnlitColor(new Color(1f, 0.85f, 0.2f));

        Texture tex = Resources.Load<Texture2D>("Textures/sun");
        return tex != null ? UnlitTexture(tex) : UnlitColor(new Color(1f, 0.85f, 0.2f));
    }

    private Material LitTexture(Texture tex)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetTexture("_BaseMap", tex);
        m.SetFloat("_Smoothness", 0.1f);
        return m;
    }


    private Material LitColor(Color c)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.color = c;
        m.SetFloat("_Smoothness", 0.1f);
        return m;
    }

    private Material UnlitTexture(Texture tex)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        m.SetTexture("_BaseMap", tex);
        return m;
    }

    private Material UnlitColor(Color c)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        m.color = c;
        return m;
    }

    private void CreateOrbits()
    {
        double T = AstronomyTime.CenturiesSinceJ2000(simulationTime);

        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlit == null) unlit = Shader.Find("Sprites/Default");

        foreach (Planet p in Enum.GetValues(typeof(Planet)))
        {
            GameObject orbitGO = new GameObject(p + " Orbit");
            orbitGO.transform.SetParent(transform);

            LineRenderer line = orbitGO.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = false;
            line.startWidth = orbitWidth;
            line.endWidth = orbitWidth;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            Color c = GetPlaceholderColor(p);
            Material mat = new Material(unlit);
            mat.color = new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, 1f);
            line.material = mat;

            Vector3[] auPoints = OrbitalMechanics.GetOrbitPoints(p, T, OrbitSegments);
            line.positionCount = auPoints.Length;
            for (int i = 0; i < auPoints.Length; i++)
                line.SetPosition(i, EclipticToUnity(auPoints[i]));

            orbitLines.Add(orbitGO);
        }
    }

    private void ApplyOrbitVisibility()
    {
        foreach (GameObject orbit in orbitLines)
            orbit.SetActive(showOrbits);
    }

    public void UpdatePositions()
    {
        foreach (PlanetBody body in planets)
        {
            // pozycja na orbicie
            Vector3 helio = OrbitalMechanics.GetHeliocentricPosition(body.planet, simulationTime);
            body.transform.position = EclipticToUnity(helio);

            // obrót wokół własnej osi
            double periodHours = PlanetFacts.Get(body.planet).rotationPeriodHours;
            if (periodHours != 0.0 && !isPaused)
            {
                double degPerSimSecond = 360.0 / (periodHours * 3600.0);
                rotationAngles[body] += degPerSimSecond * timeMultiplier * Time.deltaTime;
            }
            body.transform.localRotation =
                Quaternion.Euler(0f, (float)rotationAngles[body], 0f) *  // obrót wokół osi planety (świat Y)
                Quaternion.Euler(-90f, 0f, 0f);                          // bazowe wyprostowanie modelu z blendera
        }
        if (ringObject != null && saturnBody != null)
            ringObject.transform.position = saturnBody.transform.position;
    }

    public void SetTime(DateTime utc) => simulationTime = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
    public void JumpToNow() => simulationTime = DateTime.UtcNow;
    public void SetOrbitsVisible(bool visible) => showOrbits = visible;

    private Vector3 EclipticToUnity(Vector3 ecliptic)
    {
        return new Vector3(ecliptic.x, ecliptic.z, ecliptic.y) * distanceScale;
    }

    private DateTime ParseStartDate()
    {
        if (string.IsNullOrWhiteSpace(startDateUtc))
            return DateTime.UtcNow;

        if (DateTime.TryParse(startDateUtc, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal, out DateTime parsed))
            return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

        Debug.LogWarning($"Nie udalo sie odczytac daty '{startDateUtc}', uzywam biezacej.");
        return DateTime.UtcNow;
    }

    private Color GetPlaceholderColor(Planet p)
    {
        switch (p)
        {
            case Planet.Mercury: return new Color(0.55f, 0.55f, 0.55f);
            case Planet.Venus:   return new Color(0.93f, 0.85f, 0.60f);
            case Planet.Earth:   return new Color(0.20f, 0.45f, 0.85f);
            case Planet.Mars:    return new Color(0.80f, 0.35f, 0.20f);
            case Planet.Jupiter: return new Color(0.80f, 0.65f, 0.50f);
            case Planet.Saturn:  return new Color(0.85f, 0.78f, 0.60f);
            case Planet.Uranus:  return new Color(0.60f, 0.85f, 0.90f);
            case Planet.Neptune: return new Color(0.25f, 0.40f, 0.80f);
            default:             return Color.white;
        }
    }
}