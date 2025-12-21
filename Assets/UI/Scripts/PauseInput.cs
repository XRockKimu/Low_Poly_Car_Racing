using UnityEngine;

public class PauseInput : MonoBehaviour
{
    public UIManager uiManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1f)
            {
                uiManager.ShowPauseMenu();   // pause game
            }
            else
            {
                uiManager.ResumeGame();     // resume game
            }
        }
    }
}
