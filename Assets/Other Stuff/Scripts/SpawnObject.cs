using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public GameObject prefabToSpawn;

    public void Spawn()
    {
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning($"{name}: No prefab assigned to spawn.");
        }
    }
}
