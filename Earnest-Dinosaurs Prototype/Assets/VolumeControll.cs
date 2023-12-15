using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControll : MonoBehaviour
{
    [SerializeField] string volumeParameter;
    [SerializeField] AudioMixer mixer;
    [SerializeField] Toggle toggle;
    [SerializeField] Slider slider;
    public float multiplier = 30f; // if this changes, must change for GameManager in Awake() function
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] clips;

    private bool disableToggle = false;
    private bool disableSlider = false;

    private void Awake()
    {
        slider.onValueChanged.AddListener(SliderValueChange);
        toggle.onValueChanged.AddListener(ToggleValueChange);
    }

    void Start()
    {
        disableSlider = true;
        disableToggle = true;

        slider.value = PlayerPrefs.GetFloat(volumeParameter, slider.value);
        toggle.isOn = slider.value <= slider.minValue;

        disableToggle = false;
        disableSlider = false;
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(volumeParameter, slider.value);
    }

    public void SliderValueChange(float value)
    {
        if (disableSlider) { return; }

        mixer.SetFloat(volumeParameter, Mathf.Log10(value) * multiplier);
        disableToggle = true;
        toggle.isOn = slider.value <= slider.minValue;
        disableToggle = false;
    }

    public void PlayRandomNoise()
    {
        if(aud != null && !aud.isPlaying && clips.Length > 0)
        {
            aud.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }

    public void ToggleValueChange(bool Muted)
    {
        if (disableToggle) { return; }

        if(!Muted)
        {
            disableSlider = true;
            slider.value = .8f;
            mixer.SetFloat(volumeParameter, Mathf.Log10(slider.value) * multiplier);
            disableSlider = false;
        }
        else
        {
            disableSlider = true;
            slider.value = slider.minValue;
            mixer.SetFloat(volumeParameter, Mathf.Log10(slider.value) * multiplier);
            disableSlider = false;
        }
    }

    public float GetMultiplier()
    {
        return multiplier;
    }
}
