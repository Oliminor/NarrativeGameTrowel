using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BlackSmokeScreen : MonoBehaviour
{
    public static BlackSmokeScreen instance;

    [SerializeField] private GameObject smokeEffect;

    [Header("Smoke Sound")]
    [SerializeField] AudioSource smokeAudioSource;
    [SerializeField] AudioClip smokeAudio;
    [SerializeField] float playbackVolume = 1f;

    private Animator anim;

    void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (BlackSmokeScreen)");
        instance = this;

        anim = GetComponent<Animator>();
        gameObject.GetComponent<Image>().enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);
    }

    public void TriggerBlackScreen()
    {
        anim.SetTrigger("SmokeScreen");
    }
    public void TriggerGameOver()
    {
        anim.SetTrigger("GameOver");
    }

    public void TriggerRestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void TriggerSmokeEffect()
    {
        StartCoroutine(TriggerSmoke());
    }

    IEnumerator TriggerSmoke()
    {
        smokeEffect.gameObject.SetActive(true);
        smokeAudioSource.PlayOneShot(smokeAudio);
        yield return new WaitForSeconds(5);
        smokeEffect.gameObject.SetActive(false);
    }
}
