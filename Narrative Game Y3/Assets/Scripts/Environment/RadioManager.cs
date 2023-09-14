using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioManager : MonoBehaviour, IObjectInteraction
{
    [SerializeField] AudioClip[] AvailableSongs;
    [SerializeField] AudioSource radioAudioSource;
    [SerializeField] AudioClip interactSound;
    int songIndex = 0;
    int maxSongindex;
    bool isOn = true;

    Animator animator;

    private void Start() //Get animator & find max song list index
    {
        animator = GetComponent<Animator>();
        maxSongindex = AvailableSongs.Length;
    }

    private void Update() //Cycle song when finished & radio still active
    {
        if (isOn && !radioAudioSource.isPlaying && Application.isFocused) CycleSong();
    }

    public void Interact() //Click in scene to toggle radio on/off, cycles song when turning on
    {
        isOn = !isOn;

        if(isOn)
        {
            CycleSong();
        }
        else
        {
            radioAudioSource.Stop();
            animator.enabled = false;
        }
        radioAudioSource.PlayOneShot(interactSound, 0.1f);
    }

    private void CycleSong() //Cycle song according to elements in the song list array
    {
        songIndex += 1;
        songIndex %= maxSongindex;

        animator.enabled = true;
        radioAudioSource.clip = AvailableSongs[songIndex];
        radioAudioSource.Play();
    }

    public bool isObjectActive()
    {
        return true;
    }
}
