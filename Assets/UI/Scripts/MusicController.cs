using UnityEngine;

public class MusicController : MonoBehaviour
{
    // Optional – can be empty for now
    public AudioSource musicSource;

    public GameObject onText;   // "On"
    public GameObject offText;  // "Off"

    private bool musicOn;

    void Start()
    {
        musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        ApplyState();
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        ApplyState();

        PlayerPrefs.SetInt("MusicOn", musicOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyState()
    {
        // UI only (always works)
        onText.SetActive(musicOn);
        offText.SetActive(!musicOn);

        // Audio only if it exists (safe)
        if (musicSource != null)
        {
            musicSource.mute = !musicOn;
        }
    }
}
