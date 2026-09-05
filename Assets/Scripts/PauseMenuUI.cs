using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


// Menu pauzy wywolywane klawiszem Esc.

public class PauseMenuUI : MonoBehaviour
{
    private Font font;
    private SolarSystemManager manager;

    private GameObject pausePanel;
    private GameObject optionsPanel;

    private Button modeRealisticBtn, modeLowPolyBtn;
    private Button orbitsYesBtn, orbitsNoBtn;

    private bool wasPausedBefore;

    private static readonly Color BtnNormal   = new Color(0.22f, 0.34f, 0.55f, 1f);
    private static readonly Color BtnSelected = new Color(0.30f, 0.55f, 0.35f, 1f);
    private static readonly Color BtnDanger   = new Color(0.55f, 0.25f, 0.25f, 1f);

    void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        manager = FindFirstObjectByType<SolarSystemManager>();
        BuildUI();
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)      BackToPause();
            else if (pausePanel.activeSelf)   Resume();
            else                              PauseGame();
        }
    }

    // Akcje 

    private void PauseGame()
    {
        wasPausedBefore = manager != null && manager.isPaused;
        if (manager != null) manager.isPaused = true;
        pausePanel.SetActive(true);
    }

    private void Resume()
    {
        if (manager != null) manager.isPaused = wasPausedBefore;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    private void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        RefreshOptionsButtons();
    }

    private void BackToPause()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    private void GoToMainMenu()
    {
        GameSettings.Save();
        if (manager != null) manager.isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    // Budowa UI 

    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("PauseMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // nad innymi panelami
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        BuildPausePanel(canvas.transform);
        BuildOptionsPanel(canvas.transform);
    }

    private void BuildPausePanel(Transform canvas)
    {
        pausePanel = NewCentered("PausePanel", canvas, Vector2.zero, new Vector2(460, 360)).gameObject;
        pausePanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        Transform p = pausePanel.transform;

        AddText(p, new Vector2(0, 130), new Vector2(440, 50),
                "Pauza", 30, TextAnchor.MiddleCenter);

        AddButton(p, new Vector2(0,  50), new Vector2(360, 52), "Wznów",      BtnSelected, 22, out _)
            .onClick.AddListener(Resume);
        AddButton(p, new Vector2(0, -10), new Vector2(360, 52), "Opcje",      BtnNormal,   22, out _)
            .onClick.AddListener(OpenOptions);
        AddButton(p, new Vector2(0, -70), new Vector2(360, 52), "Menu główne", BtnDanger,  22, out _)
            .onClick.AddListener(GoToMainMenu);
    }

    private void BuildOptionsPanel(Transform canvas)
    {
        optionsPanel = NewCentered("OptionsPanel", canvas, Vector2.zero, new Vector2(700, 560)).gameObject;
        optionsPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        Transform p = optionsPanel.transform;

        AddText(p, new Vector2(0, 220), new Vector2(680, 60),
                "Opcje", 30, TextAnchor.MiddleCenter);

        AddText(p, new Vector2(-180, 130), new Vector2(220, 40),
                "Tryb wyświetlania:", 20, TextAnchor.MiddleRight);
        modeRealisticBtn = AddButton(p, new Vector2( 15, 130), new Vector2(150, 44),
                                     "Realistyczny", BtnNormal, 18, out _);
        modeRealisticBtn.onClick.AddListener(() => SetMode(false));
        modeLowPolyBtn   = AddButton(p, new Vector2(175, 130), new Vector2(150, 44),
                                     "Low-poly", BtnNormal, 18, out _);
        modeLowPolyBtn.onClick.AddListener(() => SetMode(true));

        AddText(p, new Vector2(-180, 60), new Vector2(220, 40),
                "Pokazuj orbity:", 20, TextAnchor.MiddleRight);
        orbitsYesBtn = AddButton(p, new Vector2( 15, 60), new Vector2(150, 44),
                                 "Tak", BtnNormal, 18, out _);
        orbitsYesBtn.onClick.AddListener(() => SetOrbits(true));
        orbitsNoBtn  = AddButton(p, new Vector2(175, 60), new Vector2(150, 44),
                                 "Nie", BtnNormal, 18, out _);
        orbitsNoBtn.onClick.AddListener(() => SetOrbits(false));


        AddButton(p, new Vector2(0, -210), new Vector2(220, 52),
                  "Wstecz", BtnNormal, 22, out _).onClick.AddListener(BackToPause);
    }

    // Ustawienia

    private void SetMode(bool lowPoly)
    {
        GameSettings.startLowPoly = lowPoly;
        GameSettings.Save();
        if (manager != null) manager.SetVisualMode(lowPoly);
        RefreshOptionsButtons();
    }
    private void SetOrbits(bool show)
    {
        GameSettings.showOrbits = show;
        GameSettings.Save();
        if (manager != null) manager.SetOrbitsVisible(show);
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