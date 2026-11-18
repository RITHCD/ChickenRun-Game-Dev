using System.Collections;
using UnityEngine;

public class FoxManager : MonoBehaviour
{
    public GameObject fox;             // optional, not needed
    public float foxAwayTime = 10f;    // safe seconds before the fox goes away
    public float hitCooldown = 1.2f;   // ignore extra hits right after a hit
    public float lungeTime = 0.7f;     // let the fox catch the chicken before game over

    public int Hits { get; private set; }

    float foxTimer, nextHitTime;

    void Awake()
    {
        // create the chasing fox automatically if the scene has none
        if (FoxChase.I == null)
        {
            GameObject go = new GameObject("FoxChase");
            FoxChase fc = go.AddComponent<FoxChase>();
            if (fc.player == null)
            {
                PlayerControl pc = FindFirstObjectByType<PlayerControl>();
                if (pc != null) fc.player = pc.transform;
            }
        }

        // keep FoxChase in sync with this manager (one set of values, not two)
        if (FoxChase.I != null)
        {
            FoxChase.I.stopTimeOnCatch = false;   // GameManager handles game over
            FoxChase.I.forgiveTime = foxAwayTime;
            FoxChase.I.hitCooldown = hitCooldown;
        }
    }

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
        if (Hits >= 2) return;                      // already caught, waiting for game over
        if (Time.time < nextHitTime) return;
        nextHitTime = Time.time + hitCooldown;
        Hits++; foxTimer = 0f;

        if (FoxChase.I != null) FoxChase.I.Hit();   // fox runs up behind the chicken

        if (Hits >= 2) StartCoroutine(GameOverAfterLunge());
        else Show(true);
    }

    // wait a moment so the fox lunge is visible, then end the game
    IEnumerator GameOverAfterLunge()
    {
        yield return new WaitForSeconds(lungeTime);
        if (GameManager.I != null) GameManager.I.GameOver();
    }

    void Show(bool on) { if (fox != null) fox.SetActive(on); }
}