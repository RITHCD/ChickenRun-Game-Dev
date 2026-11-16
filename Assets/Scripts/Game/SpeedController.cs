using UnityEngine;

public class SpeedController : MonoBehaviour
{
    public float startSpeed = 12f;
    public float maxSpeed = 32f;
    public float speedUp = 0.25f;

    public float Speed { get; private set; }

    void Awake() { Speed = startSpeed; }

    public void ResetSpeed() { Speed = startSpeed; }

    void Update()
    {
        if (GameManager.I == null || !GameManager.I.IsPlaying) return;
        Speed = Mathf.Min(Speed + speedUp * Time.deltaTime, maxSpeed);
    }
}