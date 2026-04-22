using UnityEngine;

public class WeightSystem : MonoBehaviour
{
    [Header("Weight Settings")]
    [SerializeField] private float maxCarryWeight = 30f;
    [SerializeField] private float speedPenaltyPerKg = 0.015f;

    private float currentWeight = 0f;
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    public bool AddWeight(float weight)
    {
        if (currentWeight + weight <= maxCarryWeight)
        {
            currentWeight += weight;
            UpdateSpeed();
            return true;
        }
        return false;
    }

    public void SellAll()
    {
        int moneyEarned = Mathf.RoundToInt(currentWeight * 5);

        if (moneyEarned > 0)
        {
            GameManager.Instance.AddMoney(moneyEarned);
            currentWeight = 0; 
            UpdateSpeed();
            Debug.Log($"Продано на {moneyEarned} монет!");
        }
    }

    public void RemoveWeight(float weight)
    {
        currentWeight = Mathf.Max(0, currentWeight - weight);
        UpdateSpeed();
    }

    void UpdateSpeed()
    {
        float speedMultiplier = Mathf.Max(0.1f, 1f - (currentWeight * speedPenaltyPerKg));
        playerController.SetSpeedMultiplier(speedMultiplier);

        Debug.Log($"Вес: {currentWeight:F1}/{maxCarryWeight} кг | Скорость: {speedMultiplier * 100:F0}%");
    }

    public float GetCurrentWeight() => currentWeight;
    public float GetMaxWeight() => maxCarryWeight;
    public float GetRemainingCapacity() => maxCarryWeight - currentWeight;
}