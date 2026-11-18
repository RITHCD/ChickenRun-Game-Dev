using UnityEngine;

public class FoxManager : MonoBehaviour
{
    public GameObject fox;             // optional, leave empty until the fox is ready
    public float foxAwayTime = 10f;    // safe seconds before the fox goes away
    public float hitCooldown = 1.2f;   // ignore extra hits right after a hit

    public int Hits { get; private set; }

    float foxTimer, nextHitTime;

    void Start() { Show(false); }

    void Update()
    {
        if (GameManager.I == null || !GameManager.I.IsPlaying || Hits == 0) return;
        foxTimer += Time.deltaTime;
        if (foxTimer > foxAwayTime) { Hits = 0; Show(false); }
    }

    public void ResetFox()
    {
        Hits = 0; foxTimer = 0f; nextHitTime = 0f;
        Show(false);
    }

    public void RegisterHit()
    {
        if (GameManager.I == null || !GameManager.I.IsPlaying) return;
        if (Time.time < nextHitTime) return;
        nextHitTime = Time.time + hitCooldown;
        Hits++; foxTimer = 0f;
        if (Hits >= 2) GameManager.I.GameOver();
        else Show(true);
    }

    void Show(bool on) { if (fox != null) fox.SetActive(on); }
}