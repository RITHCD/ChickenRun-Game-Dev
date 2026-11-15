using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerControl : MonoBehaviour
{
    [Header("Running Settings")]
    public float speed = 12f;
    public float maxSpeed = 30f;
    public float accel = 0.25f;
    public float laneDistance = 2f;
    public float laneChangeSpeed = 15f;

    [Header("Jump Settings")]
    public bool canJump = true;
    public float jumpForce = 8f;
    public float gravity = 25f;

    [Header("Crouch / Slide Settings")]
    public bool canCrouch = true;
    public float crouchDuration = 0.8f;
    public float crouchColliderHeight = 0.5f;
    public Vector3 crouchColliderCenter = new Vector3(0f, 0.25f, 0f);
    [Range(0.2f, 1f)] public float crouchScaleY = 0.5f;
    public Transform modelTransform;
    public bool isCrouching = false;

    [Header("Camera Follow")]
    public bool followCamera = true;
    public Vector3 cameraOffset = new Vector3(0f, 3.6f, -6.5f);

    [Header("Animation (Legs)")]
    public Transform leftLeg;
    public Transform rightLeg;
    public float legSwingSpeed = 14f;
    public float legSwingAngle = 30f;

    [Header("Score & State")]
    public int score = 0;
    public bool isDead = false;
    public bool stopOnHit = true;

    private int lane = 0;
    private float targetX = 0f;
    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    private float crouchTimer = 0f;

    private CapsuleCollider col;
    private float originalColliderHeight = 1.2f;
    private Vector3 originalColliderCenter = new Vector3(0f, 0.6f, 0f);
    private Vector3 originalModelScale = Vector3.one;
    private Vector3 originalModelPosition = Vector3.zero;

    void Start()
    {
        // Auto-connect to WorldManager if player reference is missing
        var wm = Object.FindFirstObjectByType<WorldManager>();
        if (wm != null && wm.player == null)
        {
            wm.player = this.transform;
        }

        // Cache collider and original dimensions
        col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            originalColliderHeight = col.height;
            originalColliderCenter = col.center;
        }

        // Cache visual model and original scale
        if (modelTransform == null)
        {
            modelTransform = transform.Find("BaseAnimal");
        }
        if (modelTransform != null)
        {
            originalModelScale = modelTransform.localScale;
            originalModelPosition = modelTransform.localPosition;
        }

        // Auto-find chicken legs if not assigned in Inspector
        if (leftLeg == null) leftLeg = transform.Find("BaseAnimal/Body/Legs/Leg_L_top");
        if (rightLeg == null) rightLeg = transform.Find("BaseAnimal/Body/Legs/Leg_R_top");
    }

    void Update()
    {
        if (isDead) return;

        // Forward speed acceleration
        speed = Mathf.Min(speed + accel * Time.deltaTime, maxSpeed);

        HandleInput();
        MovePlayer();
        UpdateCrouch();
        AnimateLegs();
    }

    void HandleInput()
    {
        bool leftPressed = false;
        bool rightPressed = false;
        bool jumpPressed = false;
        bool crouchPressed = false;
        bool crouchHeld = false;

#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k != null)
        {
            leftPressed = k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame;
            rightPressed = k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame;
            jumpPressed = k.spaceKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame;
            crouchPressed = k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame || k.cKey.wasPressedThisFrame;
            crouchHeld = k.sKey.isPressed || k.downArrowKey.isPressed || k.cKey.isPressed;
        }
#else
        leftPressed = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        rightPressed = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
        jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        crouchPressed = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.C);
        crouchHeld = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.C);
#endif

        // Lane changes
        if (leftPressed) lane = Mathf.Max(lane - 1, -1);
        if (rightPressed) lane = Mathf.Min(lane + 1, 1);

        // Crouch / Slide trigger
        if (canCrouch && crouchPressed)
        {
            crouchTimer = crouchDuration;
            // Fast dive if in mid-air
            if (!isGrounded)
            {
                verticalVelocity = -gravity * 1.5f;
            }
        }

        // Keep crouching if key is held down
        if (canCrouch && crouchHeld && crouchTimer > 0f)
        {
            crouchTimer = Mathf.Max(crouchTimer, 0.15f);
        }

        // Count down crouch duration
        if (crouchTimer > 0f)
        {
            crouchTimer -= Time.deltaTime;
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }

        // Jump (cancels crouch)
        if (canJump && isGrounded && jumpPressed)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
            isCrouching = false;
            crouchTimer = 0f;
        }
    }

    void MovePlayer()
    {
        Vector3 p = transform.position;

        // Smooth horizontal lane transition
        targetX = lane * laneDistance;
        p.x = Mathf.Lerp(p.x, targetX, laneChangeSpeed * Time.deltaTime);

        // Forward movement
        p.z += speed * Time.deltaTime;

        // Vertical jumping & gravity
        if (!isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            p.y += verticalVelocity * Time.deltaTime;
            if (p.y <= 0f)
            {
                p.y = 0f;
                verticalVelocity = 0f;
                isGrounded = true;
            }
        }
        else
        {
            p.y = 0f;
        }

        transform.position = p;
    }

    void UpdateCrouch()
    {
        float targetHeight = isCrouching ? crouchColliderHeight : originalColliderHeight;
        Vector3 targetCenter = isCrouching ? crouchColliderCenter : originalColliderCenter;

        // Smoothly adjust capsule collider height and center
        if (col != null)
        {
            col.height = Mathf.Lerp(col.height, targetHeight, 20f * Time.deltaTime);
            col.center = Vector3.Lerp(col.center, targetCenter, 20f * Time.deltaTime);
        }

        // Smoothly squash visual model when crouching
        if (modelTransform != null)
        {
            Vector3 targetScale = isCrouching
                ? new Vector3(originalModelScale.x * 1.15f, originalModelScale.y * crouchScaleY, originalModelScale.z * 1.15f)
                : originalModelScale;
            Vector3 targetPos = isCrouching
                ? new Vector3(originalModelPosition.x, originalModelPosition.y * crouchScaleY, originalModelPosition.z)
                : originalModelPosition;

            modelTransform.localScale = Vector3.Lerp(modelTransform.localScale, targetScale, 20f * Time.deltaTime);
            modelTransform.localPosition = Vector3.Lerp(modelTransform.localPosition, targetPos, 20f * Time.deltaTime);
        }
    }

    void AnimateLegs()
    {
        if (leftLeg == null || rightLeg == null) return;

        // Don't swing legs while crouching/sliding
        if (isCrouching)
        {
            leftLeg.localRotation = Quaternion.Euler(0f, -90f, 0f);
            rightLeg.localRotation = Quaternion.Euler(0f, -90f, 0f);
            return;
        }

        float angle = Mathf.Sin(Time.time * legSwingSpeed * (speed / 12f)) * legSwingAngle;
        leftLeg.localRotation = Quaternion.Euler(0f, -90f, angle);
        rightLeg.localRotation = Quaternion.Euler(0f, -90f, -angle);
    }

    void LateUpdate()
    {
        if (!followCamera) return;
        var cam = Camera.main;
        if (cam == null) return;

        Vector3 p = transform.position + cameraOffset;
        p.x = transform.position.x * 0.5f;
        cam.transform.position = p;
        cam.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
    }

    // =========================================================================
    // Collision & Trigger Detection (Obstacles & Eggs)
    // =========================================================================

    /// <summary>
    /// Trigger detection: called when the chicken (or the object) has Is Trigger enabled.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            HandleHit(other.gameObject);
        }
    }

    /// <summary>
    /// Physical collision detection: called when non-trigger colliders touch.
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.gameObject != null)
        {
            HandleHit(collision.gameObject);
        }
    }

    /// <summary>
    /// Unified detection logic handling both OnTriggerEnter and OnCollisionEnter.
    /// </summary>
    void HandleHit(GameObject hitObj)
    {
        if (isDead || hitObj == null) return;

        // -----------------------------------------------------------------
        // 1. DETECT EGGS (Normal & Gold)
        // -----------------------------------------------------------------
        bool isGoldEgg = hitObj.CompareTag("Gold") || hitObj.name.Contains("Gold");
        bool isNormalEgg = hitObj.CompareTag("Egg") || hitObj.name.Contains("Egg");

        if (isGoldEgg || isNormalEgg)
        {
            int points = isGoldEgg ? 5 : 1;
            score += points;
            Debug.Log($"[ChickenPlayer] COLLECTED {(isGoldEgg ? "GOLD EGG (+5)" : "EGG (+1)")}! Current Score: {score}");

            // Find top-level egg object to destroy the entire egg prefab
            Transform eggRoot = hitObj.transform;
            while (eggRoot.parent != null && (eggRoot.parent.name.Contains("Egg") || eggRoot.parent.name.Contains("Gold")))
            {
                eggRoot = eggRoot.parent;
            }
            Destroy(eggRoot.gameObject);
            return;
        }

        // -----------------------------------------------------------------
        // 2. DETECT OBSTACLES (Beam, Bench, Fence, Haybale, Rock, etc.)
        // -----------------------------------------------------------------
        Obstacle obstacle = hitObj.GetComponentInParent<Obstacle>();
        bool isObstacle = obstacle != null || hitObj.CompareTag("Obstacle") || hitObj.name.Contains("Obstacle");

        if (isObstacle)
        {
            string obsName = obstacle != null ? obstacle.gameObject.name : hitObj.name;
            string obsType = obstacle != null ? obstacle.type.ToString() : "General";

            Debug.Log($"[ChickenPlayer] HIT OBSTACLE! Touched: {obsName} (Type: {obsType})");

            if (stopOnHit)
            {
                isDead = true;
                speed = 0f;
            }
        }
    }
}
