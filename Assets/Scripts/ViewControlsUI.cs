using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
    Przyciski w lewym dolnym rogu: przelacznik orbit i trybu low-poly.
    Synchronizuja sie ze stanem SolarSystemManager (jak obsługa klawiszami, to O i L).
*/
public class ViewControlsUI : MonoBehaviour
{
    private SolarSystemManager manager;
    private Font font;

    private Button orbitsBtn;
    private Text orbitsLabel;
    private Button lowPolyBtn;
    private Text lowPolyLabel;

    void Start()
    {
        manager = FindFirstObjectByType<SolarSystemManager>();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
        RefreshLabels();
    }

    void Update()
    {
        // synchronizacja gdy uzytkownik nacisnie klawisz O lub L
        RefreshLabels();
    }

    private void BuildUI()
    {
        EnsureEventSystem();

        GameObject canvasGO = new GameObject("ViewControlsCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // panel w lewym dolnym rogu
        GameObject panelGO = new GameObject("Panel", typeof(RectTransform));
        RectTransform panelRt = panelGO.GetComponent<RectTransform>();
        panelRt.SetParent(canvas.transform, false);
        panelRt.anchorMin = new Vector2(0, 0);
        panelRt.anchorMax = new Vector2(0, 0);
        panelRt.pivot = new Vector2(0, 0);
        panelRt.anchoredPosition = new Vector2(15, 15);
        panelRt.sizeDelta = new Vector2(220, 110);
        panelGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        orbitsBtn = AddButton(panelRt, new Vector2(10, 10), new Vector2(200, 40),
                              new Color(0.22f, 0.34f, 0.55f, 1f), out orbitsLabel);
        orbitsBtn.onClick.AddListener(ToggleOrbits);

        lowPolyBtn = AddButton(panelRt, new Vector2(10, 60), new Vector2(200, 40),
                               new Color(0.22f, 0.34f, 0.55f, 1f), out lowPolyLabel);
        lowPolyBtn.onClick.AddListener(ToggleLowPoly);
    }

    private void ToggleOrbits()
    {
        if (manager == null) return;
        manager.SetOrbitsVisible(!manager.showOrbits);
    }

    private void ToggleLowPoly()
    {
        if (manager == null) return;
        manager.SetVisualMode(!manager.useLowPoly);
    }

    private void RefreshLabels()
    {
        if (manager == null) return;
        orbitsLabel.text = manager.showOrbits ? "Orbity: WL" : "Orbity: WYL";
        lowPolyLabel.text = manager.useLowPoly ? "Tryb: low-poly" : "Tryb: realistyczny";
    }

    // Helpery UI

    private Button AddButton(Transform parent, Vector2 pos, Vector2 size,
                             Color color, out Text labelText)
    {
        GameObject go = new GameObject("Button", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = color;
        Button btn = go.AddComponent<Button>();
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
        labelText.fontSize = 18;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;
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