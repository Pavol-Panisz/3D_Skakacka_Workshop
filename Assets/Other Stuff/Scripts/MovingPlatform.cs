using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    public Vector3 startOffset = Vector3.zero;
    public Vector3 endOffset = new Vector3(0f, 0f, 5f);
    public float speed = 1f;

    private Rigidbody rb;
    private Vector3 startPos;
    private Vector3 endPos;
    private float time;
    private bool initialized;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Start()
    {
        // Lock world-space endpoints once at the start of play
        startPos = transform.TransformPoint(startOffset);
        endPos = transform.TransformPoint(endOffset);
        initialized = true;
    }

    void OnEnable()
    {
        // Do NOT reset time — we want to resume from where we left off
        if (initialized)
        {
            // Optionally snap position exactly to where it should be based on current time
            float t = (1f - Mathf.Cos(time)) * 0.5f;
            rb.position = Vector3.Lerp(startPos, endPos, t);
        }
    }

    void FixedUpdate()
    {
        time += Time.fixedDeltaTime * speed;
        float t = (1f - Mathf.Cos(time)) * 0.5f; // smooth easing
        Vector3 targetPos = Vector3.Lerp(startPos, endPos, t);
        rb.MovePosition(targetPos);
    }

    void OnDrawGizmos()
    {
        Vector3 start = Application.isPlaying ? startPos : transform.TransformPoint(startOffset);
        Vector3 end = Application.isPlaying ? endPos : transform.TransformPoint(endOffset);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(start, 0.8f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(end, 0.8f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, end);
    }
}
