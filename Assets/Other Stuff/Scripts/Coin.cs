using UnityEngine;
using UnityEngine.UI;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public float upDownSpeed = 2f;
    public float upDownAmount = 0.25f;

    [Space]
    public bool onlyPlayerCanPickUp = true;

    private Vector3 startPos;
    private Text coinsText;
    public static int coins = 0;

    void Start()
    {
        coins = 0;

        startPos = transform.position;
        GameObject textObj = GameObject.FindGameObjectWithTag("CoinsText");
        if (textObj != null)
            coinsText = textObj.GetComponent<Text>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * upDownSpeed) * upDownAmount;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (onlyPlayerCanPickUp && !collider.gameObject.CompareTag("Player"))
            return;

        coins++;
        if (coinsText != null)
            coinsText.text = "Coins: " + coins;

        Destroy(gameObject);
        
    }
}
