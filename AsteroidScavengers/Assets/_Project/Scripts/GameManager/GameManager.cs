using UnityEngine;
using TMPro; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    [Header("Game Rules")]
    public int currentMoney = 0;
    public int quotaAmount = 300; 
    public float timeLimit = 300f; 

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI quotaText;
    [SerializeField] private TextMeshProUGUI timerText;

    private float currentTime;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        currentTime = timeLimit;
        UpdateUI();
    }

    void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            EndGame(false);
        }
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();

        if (currentMoney >= quotaAmount)
        {
            EndGame(true);
        }
    }

    void UpdateUI()
    {
        if (moneyText != null) moneyText.text = $"$$$: {currentMoney}";
        if (quotaText != null) quotaText.text = $"Цель: {quotaAmount}";

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        if (timerText != null) timerText.text = $"Время: {minutes:D2}:{seconds:D2}";
    }

    void EndGame(bool win)
    {
        isGameOver = true;
        Time.timeScale = 0; 
        Debug.Log(win ? "КВОТА ВЫПОЛНЕНА!" : "ВРЕМЯ ВЫШЛО...");
    }
}