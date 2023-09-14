using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeckInteract : MonoBehaviour, IObjectInteraction, IMouseEnter, IMouseExit
{

    [SerializeField] private CardType deckType;

    public void Interact()
    {
        if (!GameManager.instance.ReadyToContinue()) return;

        ToggleOutlineOff();

        PlayingCardManager.instance.InteractWithDeck(deckType);
    }

    public void MouseEnter()
    {
        if (transform.childCount == 0) return;

        if (!GameManager.instance.ReadyToContinue()) return;

        ToggleOutlineOn();
    }

    public void MouseExit()
    {
        ToggleOutlineOff();
    }

    private void ToggleOutlineOn()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out Outline _childOutline))
            {
                _childOutline.enabled = true;
                _childOutline.SetToggle(true);
                _childOutline.SetOutlineWidth(GlobalOutlineManager.instance.GetOutlineWIdth());
            }
        }
    }

    private void ToggleOutlineOff()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out Outline _childOutline))
            {
                _childOutline.enabled = false;
                _childOutline.SetToggle(false);
            }
        }
    }

    public bool isObjectActive()
    {
        return transform.childCount != 0;
    }
}
