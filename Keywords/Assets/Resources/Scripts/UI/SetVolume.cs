using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SetVolume : MonoBehaviour {
    public AudioMixer mixer;
    private Slider slider;
    public string paramName;
    private void Start() {
        slider = gameObject.GetComponent<Slider>();
        float sliderValue = PlayerPrefs.GetFloat(paramName, slider.maxValue);
        slider.value = sliderValue;
        SetLevel(sliderValue);
    }
    public void SetLevel(float sliderValue) {
        PlayerPrefs.SetFloat(paramName, sliderValue);
        mixer.SetFloat(paramName, Mathf.Log10(sliderValue) * 20);
    }
}
