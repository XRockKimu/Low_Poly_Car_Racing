using UnityEngine;
using UnityEngine.UI;

public class VolumeUISync : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
    }
}
