using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


// Ekran startowy z przyciskami oraz panelem opcji
public class MainMenuUI : MonoBehaviour
{
    private Font font;
    private GameObject mainPanel;
    private GameObject optionsPanel;

    private Button modeRealisticBtn, modeLowPolyBtn;
    private Button orbitsYesBtn, orbitsNoBtn;

    private static readonly Color BtnNormal   = new Color(0.22f, 0.34f, 0.55f, 1f);
    private static readonly Color BtnSelected = new Color(0.30f, 0.55f, 0.35f, 1f);
    private static readonly Color BtnDanger   = new Color(0.55f, 0.25f, 0.25f, 1f);

    void Start()
    {
        SkyboxBuilder.Apply();
        
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameSettings.Load();
        EnsureCameraAndEventSystem();
        BuildUI();
        ShowMain();
    }

    private void EnsureCameraAndEventSystem()
    {
        if (Camera.main == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            Camera cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
        }
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        BuildMainPanel(canvas.transform);
        BuildOptionsPanel(canvas.transform);
    }

    // Panel glowny

    private void BuildMainPanel(Transform canvas)
    {
        mainPanel = NewCentered("MainPanel", canvas, Vector2.zero, new Vector2(560, 480)).gameObject;
        mainPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        Transform p = mainPanel.transform;

        AddText(p, new Vector2(0, 170), new Vector2(540, 60),
                "Symulator Układu Słonecznego", 30, TextAnchor.MiddleCenter);
        AddText(p, new Vector2(0, 115), new Vector2(540, 30),
                "praca inżynierska", 16, TextAnchor.MiddleCenter);

        AddButton(p, new Vector2(0,   30), new Vector2(320, 56), "Start",  BtnSelected, 22, out _)
            .onClick.AddListener(StartSimulation);
        AddButton(p, new Vector2(0,  -40), new Vector2(320, 56), "Opcje",  BtnNormal,   22, out _)
            .onClick.AddListener(ShowOptions);
        AddButton(p, new Vector2(0, -110), new Vector2(320, 56), "Wyjdź",  BtnDanger,   22, out _)
            .onClick.AddListener(QuitApp);
    }

    private void StartSimulation()
    {
        GameSettings.Save();
        SceneManager.LoadScene("SolarSystem");
    }

    private void QuitApp()
    {
        GameSettings.Save();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Panel opcji 

    private void BuildOptionsPanel(Transform canvas)
    {
        optionsPanel = NewCentered("OptionsPanel", canvas, Vector2.zero, new Vector2(700, 560)).gameObject;
        optionsPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        Transform p = optionsPanel.transform;

        AddText(p, new Vector2(0, 220), new Vector2(680, 60),
                "Opcje", 30, TextAnchor.MiddleCenter);

        // Tryb startowy
        AddText(p, new Vector2(-180, 130), new Vector2(220, 40),
                "Tryb startowy:", 20, TextAnchor.MiddleRight);
        modeRealisticBtn = AddButton(p, new Vector2( 15, 130), new Vector2(150, 44),
                                     "Realistyczny", BtnNormal, 18, out _);
        modeRealisticBtn.onClick.AddListener(() => SetMode(false));
        modeLowPolyBtn   = AddButton(p, new Vector2(175, 130), new Vector2(150, 44),
                                     "Low-poly", BtnNormal, 18, out _);
        modeLowPolyBtn.onClick.AddListener(() => SetMode(true));

        // Orbity
        AddText(p, new Vector2(-180, 60), new Vector2(220, 40),
                "Pokazuj orbity:", 20, TextAnchor.MiddleRight);
        orbitsYesBtn = AddButton(p, new Vector2( 15, 60), new Vector2(150, 44),
                                 "Tak", BtnNormal, 18, out _);
        orbitsYesBtn.onClick.AddListener(() => SetOrbits(true));
        orbitsNoBtn  = AddButton(p, new Vector2(175, 60), new Vector2(150, 44),
                                 "Nie", BtnNormal, 18, out _);
        orbitsNoBtn.onClick.AddListener(() => SetOrbits(false));

        AddButton(p, new Vector2(0, -210), new Vector2(220, 52),
                  "Wstecz", BtnNormal, 22, out _).onClick.AddListener(ShowMain);
    }

    private void SetMode(bool lowPoly)
    {
        GameSettings.startLowPoly = lowPoly;
        GameSettings.Save();
        RefreshOptionsButtons();
    }
    private void SetOrbits(bool show)
    {
        GameSettings.showOrbits = show;
        GameSettings.Save();
        RefreshOptionsButtons();
    }

    private void RefreshOptionsButtons()
    {
        SetBtnColor(modeRealisticBtn, !GameSettings.startLowPoly);
        SetBtnColor(modeLowPolyBtn,    GameSettings.startLowPoly);
        SetBtnColor(orbitsYesBtn,      GameSettings.showOrbits);
        SetBtnColor(orbitsNoBtn,      !GameSettings.showOrbits);
    }

    private void SetBtnColor(Button btn, bool selected)
        => btn.GetComponent<Image>().color = selected ? BtnSelected : BtnNormal;

    private void ShowMain()
    {
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }
    private void ShowOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
        RefreshOptionsButtons();
    }

    // Helpery UI 

    private RectTransform NewCentered(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    private Text AddText(Transform parent, Vector2 pos, Vector2 size,
                         string text, int fontSize, TextAnchor align)
    {
        RectTransform rt = NewCentered("Text", parent, pos, size);
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
                             string label, Color color, int fontSize, out Text labelText)
    {
        RectTransform rt = NewCentered("Button", parent, pos, size);
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
        labelText.fontSize = fontSize;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.text = label;
        return btn;
    }
}