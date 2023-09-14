//  --------------------------------------  References  --------------------------------------
//
//  Audio Manager - Introduction to AUDIO in Unity - Brackeys - https://www.youtube.com/watch?v=6OT43pvUyfY
//
//  --------------------------------------  References  --------------------------------------
using System;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public AudioMixerGroup outputAudioMixerGroup;

    public bool Default;
    public bool loop;

    [Range(0f, 1f), HideInInspector]
    public float volume = 1f;
    [Range(0.1f, 3f), HideInInspector]
    public float pitch = 1f;
    [Range(0f, 1f), HideInInspector]
    public float spacialBlend = 0f;


    [HideInInspector]
    public AudioSource source;
}