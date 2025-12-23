using UnityEngine;
using TMPro;

public class TimerSettings : MonoBehaviour
{
    public TMP_Dropdown timerDropdown;

    void Start()
    {
        // Default = 10 minutes
        int savedMinutes = PlayerPrefs.GetInt("GameTimer", 10);

        // Safety: prevent invalid saved value
        savedMinutes = Mathf.Max(1, savedMinutes);

        for (int i = 0; i < timerDropdown.options.Count; i++)
        {
            if (timerDropdown.options[i].text == savedMinutes.ToString())
            {
                timerDropdown.value = i;
                break;
            }
        }
    }

    public void OnTimerChanged()
    {
        int selectedMinutes =
            int.Parse(timerDropdown.options[timerDropdown.value].text);

        // Safety: never allow 0
        selectedMinutes = Mathf.Max(1, selectedMinutes);

        PlayerPrefs.SetInt("GameTimer", selectedMinutes);
        PlayerPrefs.Save();

        Debug.Log($"[TimerSettings] Time limit set to {selectedMinutes} minutes");
    }
}
