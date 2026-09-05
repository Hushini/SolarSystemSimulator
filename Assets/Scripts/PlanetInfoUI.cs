using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


// Obsługuje wybór planety kliknięciem, panel z jej danymi oraz przejście do sceny testu grawitacji.
public class PlanetInfoUI : MonoBehaviour
{
    private Camera cam;
    private Font font;

    private GameObject panel;
    private Text nameText;
    private Text statsText;
    private Text descriptionText;

    private Planet currentPlanet = Planet.Earth;

    void Start()
    {
        cam = Camera.main;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
        panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    private void HandleClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlanetBody body = hit.collider.GetComponent<PlanetBody>();
            if (body != null)
                ShowPlanet(body.planet);
        }
    }

    public void ShowPlanet(Planet planet)
    {
        currentPlanet = planet;
        PlanetInfo info = PlanetFacts.Get(planet);
        nameText.text = info.polishName;
        statsText.text = BuildStatsText(info);
        descriptionText.text = info.description;
        panel.SetActive(true);
    }

    // Zapamiętuje wybraną planetę i przechodzi do sceny testu grawitacji.
    private void OpenGravityTest()
    {
        GravityTestContext.selectedPlanet = currentPlanet;
        SceneManager.LoadScene("GravityTest");
    }

    private string BuildStatsText(PlanetInfo info)
    {
        string rotation = info.rotationPeriodHours < 0
            ? $"{Math.Abs(info.rotationPeriodHours):0.#} h (obrot wsteczny)"
            : $"{info.rotationPeriodHours:0.#} h";

        string orbit = info.orbitalPeriodDays > 365.0
            ? $"{info.orbitalPeriodDays:0.#} dni (~{info.orbitalPeriodDays / 365.25:0.#} lat)"
            : $"{info.orbitalPeriodDays:0.#} dni";

        return
            $"Masa:  {FormatMass(info.massKg)}\n" +
            $"Promien:  {info.radiusKm:0} km\n" +
            $"Grawitacja przy powierzchni:  {info.surfaceGravity:0.#} m/s\u00B2\n" +
            $"Okres obiegu:  {orbit}\n" +
            $"Okres obrotu:  {rotation}\n" +
            $"Srednia odleglosc od Slonca:  {info.distanceFromSunMln:0.#} mln km\n" +
            $"Liczba ksiezycow:  {info.moons}";
    }

    private string FormatMass(double kg)
    {
        int exp = (int)Math.Floor(Math.Log10(kg));
        double mantissa = kg / Math.Pow(10.0, exp);
        return $"{mantissa:0.00} \u00D7 10^{exp} kg";
    }

    // Budowa interfejsu

    private void BuildUI()
    {
        EnsureEventSystem();

        GameObject canvasGO = new GameObject("PlanetInfoCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject panelGO = new GameObject("Panel", typeof(RectTransform));
        RectTransform panelRt = panelGO.GetComponent<RectTransform>();
        panelRt.SetParent(canvas.transform, false);
        panelRt.anchorMin = new Vector2(1, 1);
        panelRt.anchorMax = new Vector2(1, 1);
        panelRt.pivot = new Vector2(1, 1);
        panelRt.anchoredPosition = new Vector2(-15, -15);
        panelRt.sizeDelta = new Vector2(380, 525);
        panelGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
        panel = panelGO;

        nameText = AddText(panelRt, new Vector2(20, -16), new Vector2(300, 44),
                           32, TextAnchor.MiddleLeft);

        Button closeBtn = AddButton(panelRt, new Vector2(330, -16), new Vector2(34, 34),
                                    "X", new Color(0.5f, 0.25f, 0.25f, 1f), out _);
        closeBtn.onClick.AddListener(() => panel.SetActive(false));

        statsText = AddText(panelRt, new Vector2(20, -72), new Vector2(345, 230),
                            17, TextAnchor.UpperLeft);

        descriptionText = AddText(panelRt, new Vector2(20, -312), new Vector2(345, 145),
                                  17, TextAnchor.UpperLeft);

        Button gravityBtn = AddButton(panelRt, new Vector2(20, -470), new Vector2(345, 44),
                                      "Test grawitacji",
                                      new Color(0.22f, 0.40f, 0.30f, 1f), out _);
        gravityBtn.onClick.AddListener(OpenGravityTest);
    }

    private RectTransform NewRect(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    private Text AddText(Transform parent, Vector2 pos, Vector2 size,
                         int fontSize, TextAnchor align)
    {
        RectTransform rt = NewRect("Text", parent, pos, size);
        Text t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private Button AddButton(Transform parent, Vector2 pos, Vector2 size,
                             string label, Color color, out Text labelText)
    {
        RectTransform rt = NewRect("Button", parent, pos, size);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject textGO = new GameObject("Label", typeof(RectTransform));
        RectTransform textRt = textGO.GetComponent<RectTransform>();
        textRt.SetParent(rt, false);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        labelText = textGO.AddComponent<Text>();
        labelText.font = font;
        labelText.fontSize = 20;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.text = label;
        return btn;
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}