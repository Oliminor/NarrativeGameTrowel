using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telephone : MonoBehaviour
{
    public static Telephone instance;

    private Animator anim;

    private bool isPhonePickedUp;
    void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (Telephone)");
        instance = this;

        anim = GetComponent<Animator>();
    }

    public void PickUpPhone()
    {
        isPhonePickedUp = true;
        anim.SetTrigger("PickUp");
    }

    public void PutDownpPhone()
    {
        if (isPhonePickedUp)
        {
            anim.SetTrigger("PutDown");
            isPhonePickedUp = false;
        }
    }

    public bool IsAnimationOver()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsTag("PhoneAnimation");
    }
}
