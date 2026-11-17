using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpeedController), typeof(ScoreManager), typeof(FoxManager))]
public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public static event Action OnGameOver;

    public bool autoStart = false;     // the Start panel in PlayerControl starts the game

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

    void Awake()
    {
        I = this;
        speedCtl = GetComponent<SpeedController>();
        scoreMgr = GetComponent<ScoreManager>();
        foxMgr = GetComponent<FoxManager>();
        State = GameState.Menu;
    }

    void Start()
    {
        if (autoStart) StartGame();
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // PlayerControl calls these two
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

    void OnGUI()   // temporary test text, delete before submitting
    {
        GUI.Label(new Rect(10, 10, 450, 25),
            "State " + State + "  Speed " + Speed.ToString("0.0") + "  Hits " + Hits + "  Score " + Score + "  Eggs " + Eggs);
    }
}