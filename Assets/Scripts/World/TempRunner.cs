using UnityEngine;
using UnityEngine.InputSystem;

public class TempRunner : MonoBehaviour
{
    public float speed = 12f, maxSpeed = 30f, accel = 0.25f, laneDistance = 2f;
    public Vector3 cameraOffset = new Vector3(0f, 3.6f, -6.5f);
    int lane;

    void Update()
    {
        speed = Mathf.Min(speed + accel * Time.deltaTime, maxSpeed);
        var k = Keyboard.current;
        if (k != null)
        {
            if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame) lane = Mathf.Max(lane - 1, -1);
            if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame) lane = Mathf.Min(lane + 1, 1);
        }
        Vector3 p = transform.position;
        p.x = Mathf.Lerp(p.x, lane * laneDistance, 12f * Time.deltaTime);
        p.z += speed * Time.deltaTime;
        transform.position = p;
    }

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 p = transform.position + cameraOffset;
        p.x = transform.position.x * 0.5f;
        cam.transform.position = p;
        cam.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Egg") || other.CompareTag("Gold")) Destroy(other.gameObject);
        else if (other.GetComponent<Obstacle>() != null) Debug.Log("Hit obstacle: " + other.name);
    }
}
