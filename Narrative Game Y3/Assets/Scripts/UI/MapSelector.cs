using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSelector : MonoBehaviour, IObjectInteraction, IMouseEnter, IMouseExit
{
    [SerializeField] Transform mapPrefab;
    [SerializeField] Transform redCircle;

    private void Start()
    {
        redCircle.gameObject.SetActive(false);
    }

    /// <summary>
    ///  Triggers the diorama animation
    /// </summary>
    public void Interact()
    {
        if (DioramaManager.instance.GetAnimationTransform().GetChild(0) == mapPrefab.transform) return;

        NavigationCamera.instance.ToggleMap();
        DioramaManager.instance.TriggerDioramaAnimation(mapPrefab);
    }

    public bool isObjectActive()
    {
        return true;
    }

    public void MouseEnter()
    {
        redCircle.gameObject.SetActive(true);
    }


    public void MouseExit()
    {
        redCircle.gameObject.SetActive(false);
    }
}
