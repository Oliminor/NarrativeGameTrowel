using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackBars : MonoBehaviour
{
    public static BlackBars instance;

    Animator anim;

    void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (BlackBars)");
        instance = this;

        anim = GetComponent<Animator>();
    }

    public void BlackBarOn()
    {
        anim.SetTrigger("BlackBarOn");
    }

    public void BlackBarOff()
    {
        anim.SetTrigger("BlackBarOff");
    }
}
