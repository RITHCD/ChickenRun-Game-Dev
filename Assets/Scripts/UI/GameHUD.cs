using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [Header("UI References")]
    public Canvas targetCanvas;
    public GameObject hudRoot;
    public TMP_Text scoreText;
    public TMP_Text eggText;
    public Image eggIcon;
    public Sprite eggSprite;

    [Header("Settings")]
    public bool showOnlyWhilePlaying = true;
    public Color badgeColor = new Color(0.08f, 0.08f, 0.1f, 0.65f);

    private int lastScore = -1;
    private int lastEggs = -1;
    private float eggPunchTimer = 0f;
    private const float PunchDuration = 0.25f;

    // Automatically ensure GameHUD exists in any loaded scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInit()
    {
        if (FindFirstObjectByType<GameHUD>() == null)
        {
            GameObject hudGo = new GameObject("GameHUD");
            hudGo.AddComponent<GameHUD>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitHUD();
    }

    public void InitHUD()
    {
        if (targetCanvas == null)
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
        }

        if (targetCanvas == null)
        {
            // If no canvas exists yet, create one
            GameObject cGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = cGo.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler cs = cGo.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
        }

        // Try to load egg sprite if unassigned
        if (eggSprite == null)
        {
            eggSprite = Resources.Load<Sprite>("egg score");
            if (eggSprite == null)
            {
                eggSprite = Resources.Load<Sprite>("egg_icon");
            }
        }

        // Check if HUD already built under this canvas
        Transform existing = targetCanvas.transform.Find("HUD_Panel");
        if (existing != null)
        {
            hudRoot = existing.gameObject;
            BindExistingElements(existing);
        }
        else
        {
            BuildHUD(targetCanvas.transform);
        }

        // Apply initial values
        UpdateValues(true);
    }

    void BindExistingElements(Transform root)
    {
        foreach (TMP_Text t in root.GetComponentsInChildren<TMP_Text>(true))
        {
            string n = t.name.ToLower();
            if (n.Contains("score")) scoreText = t;
            else if (n.Contains("egg")) eggText = t;
        }

        foreach (Image img in root.GetComponentsInChildren<Image>(true))
        {
            if (img.name.ToLower().Contains("icon")) eggIcon = img;
        }
    }

    void BuildHUD(Transform parent)
    {
        Sprite pillSprite = CreatePillSprite(64, 64, 28);

        // 1. Root container (fills screen)
        hudRoot = new GameObject("HUD_Panel", typeof(RectTransform));
        hudRoot.transform.SetParent(parent, false);
        RectTransform rt = hudRoot.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 2. Egg Badge (Top-Left)
        GameObject eggBadge = new GameObject("EggBadge", typeof(RectTransform), typeof(Image));
        eggBadge.transform.SetParent(hudRoot.transform, false);
        Image eggBadgeImg = eggBadge.GetComponent<Image>();
        eggBadgeImg.sprite = pillSprite;
        eggBadgeImg.type = Image.Type.Sliced;
        eggBadgeImg.color = badgeColor;

        RectTransform eggBadgeRt = eggBadge.GetComponent<RectTransform>();
        eggBadgeRt.anchorMin = new Vector2(0f, 1f);
        eggBadgeRt.anchorMax = new Vector2(0f, 1f);
        eggBadgeRt.pivot = new Vector2(0f, 1f);
        eggBadgeRt.anchoredPosition = new Vector2(35f, -35f);
        eggBadgeRt.sizeDelta = new Vector2(170f, 60f);

        // Egg Icon
        GameObject eggIconGo = new GameObject("EggIcon", typeof(RectTransform), typeof(Image));
        eggIconGo.transform.SetParent(eggBadge.transform, false);
        eggIcon = eggIconGo.GetComponent<Image>();
        if (eggSprite != null)
        {
            eggIcon.sprite = eggSprite;
        }
        else
        {
            eggIcon.sprite = CreateProceduralEggSprite();
        }
        eggIcon.preserveAspect = true;

        RectTransform eggIconRt = eggIconGo.GetComponent<RectTransform>();
        eggIconRt.anchorMin = new Vector2(0f, 0.5f);
        eggIconRt.anchorMax = new Vector2(0f, 0.5f);
        eggIconRt.pivot = new Vector2(0f, 0.5f);
        eggIconRt.anchoredPosition = new Vector2(12f, 0f);
        eggIconRt.sizeDelta = new Vector2(44f, 44f);

        // Egg Text
        GameObject eggTextGo = new GameObject("EggText", typeof(RectTransform), typeof(TextMeshProUGUI));
        eggTextGo.transform.SetParent(eggBadge.transform, false);
        eggText = eggTextGo.GetComponent<TextMeshProUGUI>();
        eggText.text = "0";
        eggText.fontSize = 32;
        eggText.fontStyle = FontStyles.Bold;
        eggText.color = Color.white;
        eggText.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform eggTextRt = eggTextGo.GetComponent<RectTransform>();
        eggTextRt.anchorMin = new Vector2(0f, 0f);
        eggTextRt.anchorMax = new Vector2(1f, 1f);
        eggTextRt.pivot = new Vector2(0f, 0.5f);
        eggTextRt.offsetMin = new Vector2(62f, 0f);
        eggTextRt.offsetMax = new Vector2(-10f, 0f);

        // 3. Score Badge (Top-Right)
        GameObject scoreBadge = new GameObject("ScoreBadge", typeof(RectTransform), typeof(Image));
        scoreBadge.transform.SetParent(hudRoot.transform, false);
        Image scoreBadgeImg = scoreBadge.GetComponent<Image>();
        scoreBadgeImg.sprite = pillSprite;
        scoreBadgeImg.type = Image.Type.Sliced;
        scoreBadgeImg.color = badgeColor;

        RectTransform scoreBadgeRt = scoreBadge.GetComponent<RectTransform>();
        scoreBadgeRt.anchorMin = new Vector2(1f, 1f);
        scoreBadgeRt.anchorMax = new Vector2(1f, 1f);
        scoreBadgeRt.pivot = new Vector2(1f, 1f);
        scoreBadgeRt.anchoredPosition = new Vector2(-35f, -35f);
        scoreBadgeRt.sizeDelta = new Vector2(230f, 60f);

        // Score Text
        GameObject scoreTextGo = new GameObject("ScoreText", typeof(RectTransform), typeof(TextMeshProUGUI));
        scoreTextGo.transform.SetParent(scoreBadge.transform, false);
        scoreText = scoreTextGo.GetComponent<TextMeshProUGUI>();
        scoreText.text = "<color=#FFD54F><size=19>SCORE </size></color><b>0</b>";
        scoreText.fontSize = 28;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.color = Color.white;
        scoreText.alignment = TextAlignmentOptions.MidlineRight;

        RectTransform scoreTextRt = scoreTextGo.GetComponent<RectTransform>();
        scoreTextRt.anchorMin = Vector2.zero;
        scoreTextRt.anchorMax = Vector2.one;
        scoreTextRt.pivot = new Vector2(1f, 0.5f);
        scoreTextRt.offsetMin = new Vector2(16f, 0f);
        scoreTextRt.offsetMax = new Vector2(-16f, 0f);
    }

    void Update()
    {
        UpdateVisibility();
        UpdateValues();
        AnimateEggPunch();
    }

    void UpdateVisibility()
    {
        if (hudRoot == null) return;

        if (!showOnlyWhilePlaying)
        {
            if (!hudRoot.activeSelf) hudRoot.SetActive(true);
            return;
        }

        bool shouldBeVisible = false;

        if (GameManager.I != null)
        {
            // Visible when playing or paused, hidden on initial start menu
            shouldBeVisible = (GameManager.I.State == GameState.Playing || GameManager.I.State == GameState.Paused);
        }
        else
        {
            PlayerControl pc = FindFirstObjectByType<PlayerControl>();
            if (pc != null)
            {
                shouldBeVisible = pc.isGameStarted && !pc.isGameOver;
            }
            else
            {
                shouldBeVisible = true;
            }
        }

        if (hudRoot.activeSelf != shouldBeVisible)
        {
            hudRoot.SetActive(shouldBeVisible);
        }
    }

    void UpdateValues(bool force = false)
    {
        int score = 0;
        int eggs = 0;

        if (GameManager.I != null)
        {
            score = GameManager.I.Score;
            eggs = GameManager.I.Eggs;
        }
        else
        {
            PlayerControl pc = FindFirstObjectByType<PlayerControl>();
            if (pc != null)
            {
                eggs = pc.score;
                score = Mathf.FloorToInt(pc.transform.position.z / 10f) + pc.score * 10;
            }
        }

        if (eggs != lastEggs || force)
        {
            if (!force && eggs > lastEggs && lastEggs >= 0)
            {
                eggPunchTimer = PunchDuration;
            }
            lastEggs = eggs;
            if (eggText != null)
            {
                eggText.text = eggs.ToString();
            }
        }

        if (score != lastScore || force)
        {
            lastScore = score;
            if (scoreText != null)
            {
                scoreText.text = $"<color=#FFD54F><size=19>SCORE </size></color><b>{score:N0}</b>";
            }
        }
    }

    void AnimateEggPunch()
    {
        if (eggIcon == null) return;

        if (eggPunchTimer > 0f)
        {
            eggPunchTimer -= Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(eggPunchTimer / PunchDuration);
            float scale = Mathf.Lerp(1f, 1.35f, Mathf.Sin(progress * Mathf.PI));
            eggIcon.transform.localScale = Vector3.one * scale;
        }
        else
        {
            eggIcon.transform.localScale = Vector3.one;
        }
    }

    // Creates an anti-aliased 9-sliced pill sprite for sleek badge background
    Sprite CreatePillSprite(int width, int height, int radius)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] cols = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int cx = x < radius ? radius : (x >= width - radius ? width - radius - 1 : x);
                int cy = y < radius ? radius : (y >= height - radius ? height - radius - 1 : y);
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                cols[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    // Fallback golden egg sprite if texture file isn't found
    Sprite CreateProceduralEggSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] cols = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.45f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - center.x) / (size * 0.35f);
                float ny = (y - center.y) / (size * 0.44f);
                // Egg taper: narrower at top
                if (ny > 0) nx *= (1f + ny * 0.35f);
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.Clamp01((1f - dist) * size * 0.25f);

                // Golden egg gradient
                float grad = Mathf.Clamp01(1f - (dist * 0.6f));
                Color eggCol = Color.Lerp(new Color(0.95f, 0.75f, 0.2f), new Color(1f, 0.95f, 0.7f), grad);
                cols[y * size + x] = new Color(eggCol.r, eggCol.g, eggCol.b, alpha);
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
