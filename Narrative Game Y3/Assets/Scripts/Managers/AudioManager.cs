//  --------------------------------------  References  --------------------------------------
//
//  Audio Manager - Introduction to AUDIO in Unity - Brackeys - https://www.youtube.com/watch?v=6OT43pvUyfY
//
//  --------------------------------------  References  --------------------------------------
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [HideInInspector]
    public Sound[] sounds;

    private void Awake()
    {
        #region AudioManager Instance
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        #endregion
        foreach (Sound s in sounds) SoundSetUp(s);
    }

    public void SoundSetUp(Sound s)
    {
        s.source = gameObject.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.outputAudioMixerGroup = s.outputAudioMixerGroup;
        if (s.Default)
        {
            s.source.volume = 0.1f;                       //  Sets Volume on AudioSource to the default value on the Sound Class
            s.source.pitch = 1f;                        //  Sets Pitch on AudioSource to the default value on the Sound Class
            s.source.spatialBlend = 0f;                 //  Sets Spatial Blend (2D(0) or 3D(1) Sound) on AudioSource to the default value on the Sound Class
        }
        else
        {
            s.source.volume = s.volume;                 //  Sets Volume on AudioSource to volume on the Sound Class
            s.source.pitch = s.pitch;                   //  Sets Pitch on AudioSource to pitch on the Sound Class
            s.source.spatialBlend = s.spacialBlend;     //  Sets Spatial Blend (2D(0) or 3D(1) Sound) on AudioSource to spacialBlend on the Sound Class
        }
        s.source.loop = s.loop;
    }


    #region Play/Stop Audio
    public void PlayAudio(string name = null)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (name == null)
        {
            Debug.LogError("name string is null, unable to search for Sound Class in 'sounds' array!");
            return;
        }
        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void StopAudio(string name = null)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (name == null)
        {
            Debug.LogError("'name' string is null, unable to search for Sound Class in 'sounds' array!");
            return;
        }
        if (s == null)
        {
            Debug.LogError("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }
    #endregion
}