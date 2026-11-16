using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Subway-Surfers style chaser:
//   1st obstacle hit -> the fox appears behind the chicken and runs up close
//   2nd hit while the fox is chasing -> the fox catches the chicken, game over
//   no hit for forgiveTime seconds -> the fox gives up and leaves
public class FoxChase : MonoBehaviour
{
    public static FoxChase I;
    public static event Action Caught;      // other scripts can subscribe (game over UI)

    [Header("References")]
    public Transform player;                // auto-found by the tag "Player"
    public GameObject foxPrefab;            // optional: your own fox model (should face +Z)

    [Header("Chase settings")]
    public float chaseGap = 3.5f;           // distance behind the chicken while chasing
    public float hiddenGap = 20f;           // where the fox starts and leaves to
    public float closeSpeed = 6f;           // how fast the fox runs up to the chicken
    public float forgiveTime = 10f;         // safe seconds before the fox gives up
    public float hitCooldown = 1f;          // ignore repeat hits right after a hit
    public float sideSpeed = 8f;            // how fast the fox changes lane
    public bool stopTimeOnCatch = true;     // untick if your GameManager handles game over

    [Header("Events")]
    public UnityEvent onFirstHit;
    public UnityEvent onCaught;

    public int Hits { get; private set; }
    public bool IsCaught { get; private set; }
    public bool IsChasing { get { return fox != null && fox.activeSelf; } }

    GameObject fox;
    Transform tail;
    float gap, targetGap, chaseTimer, cooldown, runTime;
    bool retreating;

    void Awake()
    {
        I = this;
        Time.timeScale = 1f;
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
        fox = foxPrefab != null ? Instantiate(foxPrefab) : BuildFox();
        fox.name = "Fox";
        fox.SetActive(false);
        gap = targetGap = hiddenGap;
    }

    // Call this when the chicken hits an obstacle
    public void Hit()
    {
        if (IsCaught || cooldown > 0f || player == null) return;
        cooldown = hitCooldown;
        Hits++;

        if (Hits >= 2) { Catch(); return; }

        retreating = false;
        chaseTimer = forgiveTime;
        if (!fox.activeSelf)
        {
            gap = hiddenGap;
            fox.transform.position = new Vector3(player.position.x, 0f, player.position.z - gap);
            fox.SetActive(true);
        }
        targetGap = chaseGap;
        onFirstHit.Invoke();
    }

    void Catch()
    {
        IsCaught = true;
        retreating = false;
        if (!fox.activeSelf)
        {
            gap = hiddenGap;
            fox.transform.position = new Vector3(player.position.x, 0f, player.position.z - gap);
            fox.SetActive(true);
        }
        targetGap = 0.9f;                    // fox lunges at the chicken
        onCaught.Invoke();
        if (Caught != null) Caught();
        if (stopTimeOnCatch) StartCoroutine(StopSoon());
        Debug.Log("The fox caught the chicken. Game over.");
    }

    IEnumerator StopSoon()
    {
        yield return new WaitForSeconds(0.7f);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (player == null || fox == null) return;
        cooldown -= Time.deltaTime;
        if (!fox.activeSelf) return;
        runTime += Time.deltaTime;

        if (!IsCaught && !retreating)
        {
            chaseTimer -= Time.deltaTime;
            if (chaseTimer <= 0f)
            {
                retreating = true;           // chicken survived, fox gives up
                targetGap = hiddenGap;
                Hits = 0;
            }
        }

        float speed = IsCaught ? closeSpeed * 2.5f : closeSpeed;
        gap = Mathf.MoveTowards(gap, targetGap, speed * Time.deltaTime);

        if (retreating && gap >= hiddenGap - 0.01f)
        {
            fox.SetActive(false);
            retreating = false;
            return;
        }

        Vector3 pos = fox.transform.position;
        pos.x = Mathf.Lerp(pos.x, player.position.x, sideSpeed * Time.deltaTime);
        pos.z = player.position.z - gap;
        pos.y = Mathf.Abs(Mathf.Sin(runTime * 14f)) * 0.25f;   // running hop
        fox.transform.position = pos;
        if (tail != null) tail.localRotation = Quaternion.Euler(0f, Mathf.Sin(runTime * 12f) * 25f, 0f);
    }

    // ---------- simple low-poly fox built from cubes ----------
    GameObject BuildFox()
    {
        var root = new GameObject("Fox");
        root.transform.localScale = Vector3.one * 1.3f;
        Material orange = Mat(new Color(0.91f, 0.42f, 0.10f));
        Material white = Mat(new Color(0.97f, 0.95f, 0.90f));
        Material dark = Mat(new Color(0.15f, 0.10f, 0.08f));
        Transform t = root.transform;

        Part(t, "Body", new Vector3(0f, 0.55f, 0f), new Vector3(0.6f, 0.5f, 1.1f), orange);
        Part(t, "Head", new Vector3(0f, 0.85f, 0.7f), new Vector3(0.5f, 0.42f, 0.45f), orange);
        Part(t, "Snout", new Vector3(0f, 0.78f, 1.02f), new Vector3(0.22f, 0.18f, 0.3f), white);
        Part(t, "Nose", new Vector3(0f, 0.82f, 1.18f), new Vector3(0.1f, 0.08f, 0.08f), dark);
        Part(t, "EarL", new Vector3(-0.16f, 1.12f, 0.65f), new Vector3(0.12f, 0.22f, 0.1f), dark);
        Part(t, "EarR", new Vector3(0.16f, 1.12f, 0.65f), new Vector3(0.12f, 0.22f, 0.1f), dark);
        Part(t, "EyeL", new Vector3(-0.13f, 0.93f, 0.93f), new Vector3(0.07f, 0.07f, 0.07f), dark);
        Part(t, "EyeR", new Vector3(0.13f, 0.93f, 0.93f), new Vector3(0.07f, 0.07f, 0.07f), dark);
        foreach (float x in new[] { -0.2f, 0.2f })
            foreach (float z in new[] { -0.4f, 0.4f })
                Part(t, "Leg", new Vector3(x, 0.17f, z), new Vector3(0.12f, 0.34f, 0.12f), dark);

        var pivot = new GameObject("TailPivot");
        pivot.transform.SetParent(t, false);
        pivot.transform.localPosition = new Vector3(0f, 0.6f, -0.55f);
        Part(pivot.transform, "Tail", new Vector3(0f, 0f, -0.35f), new Vector3(0.2f, 0.2f, 0.7f), orange);
        Part(pivot.transform, "TailTip", new Vector3(0f, 0f, -0.75f), new Vector3(0.22f, 0.22f, 0.22f), white);
        tail = pivot.transform;
        return root;
    }

    static void Part(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = mat;
        Destroy(g.GetComponent<Collider>());
    }

    static Material Mat(Color c)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        var m = new Material(s);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0f);
        return m;
    }
}
