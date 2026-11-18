using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject panel;    // optional: drag your own Game Over panel here

    void OnEnable() { GameManager.OnGameOver += Show; }
    void OnDisable() { GameManager.OnGameOver -= Show; }

    void Show()
    {
        GameManager g = GameManager.I;
        if (g == null) return;

        if (panel == null) panel = FindExistingPanel();
        if (panel != null)
        {
            panel.SetActive(true);
            FillTexts(panel.transform, g);
            return;
        }
        BuildPanel(g);
    }

    GameObject FindExistingPanel()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return null;
        foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name.ToLower().Replace("_", "").Replace(" ", "");
            if (n.Contains("gameover") && t.GetComponent<RectTransform>() != null) return t.gameObject;
        }
        return null;
    }

    void FillTexts(Transform root, GameManager g)
    {
        foreach (TMP_Text t in root.GetComponentsInChildren<TMP_Text>(true))
        {
            string n = t.name.ToLower();
            if (n.Contains("score")) t.text = "Score " + g.Score;
            else if (n.Contains("egg")) t.text = "Eggs " + g.Eggs;
            else if (n.Contains("best")) t.text = "Best " + g.Best;
        }
    }

    void BuildPanel(GameManager g)
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogWarning("GameOverUI: no Canvas in the scene."); return; }

        GameObject p = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
        p.transform.SetParent(canvas.transform, false);
        RectTransform rt = p.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        p.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        MakeText(p.transform, "Game Over", 90, new Vector2(0f, 260f));
        MakeText(p.transform, "Score " + g.Score, 60, new Vector2(0f, 100f));
        MakeText(p.transform, "Eggs " + g.Eggs, 60, new Vector2(0f, 10f));
        MakeText(p.transform, "Best " + g.Best, 60, new Vector2(0f, -80f));
        MakeButton(p.transform, "Restart", new Vector2(0f, -240f), g.Restart);

        p.transform.SetAsLastSibling();
        panel = p;
    }

    void MakeText(Transform parent, string content, float size, Vector2 pos)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(900f, 120f);
        r.anchoredPosition = pos;
    }

    void MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(1f, 0.78f, 0.2f);
        go.GetComponent<Button>().onClick.AddListener(onClick);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(420f, 130f);
        r.anchoredPosition = pos;

        GameObject txt = new GameObject("Label", typeof(RectTransform));
        txt.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 60;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        RectTransform tr = txt.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;
    }
}