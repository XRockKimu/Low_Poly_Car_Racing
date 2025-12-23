using UnityEngine;
using TMPro;

public class TimerSettings : MonoBehaviour
{
    public TMP_Dropdown timerDropdown;
    public GameTimer gameTimer;

    void Start()
    {
        int savedTime = PlayerPrefs.GetInt("GameTimer", 10);

        for (int i = 0; i < timerDropdown.options.Count; i++)
        {
            if (timerDropdown.options[i].text == savedTime.ToString())
            {
                timerDropdown.value = i;
                break;
            }
        }
    }

    public void OnTimerChanged()
    {
        int selectedTime =
            int.Parse(timerDropdown.options[timerDropdown.value].text);

        PlayerPrefs.SetInt("GameTimer", selectedTime);
        PlayerPrefs.Save();

        // 🔥 Apply immediately if game is running
        if (gameTimer != null)
        {
            gameTimer.StartTimer();
        }
    }
}
