using UnityEngine;
using System.Collections;

public class FinishLine : MonoBehaviour
{
    [Header("References")]
    public UIManager uiManager;
    public GameTimer gameTimer;

    [Header("Settings")]
    public float enableDelay = 10f; // ⏱ Time before finish line activates

    private bool finished = false;
    private Collider finishCollider;

    void Awake()
    {
        finishCollider = GetComponent<Collider>();

        if (finishCollider == null)
        {
            Debug.LogError("❌ FinishLine: No Collider found!");
            return;
        }

        finishCollider.enabled = false; // ❌ disabled at start
    }

    void Start()
    {
        StartCoroutine(EnableFinishLineAfterDelay(enableDelay));
    }

    IEnumerator EnableFinishLineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        finished = false;
        finishCollider.enabled = true;

        Debug.Log("🏁 Finish Line ENABLED after " + delay + " seconds");
    }

    // Optional: Call this if you restart the race
    public void EnableFinishLine()
    {
        StopAllCoroutines();
        StartCoroutine(EnableFinishLineAfterDelay(enableDelay));
    }

    void OnTriggerEnter(Collider other)
    {
        if (!finishCollider.enabled) return;
        if (finished) return;
        if (!other.CompareTag("Player")) return;

        finished = true;

        Debug.Log("🏁 FINISH LINE REACHED - YOU WIN!");

        if (gameTimer != null)
            gameTimer.StopTimer();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayWin();

        if (uiManager != null)
            uiManager.ShowYouWon();
    }
}
