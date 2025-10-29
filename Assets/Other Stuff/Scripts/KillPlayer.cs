using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    public static Vector3 playerRespawnPoint;
    private Transform playerTransform;
    private Rigidbody playerRigidbody;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerRespawnPoint = playerTransform.position;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerTransform != null)
        {
            playerTransform.position = playerRespawnPoint;
            playerRigidbody.velocity = Vector3.zero;
        }
    }
}
