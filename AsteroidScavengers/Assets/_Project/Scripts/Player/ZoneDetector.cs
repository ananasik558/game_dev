using UnityEngine;

public class ZoneDetector : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SellZone"))
        {
            playerController.isInSellZone = true;
            Debug.Log("Зашли на платформу продажи!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SellZone"))
        {
            playerController.isInSellZone = false;
            Debug.Log("Покинули платформу продажи.");
        }
    }
}