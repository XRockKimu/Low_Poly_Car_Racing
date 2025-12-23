using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownController : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public GameTimer gameTimer;

    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        Time.timeScale = 0f;
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "2";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "1";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(0.7f);

        countdownText.gameObject.SetActive(false);

        Time.timeScale = 1f;
        gameTimer.StartTimer(); // ← YOUR ORIGINAL TIMER START
    }
}
