using UnityEngine;

public class PauseInput : MonoBehaviour
{
    public UIManager uiManager;

    void Update()
    {
        // ❌ Do NOT allow ESC in Main Menu
        if (uiManager.mainMenu.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1f)
            {
                uiManager.ShowPauseMenu();
            }
            else
            {
                uiManager.ResumeGame();
            }
        }
    }
}
