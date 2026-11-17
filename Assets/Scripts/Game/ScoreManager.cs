using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int Eggs { get; private set; }
    public float Distance { get; private set; }
    public int Score { get { return Mathf.FloorToInt(Distance / 10f) + Eggs; } }
    public int Best { get { return PlayerPrefs.GetInt("best", 0); } }

    public void ResetScore() { Eggs = 0; Distance = 0f; }
    public void AddEgg(int n) { Eggs += n; }

    public void SaveBest()
    {
        if (Score > Best) PlayerPrefs.SetInt("best", Score);
    }

    void Update()
    {
        if (GameManager.I == null || !GameManager.I.IsPlaying) return;
        Distance += GameManager.I.Speed * Time.deltaTime;
    }
}