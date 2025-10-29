using UnityEngine;
using UnityEngine.Events;

public class ActionOnCollision : MonoBehaviour
{
    public UnityEvent EventToFire;
    public LayerMask acceptedLayers = ~0;

    [Header("Visual Feedback")]
    public Material TurnedOnMaterial;
    public Material TurnedOffMaterial;

    private MeshRenderer meshRenderer;
    private int collisionCount = 0;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateMaterial();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & acceptedLayers) != 0)
        {
            collisionCount++;
            EventToFire?.Invoke();
            UpdateMaterial();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & acceptedLayers) != 0)
        {
            collisionCount = Mathf.Max(0, collisionCount - 1);
            UpdateMaterial();
        }
    }

    void UpdateMaterial()
    {
        if (meshRenderer == null) return;

        if (collisionCount > 0 && TurnedOnMaterial != null)
            meshRenderer.material = TurnedOnMaterial;
        else if (collisionCount <= 0 && TurnedOffMaterial != null)
            meshRenderer.material = TurnedOffMaterial;
    }
}
