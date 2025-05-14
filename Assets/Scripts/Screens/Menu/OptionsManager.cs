using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [Header("Sliders")]
    public Slider volumeSlider;
    public Slider brightnessSlider;

    [Header("Brilho")]
    public Image brightnessOverlay; 

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        float brightness = PlayerPrefs.GetFloat("Brightness", 1f);

        volumeSlider.value = volume;
        brightnessSlider.value = brightness;

        ApplyVolume(volume);
        ApplyBrightness(brightness);
    }

    public void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat("Volume", value);
    }

    public void OnBrightnessChanged(float value)
    {
        ApplyBrightness(value);
        PlayerPrefs.SetFloat("Brightness", value);
    }

    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void ApplyBrightness(float value)
    {
        if (brightnessOverlay != null)
        {
            Color color = brightnessOverlay.color;

            float maxDarkness = 0.85f;
            color.a = Mathf.Lerp(0f, maxDarkness, 1f - value);

            brightnessOverlay.color = color;
        }
    }

}
