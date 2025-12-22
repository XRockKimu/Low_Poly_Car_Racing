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
    if (menuVideoPlayer == null)
        return;

    if (videoPaused)
    {
        if (menuVideoPlayer.isPlaying)
            menuVideoPlayer.Pause();

        videoText.SetActive(false);
        imageText.SetActive(true);
    }
    else
    {
        // 🔒 Safety: only play when ready
        if (!menuVideoPlayer.isPrepared)
        {
            menuVideoPlayer.Prepare();
            menuVideoPlayer.prepareCompleted += OnVideoPrepared;
        }
        else
        {
            menuVideoPlayer.Play();
        }

        videoText.SetActive(true);
        imageText.SetActive(false);
    }
}

void OnVideoPrepared(VideoPlayer vp)
{
    vp.prepareCompleted -= OnVideoPrepared;
    vp.Play();
}

}
