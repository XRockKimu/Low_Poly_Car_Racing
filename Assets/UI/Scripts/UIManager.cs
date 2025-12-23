using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject optionsMenu;
    public GameObject gameplayHUD;
    public GameObject gameOverMenu;

    [Header("References")]
    public GameTimer gameTimer;

    private bool optionsFromPause = false;
    public static bool IsRestarting = false;

void Start()
{
    if (IsRestarting)
    {
        IsRestarting = false;

        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        gameplayHUD.SetActive(true);

        Time.timeScale = 1f;
        gameTimer.StartTimer();
    }
    else
    {
        Time.timeScale = 0f;
        ShowMainMenu();
    }
}


    // ================= MAIN MENU =================

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        gameplayHUD.SetActive(false);
        gameOverMenu.SetActive(false);

        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        gameplayHUD.SetActive(true);

        Time.timeScale = 1f;
        gameTimer.StartTimer();
    }

    // ================= PAUSE =================

    public void ShowPauseMenu()
    {
        pauseMenu.SetActive(true);
        gameplayHUD.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        gameplayHUD.SetActive(true);
        Time.timeScale = 1f;
    }

    // ================= OPTIONS =================

    public void ShowOptionsMenu()
    {
        optionsFromPause = pauseMenu.activeSelf;

        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        gameplayHUD.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void BackFromOptions()
    {
        optionsMenu.SetActive(false);

        if (optionsFromPause)
            ShowPauseMenu();
        else
            ShowMainMenu();
    }
    // ================= Back =================
    public void BackToMainMenu()
    {
        IsRestarting = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ================= GAME OVER =================

    public void ShowGameOver()
    {
        gameplayHUD.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        gameOverMenu.SetActive(true);

        Time.timeScale = 0f;
    }

    // ================= BUTTON ACTIONS =================

    public void RestartGame()
    {
        IsRestarting = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // QUIT = BACK TO START MENU
    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    // Close = BACK TO START MENU
    public void CloseGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Close();
        #endif
    }
     // Save Settings
    public void Save()
    {
        IsRestarting = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
