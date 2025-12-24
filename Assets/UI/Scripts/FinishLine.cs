using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public UIManager uiManager;
    public GameTimer gameTimer;

    private bool finished = false;
    private Collider finishCollider;

    void Awake()
    {
        finishCollider = GetComponent<Collider>();
        finishCollider.enabled = true; // 🚫 disabled at start
    }

    // ✅ Called when race actually begins
    public void EnableFinishLine()
    {
        finished = false;
        finishCollider.enabled = true;
        Debug.Log("FinishLine ENABLED");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!finishCollider.enabled)
            return;

        if (finished)
            return;

        if (!other.CompareTag("Player"))
            return;

        finished = true;

        Debug.Log("🏁 FINISH LINE REACHED - YOU WIN!");

        if (gameTimer != null)
            gameTimer.StopTimer();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayWin();

        if (uiManager != null)
            uiManager.ShowYouWon();;
    }
}
