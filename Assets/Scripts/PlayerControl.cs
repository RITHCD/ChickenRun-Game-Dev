using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    [Header("Player Movement")]
    public float speed = 12f;
    public float maxSpeed = 30f;
    public float accel = 0.25f;
    public float laneDistance = 2f;
    public float jumpForce = 8f;
    public float gravity = 25f;

    [Header("Crouch Settings")]
    public float crouchDuration = 0.8f;
    public float crouchHeight = 0.5f;

    [Header("Camera Follow")]
    public Vector3 cameraOffset = new Vector3(0f, 3.6f, -6.5f);

    [Header("Game Score")]
    public int score = 0;
    public bool isGameOver = false;

    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject pausePanel;
    public bool isGameStarted = false;
    public bool isPaused = false;

    private int lane = 0;
    private float yVelocity = 0f;
    private bool isGrounded = true;
    private bool isCrouching = false;
    private float crouchTimer = 0f;

    private CapsuleCollider col;
    private float originalHeight;
    private Vector3 originalCenter;

    private Transform chickenModel;
    private Vector3 originalModelScale;
    private Vector3 originalModelPos;

    void Start()
    {
        // Connect to WorldManager so endless tiles keep spawning
        WorldManager wm = FindObjectOfType<WorldManager>();
        if (wm != null)
        {
            if (wm.player == null)
            {
                wm.player = transform;
            }
            // Only spawn white eggs
            wm.goldChance = 0f;
        }

        // Get the main capsule collider on the chicken
        col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            originalHeight = col.height;
            originalCenter = col.center;
        }

        // Disable extra child colliders so only the main player collider is used
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < childColliders.Length; i++)
        {
            if (childColliders[i] != col)
            {
                childColliders[i].enabled = false;
            }
        }

        // Find the chicken visual model
        chickenModel = transform.Find("BaseAnimal");
        if (chickenModel != null)
        {
            originalModelScale = chickenModel.localScale;
            originalModelPos = chickenModel.localPosition;
        }

        // Ensure Canvas has camera assigned
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }

        // Setup Start panel
        if (startPanel == null)
        {
            if (canvas != null)
            {
                Transform startT = canvas.transform.Find("StartPanel");
                if (startT != null)
                {
                    startPanel = startT.gameObject;
                }
            }
            if (startPanel == null)
            {
                startPanel = GameObject.Find("StartPanel");
            }
            if (startPanel == null)
            {
                startPanel = GameObject.Find("StartPanel(Clone)");
            }
            if (startPanel == null && canvas != null)
            {
                for (int i = 0; i < canvas.transform.childCount; i++)
                {
                    Transform child = canvas.transform.GetChild(i);
                    if (child.name.ToLower().Contains("start"))
                    {
                        startPanel = child.gameObject;
                        break;
                    }
                }
            }
        }

        if (startPanel != null)
        {
            // Show start panel when first entering game
            startPanel.SetActive(true);
            isGameStarted = false;

            // Connect Play button to StartGame()
            Button playBtn = startPanel.GetComponentInChildren<Button>();
            if (playBtn != null)
            {
                playBtn.onClick.AddListener(StartGame);
            }
        }
        else
        {
            // If no start panel in scene, start playing immediately
            isGameStarted = true;
        }

        // Setup Pause panel
        if (pausePanel == null)
        {
            if (canvas != null)
            {
                Transform pauseT = canvas.transform.Find("Pause_Panel");
                if (pauseT != null)
                {
                    pausePanel = pauseT.gameObject;
                }
            }
            if (pausePanel == null)
            {
                pausePanel = GameObject.Find("Pause_Panel");
            }
            if (pausePanel == null && canvas != null)
            {
                for (int i = 0; i < canvas.transform.childCount; i++)
                {
                    Transform child = canvas.transform.GetChild(i);
                    if (child.name.ToLower().Contains("pause"))
                    {
                        pausePanel = child.gameObject;
                        break;
                    }
                }
            }
        }

        // Hook up Resume and Restart buttons on Pause panel
        if (pausePanel != null)
        {
            Button[] buttons = pausePanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                string btnName = btn.gameObject.name.ToLower();
                if (btnName.Contains("resume"))
                {
                    btn.onClick.AddListener(ResumeGame);
                }
                else if (btnName.Contains("restart"))
                {
                    btn.onClick.AddListener(RestartGame);
                }
            }
            pausePanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        isGameStarted = true;
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        var keyboard = Keyboard.current;

        // Toggle Pause with Escape key
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (isGameStarted && !isGameOver)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        // Wait until player clicks Play, and stop moving if paused or game over
        if (!isGameStarted || isPaused || isGameOver)
        {
            return;
        }

        // Increase speed slowly over time
        if (speed < maxSpeed)
        {
            speed += accel * Time.deltaTime;
        }

        // Handle keyboard input
        if (keyboard != null)
        {
            // Move left
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            {
                if (lane > -1)
                {
                    lane--;
                }
            }

            // Move right
            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            {
                if (lane < 1)
                {
                    lane++;
                }
            }

            // Jump
            if ((keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) && isGrounded)
            {
                yVelocity = jumpForce;
                isGrounded = false;
                isCrouching = false;
                crouchTimer = 0f;
            }

            // Crouch (press or hold S / Down arrow)
            if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                crouchTimer = crouchDuration;

                // Fall faster if crouching while in the air
                if (!isGrounded)
                {
                    yVelocity = -gravity;
                }
            }

            // Check if player is crouching
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                isCrouching = true;
            }
            else if (crouchTimer > 0f)
            {
                crouchTimer -= Time.deltaTime;
                isCrouching = true;
            }
            else
            {
                isCrouching = false;
            }
        }

        // Adjust collider and model when crouching
        if (isCrouching)
        {
            if (col != null)
            {
                col.height = crouchHeight;
                col.center = new Vector3(0f, crouchHeight * 0.5f, 0f);
            }

            if (chickenModel != null)
            {
                Vector3 duckScale = new Vector3(originalModelScale.x, originalModelScale.y * 0.5f, originalModelScale.z);
                Vector3 duckPos = new Vector3(originalModelPos.x, originalModelPos.y * 0.5f, originalModelPos.z);
                chickenModel.localScale = Vector3.Lerp(chickenModel.localScale, duckScale, 20f * Time.deltaTime);
                chickenModel.localPosition = Vector3.Lerp(chickenModel.localPosition, duckPos, 20f * Time.deltaTime);
            }
        }
        else
        {
            if (col != null)
            {
                col.height = originalHeight;
                col.center = originalCenter;
            }

            if (chickenModel != null)
            {
                chickenModel.localScale = Vector3.Lerp(chickenModel.localScale, originalModelScale, 20f * Time.deltaTime);
                chickenModel.localPosition = Vector3.Lerp(chickenModel.localPosition, originalModelPos, 20f * Time.deltaTime);
            }
        }

        // Move player forward and change lanes
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, lane * laneDistance, 12f * Time.deltaTime);
        pos.z += speed * Time.deltaTime;

        // Apply jumping and gravity
        if (!isGrounded)
        {
            yVelocity -= gravity * Time.deltaTime;
            pos.y += yVelocity * Time.deltaTime;

            if (pos.y <= 0f)
            {
                pos.y = 0f;
                yVelocity = 0f;
                isGrounded = true;
            }
        }

        transform.position = pos;
    }

    void LateUpdate()
    {
        // Make camera follow behind the player
        var cam = Camera.main;
        if (cam != null)
        {
            Vector3 camPos = transform.position + cameraOffset;
            camPos.x = transform.position.x * 0.5f;
            cam.transform.position = camPos;
            cam.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
        }
    }

    // Trigger detection for eggs and obstacles
    void OnTriggerEnter(Collider other)
    {
        // Collect white egg (+1 score)
        if (other.CompareTag("Egg") || other.name.Contains("Egg"))
        {
            score++;
            Destroy(other.gameObject);
            return;
        }

        // Hit obstacle -> stop the game
        Obstacle obstacle = other.GetComponentInParent<Obstacle>();
        if (obstacle != null)
        {
            // Duck under high beam when crouching
            if (obstacle.type == Obstacle.Type.High && isCrouching)
            {
                return;
            }

            Debug.Log("Hit obstacle: " + other.name);
            isGameOver = true;
            speed = 0f;
        }
        else if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Hit obstacle: " + other.name);
            isGameOver = true;
            speed = 0f;
        }
    }

    // Physical collision detection
    void OnCollisionEnter(Collision collision)
    {
        Obstacle obstacle = collision.gameObject.GetComponentInParent<Obstacle>();
        if (obstacle != null)
        {
            // Duck under high beam when crouching
            if (obstacle.type == Obstacle.Type.High && isCrouching)
            {
                return;
            }

            Debug.Log("Hit obstacle: " + collision.gameObject.name);
            isGameOver = true;
            speed = 0f;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Hit obstacle: " + collision.gameObject.name);
            isGameOver = true;
            speed = 0f;
        }
    }
}
