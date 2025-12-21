using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject optionsMenu;

    // 🔑 remembers where Options was opened from
    private bool optionsFromPause = false;

    void Start()
    {
        Time.timeScale = 0f;   // pause game on launch
        ShowMainMenu();
    }

    // ===== MAIN MENU =====

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    // ===== PAUSE MENU =====

    public void ShowPauseMenu()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(true);
        optionsMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    // ===== OPTIONS =====

    public void ShowOptionsMenu()
    {
        // remember if Options was opened from Pause
        optionsFromPause = pauseMenu.activeSelf;

        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void BackFromOptions()
    {
        optionsMenu.SetActive(false);

        if (optionsFromPause)
        {
            ShowPauseMenu();   // go back to Pause menu
        }
        else
        {
            ShowMainMenu();    // go back to Main Menu
        }
    }

    // ===== QUIT =====

    public void QuitGame()
    {
        ShowMainMenu();
    }
    
}
