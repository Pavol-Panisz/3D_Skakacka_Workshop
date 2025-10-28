using UnityEngine;
using UnityEngine.Events;

public class ActionOnCollision : MonoBehaviour
{
    public UnityEvent EventToFire;
    public LayerMask acceptedLayers = ~0;

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & acceptedLayers) != 0)
        {
            EventToFire?.Invoke();
        }
    }
}
