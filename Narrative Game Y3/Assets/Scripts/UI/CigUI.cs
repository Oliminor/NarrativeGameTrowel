using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CigUI : MonoBehaviour, IMouseEnter, IMouseExit
{
    public void MouseEnter()
    {
        if (!GameManager.instance.ReadyToContinue()) return;

        HUDManager.instance.ShowAndSetCigUI(GameManager.instance.GetRemainingCigarettes(), transform.position);
    }

    public void MouseExit()
    {
        HUDManager.instance.DisableCigUI();
    }
}
