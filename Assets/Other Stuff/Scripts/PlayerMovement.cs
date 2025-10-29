using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float groundAcceleration = 20f;
    public float airAcceleration = 6f;

    [Header("Friction (horizontal only)")]
    [Tooltip("How quickly horizontal velocity decays when grounded and no input is given.")]
    public float groundFriction = 12f;

    [Header("Jump")]
    public float jumpForce = 7.5f;
    [Tooltip("Small grace period allowing jump slightly after leaving the ground.")]
    public float coyoteTime = 0.1f;

    [Header("Grounding")]
    public LayerMask groundMask = ~0;
    [Tooltip("Extra distance below the capsule bottom to check for ground.")]
    public float groundCheckDistance = 0.15f;

    [Header("Air Jumps")]
    [Tooltip("How many extra jumps can be made while in air.")]
    public int maxAirJumps = 5;
    [Tooltip("How many air jumps are currently available.")]
    public int remainingAirJumps = 2;
    [Tooltip("If true, air jumps reset automatically when hitting the ground.")]
    public bool resetAirJumpsOnImpact = true;

    Rigidbody rb;
    CapsuleCollider col;

    float inputX, inputZ;
    bool jumpPressed;

    bool grounded;
    bool wasGrounded;
    bool jumpJustHappened;
    float coyoteTimer;

    Text jumpsText;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.drag = 0f;
    }

    void Start()
    {
        // Keep the value the user sets in inspector, but ensure it's valid
        remainingAirJumps = Mathf.Clamp(remainingAirJumps, 0, maxAirJumps);

        GameObject textObj = GameObject.FindGameObjectWithTag("JumpsText");
        if (textObj != null)
            jumpsText = textObj.GetComponent<Text>();

        UpdateJumpsText();
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;

        // Reload scene
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void FixedUpdate()
    {
        wasGrounded = grounded;
        grounded = IsGrounded();

        // Maintain coyote time
        if (grounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.fixedDeltaTime;

        // Reset air jumps if toggle enabled
        if (resetAirJumpsOnImpact && grounded && !wasGrounded)
        {
            remainingAirJumps = maxAirJumps;
            UpdateJumpsText();
        }

        // Handle horizontal velocity
        Vector3 moveDir = (transform.right * inputX + transform.forward * inputZ).normalized;
        Vector3 targetVel = moveDir * moveSpeed;

        Vector3 v = rb.velocity;
        Vector3 horiz = new Vector3(v.x, 0f, v.z);

        float maxDelta = (grounded ? groundAcceleration : airAcceleration) * Time.fixedDeltaTime;
        Vector3 needed = targetVel - horiz;
        Vector3 delta = Vector3.ClampMagnitude(needed, maxDelta);
        horiz += delta;

        if (grounded && targetVel.sqrMagnitude < 0.01f && horiz.sqrMagnitude > 0f)
        {
            Vector3 frictionAccel = -horiz.normalized * groundFriction * Time.fixedDeltaTime;
            if (frictionAccel.magnitude > horiz.magnitude)
                horiz = Vector3.zero;
            else
                horiz += frictionAccel;
        }

        rb.velocity = new Vector3(horiz.x, v.y, horiz.z);

        // --- Jump handling ---
        if (jumpPressed)
        {
            if (coyoteTimer > 0f && !jumpJustHappened)
            {
                DoJump();
                jumpJustHappened = true;
            }
            else if (!grounded && remainingAirJumps > 0)
            {
                DoJump();
                remainingAirJumps = Mathf.Clamp(remainingAirJumps - 1, 0, maxAirJumps);
                UpdateJumpsText();
            }

            jumpPressed = false;
        }

        if (!grounded)
            jumpJustHappened = false;
    }

    void DoJump()
    {
        Vector3 vel = rb.velocity;
        if (vel.y < 0f) vel.y = 0f;
        rb.velocity = new Vector3(vel.x, jumpForce, vel.z);
    }

    bool IsGrounded()
    {
        float radius = col.radius * Mathf.Abs(transform.localScale.x);
        float height = Mathf.Max(col.height * Mathf.Abs(transform.localScale.y), radius * 2f);

        Vector3 centerWorld = transform.TransformPoint(col.center);
        Vector3 up = transform.up;
        float half = height * 0.5f - radius;

        Vector3 top = centerWorld + up * half;
        Vector3 bottom = centerWorld - up * half;

        return Physics.CapsuleCast(top, bottom, radius * 0.98f, -up, out _, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    void UpdateJumpsText()
    {
        if (jumpsText != null)
            jumpsText.text = "Air jumps: " + remainingAirJumps + "/" + maxAirJumps;
    }

    public void SetAirJumps(int i)
    {
        
        remainingAirJumps = Mathf.Clamp(remainingAirJumps, 0, maxAirJumps);
        UpdateJumpsText();
    }

    public void AddAirJumps(int i)
    {
        
        remainingAirJumps = Mathf.Clamp(remainingAirJumps + i, 0, maxAirJumps);
        UpdateJumpsText();
    }
}
