using System.Collections;
using UnityEngine;

public class FoxManager : MonoBehaviour
{
    public GameObject fox;             // old slot, not used any more (leave empty)
    public float foxAwayTime = 10f;    // safe seconds before the fox drops back
    public float hitCooldown = 1.2f;   // ignore extra hits right after a hit
    public float lungeTime = 0.7f;     // let the fox catch the chicken before game over

    public int Hits { get; private set; }

    float foxTimer, nextHitTime;
    bool ready;

    void Start() { Setup(); }

    // CHANGED: runs once. Find the FoxChase in the scene (or make one) and give it our values.
    void Setup()
    {
        if (ready) return;
        ready = true;

        if (FoxChase.I == null)
        {
            GameObject go = new GameObject("FoxChase");
            go.AddComponent<FoxChase>();
        }

        FoxChase fc = FoxChase.I;
        if (fc == null) return;

        if (fc.player == null)
        {
            PlayerControl pc = FindFirstObjectByType<PlayerControl>();
            if (pc != null) fc.player = pc.transform;
        }

        fc.stopTimeOnCatch = false;        // GameManager handles game over
        fc.forgiveTime = foxAwayTime;
        fc.hitCooldown = hitCooldown;
    }

    void Update()
    {
        if (GameManager.I == null || !GameManager.I.IsPlaying) return;
        if (Hits == 0 || Hits >= 2) return;
        foxTimer += Time.deltaTime;
        if (foxTimer > foxAwayTime) Hits = 0;
    }

    public void ResetFox()
    {
        Setup();
        Hits = 0; foxTimer = 0f; nextHitTime = 0f;
        if (FoxChase.I != null) FoxChase.I.ResetChase();   // CHANGED: fox starts running behind the chicken
    }

    public void RegisterHit()
    {
        Setup();
        if (GameManager.I == null || !GameManager.I.IsPlaying) return;
        if (Hits >= 2) return;                      // already caught, waiting for game over
        if (Time.time < nextHitTime) return;
        nextHitTime = Time.time + hitCooldown;
        Hits++; foxTimer = 0f;
        Debug.Log("Fox hit " + Hits + " of 2");     // CHANGED: so you can see every hit in the Console

        if (FoxChase.I != null) FoxChase.I.Hit();   // fox runs up close, or catches on the 2nd hit

        if (Hits >= 2) StartCoroutine(GameOverAfterLunge());
    }

    // wait a moment so the fox lunge is visible, then end the game
    IEnumerator GameOverAfterLunge()
    {
        yield return new WaitForSeconds(lungeTime);
        if (GameManager.I != null) GameManager.I.GameOver();
    }
}