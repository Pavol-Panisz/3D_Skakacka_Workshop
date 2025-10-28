using UnityEngine;

public class GravityGun : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float holdUpDistance = 3f;
    public float pickupRadius = 0.5f;
    public float holdSmoothness = 12f;
    public float throwSpeed = 12f;
    public LayerMask pickupMask = ~0;

    Rigidbody heldRb;
    Transform heldTransform;
    float originalDrag;
    bool isHolding;
    bool canPickup = true; // blocks re-pickup until RMB is released

    void Update()
    {
        // --- Handle throwing ---
        if (isHolding && Input.GetMouseButtonDown(1))
        {
            Throw();
            canPickup = false; // block pickup until RMB released
        }

        // --- Handle pickup and drop ---
        if (canPickup)
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

        // Re-enable pickup when right mouse button is released
        if (!Input.GetMouseButton(1))
            canPickup = true;
    }

    void FixedUpdate()
    {
        if (isHolding && heldRb != null)
        {
            Vector3 targetPos = transform.position + transform.forward * holdUpDistance;
            Vector3 moveDir = (targetPos - heldTransform.position);
            heldRb.velocity = moveDir * holdSmoothness;
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
    }
}
