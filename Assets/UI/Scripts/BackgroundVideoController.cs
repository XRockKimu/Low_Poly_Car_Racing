using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideoController : MonoBehaviour
{
    public VideoPlayer menuVideoPlayer;

    public GameObject videoText;   // "Video"
    public GameObject imageText;   // "Image"

    private bool videoPaused;

    void Start()
    {
        videoPaused = PlayerPrefs.GetInt("VideoPaused", 0) == 1;
        ApplyState();
    }

    public void ToggleVideo()
    {
        videoPaused = !videoPaused;
        ApplyState();

        PlayerPrefs.SetInt("VideoPaused", videoPaused ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyState()
    {
        if (videoPaused)
        {
            menuVideoPlayer.Pause();   // ⏸ Pause video
            videoText.SetActive(false);
            imageText.SetActive(true);
        }
        else
        {
            menuVideoPlayer.Play();    // ▶ Play video
            videoText.SetActive(true);
            imageText.SetActive(false);
        }
    }
}
