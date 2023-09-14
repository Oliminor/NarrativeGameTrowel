using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///  Interfaces are stored here
/// </summary>

public interface IObjectInteraction // Trigger this when the player interact with an interactable object
{
    void Interact();
    bool isObjectActive();
}

public interface IMouseEnter
{
    void MouseEnter();
}

public interface IMouseExit // Outline when the mouse is on an interactable object
{
    void MouseExit();
}
