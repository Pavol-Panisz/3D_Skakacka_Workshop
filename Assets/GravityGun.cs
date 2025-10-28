using UnityEngine;

public class GravityGun : MonoBehaviour
{
    [Header("Pickup Settings")]

    [Tooltip("Maximum distance from the camera at which objects can be held.")]
    public float holdUpDistance = 3f;

    [Tooltip("Radius of the sphere used to detect objects when attempting to pick them up.")]
    public float pickupRadius = 0.5f;

    [Tooltip("How quickly the held object moves and rotates to match the hold position and camera orientation. Higher = tighter follow.")]
    public float holdSmoothness = 12f;

    [Tooltip("Force (as velocity change) applied to the object when thrown with the right mouse button.")]
    public float throwSpeed = 12f;

    [Tooltip("Short delay (in seconds) after throwing before new objects can be picked up. Prevents accidental re-grabs.")]
    public float throwCooldown = 0.15f;

    [Tooltip("If the held object drifts farther than this distance from the target hold position, it is automatically released.")]
    public float maxHoldDistance = 5f;

    [Tooltip("Layer mask defining which objects can be picked up by the gravity gun.")]
    public LayerMask pickupMask = ~0;


    // --- Private state ---
    Rigidbody heldRb;
    Transform heldTransform;
    float originalDrag;
    bool isHolding;
    bool canPickup = true;
    float cooldownTimer = 0f;


    void Update()
    {
        // Countdown for re-enabling pickup
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // --- Handle throwing ---
        if (isHolding && Input.GetMouseButtonDown(1))
        {
            Throw();
            canPickup = false;
            cooldownTimer = throwCooldown;
        }

        // --- Handle pickup and drop ---
        if (canPickup && cooldownTimer <= 0f)
        {
            if (Input.GetMouseButton(0))
            {
                if (!isHolding)
                    TryPickup();
            }
            else if (isHolding)
            {
                Drop();
            }
        }

        // Re-enable pickup only once RMB released *and* cooldown finished
        if (!Input.GetMouseButton(1) && cooldownTimer <= 0f)
            canPickup = true;
    }


    void FixedUpdate()
    {
        if (isHolding && heldRb != null)
        {
            // Desired hold position
            Vector3 targetPos = transform.position + transform.forward * holdUpDistance;
            Vector3 toTarget = targetPos - heldTransform.position;

            // --- Distance safety check ---
            float currentDistance = toTarget.magnitude;
            if (currentDistance > maxHoldDistance)
            {
                Drop();
                return;
            }

            // --- Position follow ---
            heldRb.velocity = toTarget * holdSmoothness;

            // --- Rotation follow ---
            Quaternion targetRot = transform.rotation;
            heldRb.MoveRotation(Quaternion.Slerp(
                heldRb.rotation,
                targetRot,
                Time.fixedDeltaTime * holdSmoothness));
        }
    }


    void TryPickup()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.SphereCast(ray, pickupRadius, out RaycastHit hit, holdUpDistance, pickupMask, QueryTriggerInteraction.Ignore))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb != null && !rb.isKinematic)
            {
                heldRb = rb;
                heldTransform = rb.transform;
                originalDrag = rb.drag;

                rb.drag = 10f;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;

                isHolding = true;
            }
        }
    }


    void Drop()
    {
        if (heldRb == null) return;

        heldRb.drag = originalDrag;
        heldRb.useGravity = true;
        heldRb.velocity = Vector3.zero;

        heldRb = null;
        heldTransform = null;
        isHolding = false;
    }


    void Throw()
    {
        if (heldRb == null) return;

        Rigidbody rb = heldRb; // store before drop
        Drop();                // fully release it

        rb.velocity = Vector3.zero;
        rb.AddForce(transform.forward * throwSpeed, ForceMode.VelocityChange);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * holdUpDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * holdUpDistance, maxHoldDistance - holdUpDistance);
    }
}
