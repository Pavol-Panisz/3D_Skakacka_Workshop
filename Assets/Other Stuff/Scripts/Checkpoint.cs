using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Checkpoint : MonoBehaviour
{
    public float spawnOffset = 3f;
    public UnityEvent OnThisSetAsRespawn;
    public UnityEvent OnOtherSetAsRespawn;

    Checkpoint[] allCheckpoints;

    private void Start()
    {
        allCheckpoints = GameObject.FindObjectsOfType<Checkpoint>();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            KillPlayer.playerRespawnPoint = transform.position + Vector3.up * spawnOffset;
            
            foreach (var checkpoint in allCheckpoints)
            {
                if (checkpoint == this)
                {
                    OnThisSetAsRespawn?.Invoke();
                }
                else
                {
                    checkpoint.OnOtherSetAsRespawn?.Invoke();
                }
            }

        }
    }
}
