using UnityEngine;

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
    public float groundFriction = 12f; // m/s^2 applied against horizontal velocity

    [Header("Jump")]
    public float jumpForce = 7.5f;

    [Header("Grounding")]
    public LayerMask groundMask = ~0;
    [Tooltip("Extra distance below the capsule bottom to check for ground.")]
    public float groundCheckDistance = 0.15f;

    Rigidbody rb;
    CapsuleCollider col;

    float inputX, inputZ;
    bool jumpPressed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.drag = 0f; // critical: never use global drag for ground friction
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        if (Input.GetButton("Jump"))
            jumpPressed = true;
    }

    void FixedUpdate()
    {
        bool grounded = IsGrounded();

        // Desired horizontal velocity in player space
        Vector3 moveDir = (transform.right * inputX + transform.forward * inputZ).normalized;
        Vector3 targetVel = moveDir * moveSpeed;

        Vector3 v = rb.velocity;
        Vector3 horiz = new Vector3(v.x, 0f, v.z);

        // Acceleration cap per step
        float maxDelta = (grounded ? groundAcceleration : airAcceleration) * Time.fixedDeltaTime;

        // Move toward target horizontal velocity
        Vector3 needed = targetVel - horiz;
        Vector3 delta = Vector3.ClampMagnitude(needed, maxDelta);
        horiz += delta;

        // Ground-only horizontal friction when there's little/no input
        if (grounded && targetVel.sqrMagnitude < 0.01f && horiz.sqrMagnitude > 0f)
        {
            // Apply an acceleration opposite to current horizontal velocity
            Vector3 frictionAccel = -horiz.normalized * groundFriction * Time.fixedDeltaTime;
            // Don’t overshoot past zero
            if (frictionAccel.magnitude > horiz.magnitude)
                horiz = Vector3.zero;
            else
                horiz += frictionAccel;
        }

        // Commit velocity (preserve vertical; never touch gravity)
        rb.velocity = new Vector3(horiz.x, v.y, horiz.z);

        // Jump (only when grounded)
        if (jumpPressed && grounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            //rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        jumpPressed = false;
    }

    bool IsGrounded()
    {
        // Build capsule endpoints at current pose
        float radius = col.radius * Mathf.Abs(transform.localScale.x);
        float height = Mathf.Max(col.height * Mathf.Abs(transform.localScale.y), radius * 2f);

        Vector3 centerWorld = transform.TransformPoint(col.center);
        Vector3 up = transform.up;
        float half = height * 0.5f - radius;

        Vector3 top = centerWorld + up * half;
        Vector3 bottom = centerWorld - up * half;

        // Cast the capsule a short distance downward; if it hits, we’re grounded
        return Physics.CapsuleCast(top, bottom, radius * 0.98f, -up, out _, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }
}
