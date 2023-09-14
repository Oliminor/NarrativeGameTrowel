using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable] public class MainMenuObjects
{
    public GameObject canvasOBJ;
    public Button startButton, settingsButton, quitButton;
}
[System.Serializable] public class SettingsObjects
{
    public GameObject canvasOBJ;
    public Button backButton;
    public Slider masterVolumeSlider, musicVolumeSlider, AmbientVolumeSlider, dialogueVolumeSlider;
}
[System.Serializable] public class GameHUDObjects
{
    public GameObject canvasOBJ;
    public Button pauseButton;
}
[System.Serializable] public class PauseMenuObjects
{
    public GameObject canvasOBJ;
    public Button resumeButton, settingsButton, quitToMenuButton;
}

public class UIManager : MonoBehaviour
{
    bool hasGameStart = false, paused = false;

    public MainMenuObjects mainMenuObjects;
    public SettingsObjects settingsObjects;
    public GameHUDObjects gameHUDObjects;
    public PauseMenuObjects pauseMenuObjects;

    private void Start()
    {
        hasGameStart = false;
        SettingsMenuUpdate();
    }

    private void Update()
    {
        MainMenuUpdate();
        SettingsMenuUpdate();
    }

    

    public void MainMenuUpdate()
    {
        mainMenuObjects.startButton.onClick.AddListener(StartButton);
        mainMenuObjects.settingsButton.onClick.AddListener(SettingsButton);
        mainMenuObjects.quitButton.onClick.AddListener(QuitButton);
    }

    public void SttingsMenuStart()
    {
        settingsObjects.masterVolumeSlider.minValue = 0.001f;
        settingsObjects.masterVolumeSlider.maxValue = 1;

        settingsObjects.musicVolumeSlider.minValue = 0.001f;
        settingsObjects.musicVolumeSlider.maxValue = 1;
        
        settingsObjects.AmbientVolumeSlider.minValue = 0.001f;
        settingsObjects.AmbientVolumeSlider.maxValue = 1;
        
        settingsObjects.dialogueVolumeSlider.minValue = 0.001f;
        settingsObjects.dialogueVolumeSlider.maxValue = 1;
    }

    public void SettingsMenuUpdate()
    {
        settingsObjects.backButton.onClick.AddListener(BackButton);
    }

    //  ------------------------------ Button Functions ----------------------------------------

    public void StartButton()
    {
        mainMenuObjects.canvasOBJ.SetActive(false);
        hasGameStart = true;
    }
    public void SettingsButton()
    {
        if (!hasGameStart) mainMenuObjects.canvasOBJ.SetActive(false);
        else gameHUDObjects.canvasOBJ.SetActive(false);
        settingsObjects.canvasOBJ.SetActive(true);        
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void QuitToMenuButton()
    {
        SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        gameHUDObjects.canvasOBJ.SetActive(false);
        paused = true;

    }

    public void Resume()
    {
        gameHUDObjects.canvasOBJ.SetActive(true);
        paused = false;
    }

    public void BackButton()
    {
        if (!hasGameStart) mainMenuObjects.canvasOBJ.SetActive(true);
        if (paused) gameHUDObjects.canvasOBJ.SetActive(true);
        settingsObjects.canvasOBJ.SetActive(false);
    }
}