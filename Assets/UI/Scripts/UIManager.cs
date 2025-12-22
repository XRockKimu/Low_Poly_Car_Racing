using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject optionsMenu;

    // remembers where Options was opened from
    private bool optionsFromPause = false;

    void Start()
    {
        // Game always starts paused at Main Menu
        Time.timeScale = 0f;
        ShowMainMenu();
    }

    // ================= MAIN MENU =================

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        // ENTER GAME (NO RESET)
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);

        Time.timeScale = 1f;
    }

    // ================= PAUSE MENU =================

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

    // ================= OPTIONS =================

    public void ShowOptionsMenu()
    {
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
            ShowPauseMenu();
        }
        else
        {
            ShowMainMenu();
        }
    }

    // ================= QUIT =================

    public void QuitGame()
    {
        // FULL RESET
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
