using UnityEngine;
using TMPro;

public class SimpleToggleButton : MonoBehaviour
{
    public TextMeshProUGUI label;
    private bool isOn = false;

    public void Toggle()
    {
        isOn = !isOn;
        label.text = isOn ? "On" : "Off";
    }
}
