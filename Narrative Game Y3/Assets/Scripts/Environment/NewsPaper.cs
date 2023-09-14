using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewsPaper : MonoBehaviour
{
    public static NewsPaper instance;

    Animator anim;

    void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (NewsPaper)");
        instance = this;

        anim = GetComponent<Animator>();
    }

   public void PickUpNewspaper()
    {
        anim.SetTrigger("PickUp");
    }

    public void PutDownNewspaper()
    {
        anim.SetTrigger("PutDown");
    }

    public bool IsAnimationOver()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsTag("NewspapaerAnimation");
    }
}
