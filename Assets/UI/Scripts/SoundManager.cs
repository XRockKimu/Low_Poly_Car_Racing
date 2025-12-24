using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Countdown Sounds")]
    public AudioClip tickSound;   // 3,2,1
    public AudioClip goSound;     // GO!

    [Header("Game State Sounds")]
    public AudioClip winSound;
    public AudioClip gameOverSound;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    // ---------- PLAY METHODS ----------

    public void PlayTick()
    {
        audioSource.PlayOneShot(tickSound);
    }

    public void PlayGo()
    {
        audioSource.PlayOneShot(goSound);
    }

    public void PlayWin()
    {
        audioSource.PlayOneShot(winSound);
    }

    public void PlayGameOver()
    {
        audioSource.PlayOneShot(gameOverSound);
    }
}
