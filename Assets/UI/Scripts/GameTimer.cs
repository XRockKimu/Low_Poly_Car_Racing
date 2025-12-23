using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("Warning Settings")]
    public float warningTime = 3f;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    [Header("Pulse Effect")]
    public float pulseSpeed = 5f;
    public float pulseScale = 1.2f;

    [Header("References")]
    public UIManager uiManager;

    private float timeRemaining;
    private bool isRunning = false;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = timerText.transform.localScale;
    }

    void Update()
    {
        if (!isRunning)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            timerText.transform.localScale = originalScale;
            OnTimeUp();
        }

        UpdateUI();

        if (timeRemaining <= warningTime)
            PulseEffect();
        else
            timerText.transform.localScale = originalScale;
    }

    public void StartTimer()
    {
        timeRemaining = PlayerPrefs.GetInt("GameTimer", 10);
        isRunning = true;
        UpdateUI();
    }

    public void StopTimer()
    {
        isRunning = false;
        timerText.transform.localScale = originalScale;
    }

    void UpdateUI()
    {
        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.color = timeRemaining <= warningTime ? warningColor : normalColor;
    }

    void PulseEffect()
    {
        float scale =
            1 + Mathf.Sin(Time.unscaledTime * pulseSpeed) * (pulseScale - 1);

        timerText.transform.localScale = originalScale * scale;
    }

    void OnTimeUp()
    {
        Debug.Log("TIME UP");
        uiManager.ShowGameOver();
    }
}
