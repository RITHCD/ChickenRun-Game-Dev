using UnityEngine;

public class ChickenBounce : MonoBehaviour
{
    [Header("Bounce (up and down)")]
    public float bounceHeight = 20f;   // pixels
    public float bounceSpeed = 3f;

    [Header("Tilt (side to side)")]
    public float tiltAngle = 4f;       // degrees
    public float tiltSpeed = 2f;

    [Header("Squash and stretch")]
    public float squashAmount = 0.04f;

    RectTransform rect;
    Vector2 startPos;
    Vector3 startScale;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        startScale = rect.localScale;
    }

    void Update()
    {
        float t = Time.unscaledTime;

        // Hop: abs(sin) makes it bounce off the ground instead of floating
        float hop = Mathf.Abs(Mathf.Sin(t * bounceSpeed));
        rect.anchoredPosition = startPos + new Vector2(0f, hop * bounceHeight);

        // Tilt
        float tilt = Mathf.Sin(t * tiltSpeed) * tiltAngle;
        rect.localRotation = Quaternion.Euler(0f, 0f, tilt);

        // Squash when landing, stretch in the air
        float s = (hop - 0.5f) * squashAmount;
        rect.localScale = new Vector3(startScale.x - s, startScale.y + s, startScale.z);
    }
}