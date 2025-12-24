using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("Warning")]
    public float warningTime = 5f;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    [Header("Pulse Effect")]
    public float pulseSpeed = 5f;
    public float pulseScale = 1.2f;

    [Header("References")]
    public UIManager uiManager;

    private float elapsedTime;
    private float timeLimit;
    private bool isRunning;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = timerText.transform.localScale;
    }

    void Update()
    {
        if (!isRunning)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= timeLimit)
        {
            elapsedTime = timeLimit;
            isRunning = false;
            timerText.transform.localScale = originalScale;
            OnTimeUp();
        }

        UpdateUI();

        if (timeLimit - elapsedTime <= warningTime)
            PulseEffect();
        else
            timerText.transform.localScale = originalScale;
    }

    public void StartTimer()
    {
        elapsedTime = 0f;

        int minutes = PlayerPrefs.GetInt("GameTimer", 10);

        // 🔒 CRITICAL SAFETY FIX (prevents instant game over)
        minutes = Mathf.Max(1, minutes);

        timeLimit = minutes * 60f; // minutes → seconds

        isRunning = true;
        UpdateUI();

        Debug.Log($"[GameTimer] Started: {minutes} min ({timeLimit}s)");
    }

    public void StopTimer()
    {
        isRunning = false;
        timerText.transform.localScale = originalScale;
    }

    void UpdateUI()
    {
        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
        timerText.color =
            (timeLimit - elapsedTime <= warningTime)
                ? warningColor
                : normalColor;
    }

    void PulseEffect()
    {
        float scale =
            1 + Mathf.Sin(Time.unscaledTime * pulseSpeed) * (pulseScale - 1);

        timerText.transform.localScale = originalScale * scale;
    }

void OnTimeUp()
{
    Debug.Log("[GameTimer] TIME UP");
    SoundManager.Instance.PlayGameOver();
    uiManager.ShowGameOver();
}

}
