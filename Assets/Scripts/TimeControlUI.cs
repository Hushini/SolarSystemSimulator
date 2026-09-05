using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
    Panel sterowania czasem.
    Data symulacji, pauza, powrót do teraz, suwak prędkości oraz pole wpisania konkretnej daty.
*/
public class TimeControlUI : MonoBehaviour
{
    private SolarSystemManager solarSystem;
    private Font font;

    private Text dateText;
    private Text pauseLabel;
    private Text speedLabel;
    private Text statusText;
    private Slider speedSlider;
    private InputField dateField;

    void Start()
    {
        solarSystem = FindFirstObjectByType<SolarSystemManager>();
        if (solarSystem == null)
        {
            Debug.LogError("TimeControlUI: brak SolarSystemManager w scenie.");
            enabled = false;
            return;
        }

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
    }

    void Update()
    {
        dateText.text = solarSystem.SimulationTime.ToString("yyyy-MM-dd  HH:mm 'UTC'");
        pauseLabel.text = solarSystem.isPaused ? "Wznow" : "Pauza";
        speedLabel.text = "Tempo: " + SpeedToText(solarSystem.timeMultiplier);
    }

    private void BuildUI()
    {
        EnsureEventSystem();

        // Canvas 
        GameObject canvasGO = new GameObject("TimeControlCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel 
        RectTransform panel = NewRect("Panel", canvas.transform,
                                      new Vector2(15, -15), new Vector2(320, 252));
        Image panelBg = panel.gameObject.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.55f);

        // Data symulacji 
        dateText = AddText(panel, new Vector2(15, -12), new Vector2(290, 34),
                           24, TextAnchor.MiddleLeft);

        // Przyciski pauzy i teraz
        Button pauseBtn = AddButton(panel, new Vector2(15, -56),
                                    new Vector2(140, 42), "Pauza", out pauseLabel);
        pauseBtn.onClick.AddListener(() => solarSystem.isPaused = !solarSystem.isPaused);

        Button nowBtn = AddButton(panel, new Vector2(165, -56),
                                  new Vector2(140, 42), "Teraz", out _);
        nowBtn.onClick.AddListener(() => solarSystem.JumpToNow());

        // Suwak prędkości 
        speedSlider = AddSlider(panel, new Vector2(15, -112), new Vector2(290, 22));
        speedSlider.minValue = -1f;
        speedSlider.maxValue = 1f;
        speedSlider.value = 0f;
        speedSlider.onValueChanged.AddListener(OnSpeedChanged);

        // Etykieta prędkości
        speedLabel = AddText(panel, new Vector2(15, -140), new Vector2(290, 24),
                             18, TextAnchor.MiddleLeft);

        // Pole daty + przycisk Ustaw
        dateField = AddInputField(panel, new Vector2(15, -176),
                                  new Vector2(215, 40), "np. 2030-06-15");
        Button applyBtn = AddButton(panel, new Vector2(240, -176),
                                    new Vector2(65, 40), "Ustaw", out _);
        applyBtn.onClick.AddListener(OnApplyDate);

        // Pasek statusu 
        statusText = AddText(panel, new Vector2(15, -222), new Vector2(290, 22),
                             15, TextAnchor.MiddleLeft);
        statusText.text = "Wpisz date i nacisnij Ustaw.";

        OnSpeedChanged(speedSlider.value);   // tempo początkowe
    }

    // Logika 

    private void OnSpeedChanged(float sliderValue)
    {
        solarSystem.timeMultiplier = SliderToMultiplier(sliderValue);
    }

    // Środek suwaka = czas rzeczywisty, końce = do 10^8*. Znak = kierunek.
    private double SliderToMultiplier(float s)
    {
        return Mathf.Sign(s) * Math.Pow(10.0, Mathf.Abs(s) * 8.0);
    }

    private string SpeedToText(double multiplier)
    {
        double abs = Math.Abs(multiplier);
        string dir = multiplier < 0 ? "wstecz " : "";

        if (abs < 60.0)
            return "czas rzeczywisty";

        double daysPerSec = abs / 86400.0;
        if (daysPerSec < 1.0)
            return $"{dir}{abs / 3600.0:0.#} godz/s";
        if (daysPerSec < 400.0)
            return $"{dir}{daysPerSec:0.#} dni/s";
        return $"{dir}{daysPerSec / 365.25:0.#} lat/s";
    }

    // Odczytuje datę z pola i ustawia ją jako czas symulacji.
    private void OnApplyDate()
    {
        Image fieldBg = dateField.GetComponent<Image>();

        if (TryParseDate(dateField.text, out DateTime parsed))
        {
            solarSystem.SetTime(parsed);
            fieldBg.color = Color.white;
            statusText.text = (parsed.Year < 1800 || parsed.Year > 2050)
                ? "Ustawiono (poza zakresem 1800-2050, pozycje przyblizone)."
                : "Ustawiono date.";
        }
        else
        {
            fieldBg.color = new Color(1f, 0.8f, 0.8f, 1f);
            statusText.text = "Niepoprawny format. Uzyj RRRR-MM-DD.";
        }
    }

    private bool TryParseDate(string input, out DateTime result)
    {
        if (DateTime.TryParse(input, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out result))
            return true;
        return DateTime.TryParse(input, CultureInfo.CurrentCulture,
                DateTimeStyles.None, out result);
    }

    // Funkcje pomocnicze do budowy UI

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

    // Tworzy obiekt UI rozciągnięty na całego rodzica, z marginesem.
    private RectTransform StretchRect(string name, Transform parent, float padding)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
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
        return t;
    }

    private Button AddButton(Transform parent, Vector2 pos, Vector2 size,
                             string label, out Text labelText)
    {
        RectTransform rt = NewRect("Button", parent, pos, size);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0.22f, 0.34f, 0.55f, 1f);
        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        RectTransform textRt = StretchRect("Label", rt, 0f);
        labelText = textRt.gameObject.AddComponent<Text>();
        labelText.font = font;
        labelText.fontSize = 20;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.text = label;

        return btn;
    }

    private Slider AddSlider(Transform parent, Vector2 pos, Vector2 size)
    {
        RectTransform rt = NewRect("Slider", parent, pos, size);
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.18f, 1f);

        Slider slider = rt.gameObject.AddComponent<Slider>();

        GameObject areaGO = new GameObject("Handle Slide Area", typeof(RectTransform));
        RectTransform area = areaGO.GetComponent<RectTransform>();
        area.SetParent(rt, false);
        area.anchorMin = Vector2.zero;
        area.anchorMax = Vector2.one;
        area.offsetMin = new Vector2(10f, 0f);
        area.offsetMax = new Vector2(-10f, 0f);

        GameObject handleGO = new GameObject("Handle", typeof(RectTransform));
        RectTransform handle = handleGO.GetComponent<RectTransform>();
        handle.SetParent(area, false);
        handle.sizeDelta = new Vector2(20f, 0f);
        handle.anchoredPosition = Vector2.zero;
        Image handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.fillRect = null;
        slider.handleRect = handle;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private InputField AddInputField(Transform parent, Vector2 pos, Vector2 size,
                                     string placeholder)
    {
        RectTransform rt = NewRect("InputField", parent, pos, size);
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        InputField field = rt.gameObject.AddComponent<InputField>();

        // tekst wpisywany
        RectTransform textRt = StretchRect("Text", rt, 7f);
        Text text = textRt.gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 18;
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleLeft;
        text.supportRichText = false;

        // tekst-podpowiedź
        RectTransform phRt = StretchRect("Placeholder", rt, 7f);
        Text ph = phRt.gameObject.AddComponent<Text>();
        ph.font = font;
        ph.fontSize = 18;
        ph.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        ph.fontStyle = FontStyle.Italic;
        ph.alignment = TextAnchor.MiddleLeft;
        ph.text = placeholder;

        field.textComponent = text;
        field.placeholder = ph;
        field.targetGraphic = bg;

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