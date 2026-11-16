using UnityEngine;

public class TitleDropIn : MonoBehaviour
{
    [Header("Drop")]
    public float startOffset = 800f;
    public float duration = 0.9f;
    public float delay = 0.2f;

    [Header("Bounce on landing")]
    public float bounceStrength = 1.2f;

    RectTransform rect;
    Vector2 finalPos;
    float timer;
    bool landed;
    bool initialized;

    void Init()
    {
        if (initialized) return;
        rect = GetComponent<RectTransform>();
        finalPos = rect.anchoredPosition;
        initialized = true;
    }

    void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        Init();
        timer = -delay;
        landed = false;
        rect.anchoredPosition = finalPos + new Vector2(0f, startOffset);
    }

    void Update()
    {
        if (landed) return;

        timer += Time.unscaledDeltaTime;
        if (timer < 0f) return;

        float t = Mathf.Clamp01(timer / duration);
        float eased = EaseOutBack(t);
        rect.anchoredPosition = finalPos + new Vector2(0f, startOffset * (1f - eased));

        if (t >= 1f)
        {
            rect.anchoredPosition = finalPos;
            landed = true;
        }
    }

    float EaseOutBack(float x)
    {
        float c1 = bounceStrength;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}