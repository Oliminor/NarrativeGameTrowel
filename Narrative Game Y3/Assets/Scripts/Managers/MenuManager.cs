using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject settingsUI;

    private void Start()
    {
        AudioManager.instance.PlayAudio("Lurking in the Shadows");
    }
    public void StartButton()
    {
        AudioManager.instance.PlayAudio("Switch Click 1");
        AudioManager.instance.StopAudio("Lurking in the Shadows");
        SceneManager.LoadScene(1);
    }

    public void SettingsButton()
    {
        AudioManager.instance.PlayAudio("Switch Click 1");
        settingsUI.SetActive(true);
    }

    public void QuitButton()
    {
        AudioManager.instance.PlayAudio("Switch Click 1");
        Application.Quit();
    }
}
