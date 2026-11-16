using UnityEngine;
using UnityEngine.EventSystems;

public class PlayButtonPulse : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Pulse")]
    public float pulseAmount = 0.06f;   // 0.06 = grows 6%
    public float pulseSpeed = 3f;

    [Header("Wiggle")]
    public float wiggleAngle = 2f;      // degrees, set 0 to turn off
    public float wiggleSpeed = 2f;

    [Header("Press effect")]
    public float pressedScale = 0.9f;
    public float pressSmooth = 15f;

    RectTransform rect;
    Vector3 startScale;
    bool pressed;
    float pressFactor = 1f;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startScale = rect.localScale;
    }

    void Update()
    {
        float t = Time.unscaledTime;

        // Smoothly shrink while pressed, return when released
        float target = pressed ? pressedScale : 1f;
        pressFactor = Mathf.Lerp(pressFactor, target, Time.unscaledDeltaTime * pressSmooth);

        // Pulse
        float pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
        rect.localScale = startScale * pulse * pressFactor;

        // Small wiggle
        float wiggle = Mathf.Sin(t * wiggleSpeed) * wiggleAngle;
        rect.localRotation = Quaternion.Euler(0f, 0f, wiggle);
    }

    public void OnPointerDown(PointerEventData eventData) { pressed = true; }
    public void OnPointerUp(PointerEventData eventData) { pressed = false; }
}