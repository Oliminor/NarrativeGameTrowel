using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public string soundName, soundname2;

    public void PlaySound()
    {
        AudioManager.instance.PlayAudio(soundName);
        AudioManager.instance.PlayAudio(soundname2);
    }

    public void StopSound()
    {
        AudioManager.instance.StopAudio(soundName);
        AudioManager.instance.StopAudio(soundname2);
    }
}
