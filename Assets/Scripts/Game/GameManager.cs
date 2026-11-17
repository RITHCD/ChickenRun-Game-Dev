using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpeedController), typeof(ScoreManager), typeof(FoxManager))]
public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public static event Action OnGameOver;

    public bool autoStart = true;      // Visal turns this off when the Start screen is ready

    public GameState State { get; private set; }
    public bool IsPlaying { get { return State == GameState.Playing; } }
    public float Speed { get { return speedCtl.Speed; } }
    public int Score { get { return scoreMgr.Score; } }
    public int Eggs { get { return scoreMgr.Eggs; } }
    public int Best { get { return scoreMgr.Best; } }
    public int Hits { get { return foxMgr.Hits; } }

    SpeedController speedCtl;
    ScoreManager scoreMgr;
    FoxManager foxMgr;
    static bool restarting;
    GUIStyle scoreStyle;

    void Awake()
    {
        I = this;
        speedCtl = GetComponent<SpeedController>();
        scoreMgr = GetComponent<ScoreManager>();
        foxMgr = GetComponent<FoxManager>();
    }

    void Start()
    {
        State = GameState.Menu;
        Time.timeScale = 0f;
        if (autoStart || restarting) { restarting = false; StartGame(); }
    }

    public void StartGame()
    {
        speedCtl.ResetSpeed();
        scoreMgr.ResetScore();
        foxMgr.ResetFox();
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        restarting = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Your team calls these two from the player script
    public void AddEgg(int n = 1) { if (IsPlaying) scoreMgr.AddEgg(n); }
    public void Hit() { foxMgr.RegisterHit(); }

    public void GameOver()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;
        scoreMgr.SaveBest();
        Time.timeScale = 0f;
        if (OnGameOver != null) OnGameOver();
    }

    void OnGUI()
    {
        if (State == GameState.Menu) return;

        if (scoreStyle == null)
        {
            scoreStyle = new GUIStyle(GUI.skin.label);
            scoreStyle.fontSize = 32;
            scoreStyle.fontStyle = FontStyle.Bold;
            scoreStyle.alignment = TextAnchor.UpperRight;
            scoreStyle.normal.textColor = Color.white;
        }

        GUI.Label(new Rect(Screen.width - 220, 20, 200, 50), "Score: " + Score, scoreStyle);
    }
}