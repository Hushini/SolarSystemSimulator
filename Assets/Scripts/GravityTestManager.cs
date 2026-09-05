using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/*
    Scena testu grawitacji z regulacja wysokosci i masy (suwak + pole tekstowe).
    Kamyk spada zgodnie z kinematyka swobodnego spadku (h = 1/2*g*t^2).
*/
public class GravityTestManager : MonoBehaviour
{
    [Header("Wartosci poczatkowe")]
    public float dropHeight = 10f;
    public float pebbleMass = 1f;
    public float pebbleRadius = 0.3f;

    private const float MinHeight = 1f;
    private const float MaxHeight = 30f;
    private const float MinMass = 0.1f;
    private const float MaxMass = 100f;

    private double gravity;
    private PlanetInfo planetInfo;
    private Camera cam;

    private GameObject pebble;
    private const float GroundY = 0f;

    private enum State { Ready, Falling, Landed }
    private State state = State.Ready;
    private float fallTime = 0f;

    private Font font;
    private Text measurementText;
    private Text weightLabel;

    private Slider heightSlider, massSlider;
    private InputField heightInput, massInput;

    void Start()
    {
        planetInfo = PlanetFacts.Get(GravityTestContext.selectedPlanet);
        gravity = planetInfo.surfaceGravity;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        BuildScene();
        BuildUI();
        ResetPebble();
        UpdateCameraForHeight();
    }

    void Update()
    {
        if (state == State.Falling)
        {
            fallTime += Time.deltaTime;
            double fallen = 0.5 * gravity * fallTime * fallTime;
            if (fallen >= dropHeight)
            {
                state = State.Landed;
                fallTime = (float)Math.Sqrt(2.0 * dropHeight / gravity);
                SetPebbleHeight(0f);
            }
            else
            {
                SetPebbleHeight((float)(dropHeight - fallen));
            }
        }
        UpdateMeasurementText();
    }

    // Scena 3D

    private void BuildScene()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, GroundY, 0f);
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.GetComponent<Renderer>().material.color = new Color(0.30f, 0.32f, 0.38f);

        pebble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pebble.name = "Pebble";
        pebble.transform.localScale = Vector3.one * (pebbleRadius * 2f);
        pebble.GetComponent<Renderer>().material.color = new Color(0.85f, 0.80f, 0.70f);

        cam = Camera.main;
        if (cam != null)
        {
            cam.farClipPlane = 300f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
        }
    }

    private void UpdateCameraForHeight()
    {
        if (cam == null) return;
        float verticalSpan = dropHeight + 2f;
        float distance = Mathf.Max(8f, verticalSpan * 0.95f);
        cam.transform.position = new Vector3(0f, dropHeight * 0.5f, -distance);
        cam.transform.LookAt(new Vector3(0f, dropHeight * 0.5f, 0f));
    }

    private void SetPebbleHeight(float h)
    {
        pebble.transform.position = new Vector3(0f, GroundY + pebbleRadius + h, 0f);
    }

    private void ResetPebble()
    {
        state = State.Ready;
        fallTime = 0f;
        SetPebbleHeight(dropHeight);
    }

    private void StartDrop()
    {
        ResetPebble();
        state = State.Falling;
    }

    private void BackToSolarSystem() => SceneManager.LoadScene("SolarSystem");

    // reakcje suwakow i pól

    private void OnHeightChanged(float value)
    {
        dropHeight = value;
        if (heightInput != null && !heightInput.isFocused)
            heightInput.text = value.ToString("0.0", CultureInfo.InvariantCulture);
        ResetPebble();
        UpdateCameraForHeight();
    }

    private void OnMassChanged(float value)
    {
        pebbleMass = value;
        if (massInput != null && !massInput.isFocused)
            massInput.text = value.ToString("0.##", CultureInfo.InvariantCulture);
        if (weightLabel != null)
            weightLabel.text = $"Ciężar (m·g):  {value * gravity:0.##} N";
    }

    private void OnHeightInputSubmit(string text)
    {
        if (TryParseFloat(text, out float v))
        {
            v = Mathf.Clamp(v, MinHeight, MaxHeight);
            heightSlider.value = v;  // wywola OnHeightChanged i odswiezy pole
        }
        else
        {
            heightInput.text = dropHeight.ToString("0.0", CultureInfo.InvariantCulture);
        }
    }

    private void OnMassInputSubmit(string text)
    {
        if (TryParseFloat(text, out float v))
        {
            v = Mathf.Clamp(v, MinMass, MaxMass);
            massSlider.value = v;
        }
        else
        {
            massInput.text = pebbleMass.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }

    private static bool TryParseFloat(string text, out float value)
    {
        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }

    // pomiary

    private void UpdateMeasurementText()
    {
        double fallen = 0.5 * gravity * fallTime * fallTime;
        if (fallen > dropHeight) fallen = dropHeight;
        double height = dropHeight - fallen;
        double velocity = gravity * fallTime;
        double kineticEnergy = 0.5 * pebbleMass * velocity * velocity;

        string status = state == State.Landed ? "(wyladowal)"
                       : state == State.Falling ? "(spada)" : "(gotowy)";

        measurementText.text =
            $"Czas: {fallTime:0.00} s   {status}\n" +
            $"Wysokość nad podłożem: {height:0.00} m\n" +
            $"Droga spadku: {fallen:0.00} m\n" +
            $"Prędkość: {velocity:0.00} m/s\n" +
            $"Energia kinetyczna (1/2 mv²): {kineticEnergy:0.##} J";
    }

    // ---------- UI ----------

    private void BuildUI()
    {
        EnsureEventSystem();

        GameObject canvasGO = new GameObject("GravityTestCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform panel = NewRect("Panel", canvas.transform,
                                      new Vector2(15, -15), new Vector2(400, 600));
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        AddText(panel, new Vector2(15, -12), new Vector2(370, 30),
                $"Planeta: {planetInfo.polishName}", 18, TextAnchor.UpperLeft);
        AddText(panel, new Vector2(15, -42), new Vector2(370, 24),
                $"g = {gravity:0.##} m/s\u00B2", 16, TextAnchor.UpperLeft);

        // Wysokosc: label + suwak + pole tekstowe + jednostka
        AddText(panel, new Vector2(15, -82), new Vector2(90, 24),
                "Wysokość:", 15, TextAnchor.MiddleLeft);
        heightSlider = AddSlider(panel, new Vector2(110, -82), new Vector2(175, 24),
                                 MinHeight, MaxHeight, dropHeight);
        heightSlider.onValueChanged.AddListener(OnHeightChanged);
        heightInput = AddInputField(panel, new Vector2(290, -82), new Vector2(60, 24),
                                    dropHeight.ToString("0.0", CultureInfo.InvariantCulture), 15);
        heightInput.onEndEdit.AddListener(OnHeightInputSubmit);
        AddText(panel, new Vector2(355, -82), new Vector2(30, 24),
                "m", 15, TextAnchor.MiddleLeft);

        // Masa: label + suwak + pole tekstowe + jednostka
        AddText(panel, new Vector2(15, -112), new Vector2(90, 24),
                "Masa:", 15, TextAnchor.MiddleLeft);
        massSlider = AddSlider(panel, new Vector2(110, -112), new Vector2(175, 24),
                               MinMass, MaxMass, pebbleMass);
        massSlider.onValueChanged.AddListener(OnMassChanged);
        massInput = AddInputField(panel, new Vector2(290, -112), new Vector2(60, 24),
                                  pebbleMass.ToString("0.##", CultureInfo.InvariantCulture), 15);
        massInput.onEndEdit.AddListener(OnMassInputSubmit);
        AddText(panel, new Vector2(355, -112), new Vector2(30, 24),
                "kg", 15, TextAnchor.MiddleLeft);

        weightLabel = AddText(panel, new Vector2(15, -142), new Vector2(370, 24),
                              $"Ciężar (m·g):  {pebbleMass * gravity:0.##} N",
                              15, TextAnchor.UpperLeft);

        AddText(panel, new Vector2(15, -180), new Vector2(370, 20),
                "Pomiary:", 14, TextAnchor.UpperLeft);
        measurementText = AddText(panel, new Vector2(15, -202), new Vector2(370, 140),
                                  "", 16, TextAnchor.UpperLeft);
        
        AddButton(panel, new Vector2(15, -440), new Vector2(180, 44),
                  "Upuść", new Color(0.22f, 0.40f, 0.30f, 1f), out _)
            .onClick.AddListener(StartDrop);
        AddButton(panel, new Vector2(205, -440), new Vector2(180, 44),
                  "Reset", new Color(0.22f, 0.34f, 0.55f, 1f), out _)
            .onClick.AddListener(ResetPebble);
        AddButton(panel, new Vector2(15, -500), new Vector2(370, 44),
                  "Powrót do Układu", new Color(0.35f, 0.35f, 0.40f, 1f), out _)
            .onClick.AddListener(BackToSolarSystem);
    }

    // ---------- helpery UI ----------

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
                         string text, int fontSize, TextAnchor align)
    {
        RectTransform rt = NewRect("Text", parent, pos, size);
        Text t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = align;
        t.text = text;
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

    private Slider AddSlider(Transform parent, Vector2 pos, Vector2 size,
                             float min, float max, float initial)
    {
        RectTransform rt = NewRect("Slider", parent, pos, size);
        GameObject sliderGO = rt.gameObject;
        Slider slider = sliderGO.AddComponent<Slider>();

        GameObject bgGO = new GameObject("Background", typeof(RectTransform));
        RectTransform bgRt = bgGO.GetComponent<RectTransform>();
        bgRt.SetParent(sliderGO.transform, false);
        bgRt.anchorMin = new Vector2(0, 0.3f);
        bgRt.anchorMax = new Vector2(1, 0.7f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bgGO.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);

        GameObject fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
        RectTransform fillAreaRt = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRt.SetParent(sliderGO.transform, false);
        fillAreaRt.anchorMin = new Vector2(0, 0.3f);
        fillAreaRt.anchorMax = new Vector2(1, 0.7f);
        fillAreaRt.offsetMin = new Vector2(5, 0);
        fillAreaRt.offsetMax = new Vector2(-10, 0);

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform));
        RectTransform fillRt = fillGO.GetComponent<RectTransform>();
        fillRt.SetParent(fillAreaGO.transform, false);
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(1, 1);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        fillGO.AddComponent<Image>().color = new Color(0.30f, 0.55f, 0.85f, 1f);
        slider.fillRect = fillRt;

        GameObject handleAreaGO = new GameObject("Handle Slide Area", typeof(RectTransform));
        RectTransform handleAreaRt = handleAreaGO.GetComponent<RectTransform>();
        handleAreaRt.SetParent(sliderGO.transform, false);
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(10, 0);
        handleAreaRt.offsetMax = new Vector2(-10, 0);

        GameObject handleGO = new GameObject("Handle", typeof(RectTransform));
        RectTransform handleRt = handleGO.GetComponent<RectTransform>();
        handleRt.SetParent(handleAreaGO.transform, false);
        handleRt.sizeDelta = new Vector2(18, 0);
        Image handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;

        slider.minValue = min;
        slider.maxValue = max;
        slider.value = initial;
        return slider;
    }

    private InputField AddInputField(Transform parent, Vector2 pos, Vector2 size,
                                     string initialValue, int fontSize)
    {
        RectTransform rt = NewRect("InputField", parent, pos, size);
        GameObject inputGO = rt.gameObject;

        Image bgImg = inputGO.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.20f, 1f);

        InputField field = inputGO.AddComponent<InputField>();
        field.targetGraphic = bgImg;

        // tekst aktualnej zawartosci
        GameObject textGO = new GameObject("Text", typeof(RectTransform));
        RectTransform textRt = textGO.GetComponent<RectTransform>();
        textRt.SetParent(inputGO.transform, false);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(8, 0);
        textRt.offsetMax = new Vector2(-5, 0);
        Text textComp = textGO.AddComponent<Text>();
        textComp.font = font;
        textComp.fontSize = fontSize;
        textComp.color = Color.white;
        textComp.alignment = TextAnchor.MiddleLeft;
        textComp.supportRichText = false;
        field.textComponent = textComp;

        // placeholder wymagany przez InputField
        GameObject phGO = new GameObject("Placeholder", typeof(RectTransform));
        RectTransform phRt = phGO.GetComponent<RectTransform>();
        phRt.SetParent(inputGO.transform, false);
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = new Vector2(8, 0);
        phRt.offsetMax = new Vector2(-5, 0);
        Text phText = phGO.AddComponent<Text>();
        phText.font = font;
        phText.fontSize = fontSize;
        phText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        phText.alignment = TextAnchor.MiddleLeft;
        phText.text = "";
        field.placeholder = phText;

        field.contentType = InputField.ContentType.DecimalNumber;
        field.text = initialValue;
        return field;
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