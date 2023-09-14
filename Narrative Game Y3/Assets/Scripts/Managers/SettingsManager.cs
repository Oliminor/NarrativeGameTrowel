using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[System.Serializable]
public class VolumeClass
{
    public string playerPrefString;
    public Slider slider;
}

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public VolumeClass[] volumeClass = new VolumeClass[4];

    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;

    private float currentRefreshRate;
    [SerializeField] private int currentResolutionIndex = 0;

    [SerializeField] Toggle fullscreen;
    public bool isFullScreen = true;

    [SerializeField] private GameObject settingsUI;

    private void Start()
    {
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        resolutionDropdown.ClearOptions();
        currentRefreshRate = Screen.currentResolution.refreshRate;

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].refreshRate == currentRefreshRate) filteredResolutions.Add(resolutions[i]);
        }
        
        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string resolutionOption = filteredResolutions[i].width + " x " + filteredResolutions[i].height;
            options.Add(resolutionOption);
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        SoundSettingsStart();

        settingsUI = this.gameObject;
    }

    public void BackButton()
    {

        AudioManager.instance.PlayAudio("Switch Click 1");
        settingsUI.SetActive(false);
    }

    public void FullscreenToggle(bool toggleOn)
    {
        AudioManager.instance.PlayAudio("Switch Click 1");
        isFullScreen = toggleOn;
        SetResolution(resolutionDropdown.value);
    }

    public void SetResolution(int resIndex)
    {
        Resolution res = filteredResolutions[resIndex];
        Screen.SetResolution(res.width, res.height, isFullScreen);
    }

    #region Volume Settings
    private void SoundSettingsStart()
    {
        if (PlayerPrefs.HasKey(volumeClass[0].playerPrefString) && volumeClass[0].slider.value != PlayerPrefs.GetFloat(volumeClass[0].playerPrefString))
            volumeClass[0].slider.value = PlayerPrefs.GetFloat(volumeClass[0].playerPrefString);
        if (!PlayerPrefs.HasKey(volumeClass[0].playerPrefString)) 
            volumeClass[0].slider.value = 1;

        if (PlayerPrefs.HasKey(volumeClass[1].playerPrefString) && volumeClass[1].slider.value != PlayerPrefs.GetFloat(volumeClass[1].playerPrefString))
            volumeClass[1].slider.value = PlayerPrefs.GetFloat(volumeClass[1].playerPrefString);
        if (!PlayerPrefs.HasKey(volumeClass[1].playerPrefString))
            volumeClass[1].slider.value = 1;

        if (PlayerPrefs.HasKey(volumeClass[2].playerPrefString) && volumeClass[2].slider.value != PlayerPrefs.GetFloat(volumeClass[2].playerPrefString))
            volumeClass[2].slider.value = PlayerPrefs.GetFloat(volumeClass[2].playerPrefString);
        if (!PlayerPrefs.HasKey(volumeClass[2].playerPrefString)) 
            volumeClass[2].slider.value = 1;

        if (PlayerPrefs.HasKey(volumeClass[3].playerPrefString) && volumeClass[3].slider.value != PlayerPrefs.GetFloat(volumeClass[3].playerPrefString))
            volumeClass[3].slider.value = PlayerPrefs.GetFloat(volumeClass[3].playerPrefString);
        if (!PlayerPrefs.HasKey(volumeClass[3].playerPrefString))
            volumeClass[3].slider.value = 1;
    }


    public void SetMasterVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterAM", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat(volumeClass[0].playerPrefString, sliderValue);
        Debug.Log(PlayerPrefs.GetFloat(volumeClass[0].playerPrefString));
    }

    public void SetMusicVolume(float sliderValue)
    {
        audioMixer.SetFloat("MusicAM", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat(volumeClass[1].playerPrefString, sliderValue);
        Debug.Log(PlayerPrefs.GetFloat(volumeClass[1].playerPrefString));
    }

    public void SetAmbientVolume(float sliderValue)
    {
        audioMixer.SetFloat("AmbientAM", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat(volumeClass[2].playerPrefString, sliderValue);
        Debug.Log(PlayerPrefs.GetFloat(volumeClass[2].playerPrefString));
    }

    public void SetDialogueVolume(float sliderValue)
    {
        audioMixer.SetFloat("DialogueAM", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat(volumeClass[3].playerPrefString, sliderValue);
        Debug.Log(PlayerPrefs.GetFloat(volumeClass[3].playerPrefString));
    }
    #endregion
}
