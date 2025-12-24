using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource musicSource;

    public GameObject onText;   // "On"
    public GameObject offText;  // "Off"

    private bool musicOn;
    private float musicVolume;

    void Start()
    {
        // Load saved values
        musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        // Ensure music is playing
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }

        ApplyState();
        ApplyVolume();
    }

    // ===== TOGGLE ON / OFF =====
    public void ToggleMusic()
    {
        musicOn = !musicOn;

        ApplyState();

        PlayerPrefs.SetInt("MusicOn", musicOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyState()
    {
        // UI
        if (onText) onText.SetActive(musicOn);
        if (offText) offText.SetActive(!musicOn);

        // Audio
        if (musicSource != null)
        {
            musicSource.mute = !musicOn;

            // Ensure playback resumes when turned ON
            if (musicOn && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    // ===== VOLUME CONTROL =====
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    void ApplyVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
}
