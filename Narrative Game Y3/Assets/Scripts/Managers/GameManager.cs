using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NarrativeGame.Dialogue;
using System;

public enum DialogueStatus
{
    Player = 0,
    NPC = 1,
    Inspection = 2,
}


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameStatus
    {
        None = 0,
        Table = 1,
        Map = 2,
        Diorama = 3,
        Newspaper = 4,
        PlayingCard = 5,
        InspectEvidence = 6,
        Dialogue = 7,
        Inspect = 8
    }

    [SerializeField] private GameStatus gameStatus;
    [SerializeField] private int startingCigarettes;
    [SerializeField] private List<Transform> cigList = new();
    [SerializeField] private RectTransform NPCIcons;

    public event Action onStatusUpdated;

    private int remainingCigarettes;

    private InputActions input;

    private NPCController selectedNPC;

    private LayerMask rayLayer;

    public RectTransform GetNPCIcons() { return NPCIcons; }
    public InputActions GetInputs() { return input; }
    public int GetRemainingCigarettes() { return remainingCigarettes; }
    public NPCController GetSelectedNPC() { return selectedNPC; }
    public void SetNPC(NPCController _npc) { selectedNPC = _npc; }
    public GameStatus GetStatus() { return gameStatus; }
    public void SetStatus(GameStatus _status) 
    {
        HandManager.instance.SetPrevStatus(gameStatus);
        gameStatus = _status;
        HUDManager.instance.ActivateUIElementsAccordingToGameStatus(gameStatus);
        if (onStatusUpdated != null) onStatusUpdated(); // Calls any functions subscribed (HandManager)
    }

    private void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (GameManager)");
        instance = this;
    }

    void Start()
    {
        Initialize();
    }

    private void Update()
    {
        MouseCheck();
    }

    private void Initialize()
    {
        Application.targetFrameRate = 300;

        SetStatus(GameStatus.Table);

        remainingCigarettes = startingCigarettes;

        input = new InputActions();
        input.GameInput.Enable();

        input.GameInput.Map.performed += MapToggle;
        input.GameInput.LMB.performed += InteractWith;
        input.GameInput.Back.performed += BackToTableDelegate;

        //input.GameInput.ScrollWheel.performed += ScrollNavigation;
        //input.GameInput.ScrollWheel.performed += ScrollNavigation;
    }

    public void ConsumeCigarette()
    {
        int index = startingCigarettes - remainingCigarettes;

        if(remainingCigarettes > 0) cigList[index].gameObject.SetActive(false);

        remainingCigarettes -= 1;
    }

    /// <summary>
    /// Interaction with the mouse scrollwheel
    /// </summary>
    private void ScrollNavigation(InputAction.CallbackContext context)
    {
        if (context.ReadValue<Vector2>().y < 0) BackToTableDelegate(context);
        if (context.ReadValue<Vector2>().y > 0) InteractWith(context);
    }

    /// <summary>
    /// Back from the current GameMode to the table view
    /// </summary>
    private void BackToTableDelegate(InputAction.CallbackContext context)
    {
        BackToTable();
    }

    public void BackToTable()
    {
        if (!ReadyToContinue()) return;

        if (gameStatus == GameStatus.InspectEvidence) SetStatus(GameStatus.Diorama);
        
            //SetStatus(GameStatus.Diorama);
            //HandManager.instance.PutDownTelephone();
        else SetStatus(GameStatus.Table);

        DisableNPCElements();
    }

    /// <summary>
    /// Toggle between Table and Map camera (trigger part with pressing "M")
    /// </summary>
    private void MapToggle(InputAction.CallbackContext context)
    {
        MapToggle();
    }

    public void MapToggle()
    {
        if (!ReadyToContinue()) return;

        DisableNPCElements();

        NavigationCamera.instance.ToggleMap();
    }

    // Checks if Pointer is on a certain UI elements (Button here)
    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = input.GameInput.MousePosition.ReadValue<Vector2>();
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        for (int index = 0; index < raycastResults.Count; index++)
        {
            RaycastResult curRaysastResult = raycastResults[index];

            if (curRaysastResult.gameObject.transform.TryGetComponent(out Button _UIElement))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Interact with the object if it contains the IObjectInteraction interface
    /// Using layers at the moment, it might change (or not)
    /// </summary>
    private void InteractWith(InputAction.CallbackContext context)
    {
        if (!ReadyToContinue()) return;

        Ray ray = Camera.main.ScreenPointToRay(input.GameInput.MousePosition.ReadValue<Vector2>());

        if (selectedNPC && !IsPointerOverUIElement())
        {
            DisableNPCElements();
        }

        if (IsPointerOverUIElement()) return;

        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (var item in hits)
        {
            if (item.transform.TryGetComponent(out InteractableObjects interactable))
            {
                if (interactable.isObjectInteractivable()) interactable.Interact();
            }
        }
    }

    /// <summary>
    /// Toggles the outline when the mouse is on any interactivable object
    /// </summary>
    private void MouseCheck()
    {
        if (!ReadyToContinue()) return;
        if (IsPointerOverUIElement()) return;

        Ray ray = Camera.main.ScreenPointToRay(input.GameInput.MousePosition.ReadValue<Vector2>());

        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (var item in hits)
        {
            if (item.transform.TryGetComponent(out InteractableObjects interactable))
            {
                if (!interactable.GetComponent<InteractableObjects>().isActiveAndEnabled) return;
                if (interactable.isObjectInteractivable()) interactable.MouseEnter();
            }
        }
    }

    private void DisableNPCElements()
    {
        if (gameStatus == GameStatus.InspectEvidence) return;
        if (gameStatus == GameStatus.Dialogue) return;

        if (!selectedNPC) return;

        selectedNPC.GetComponent<Outline>().SetToggle(false);
        HUDManager.instance.ActivateNPCInteractiveButtons(false);
        selectedNPC = null; 
    }

    /// <summary>
    ///  Check is the Camera transition or the diorama animation is over or not
    /// </summary>
    public bool ReadyToContinue()
    {
        if (NavigationCamera.instance.IsCameraTransitionOver()) return false;
        if (DioramaManager.instance.IsDioramaAnimationOver()) return false;
        if (HandManager.instance.IsAnimating()) return false;

        return true;
    }

    public void ToggleOutline(GameObject _object, bool _toggle)
    {
        if (_toggle)
        {
            if (_object.TryGetComponent(out Outline _childOutline))
            {
                _childOutline.enabled = true;
                _childOutline.SetToggle(true);
                _childOutline.SetOutlineWidth(GlobalOutlineManager.instance.GetOutlineWIdth());
            }
        }
        else
        {
            if (_object.TryGetComponent(out Outline _childOutline))
            {
                _childOutline.enabled = false;
                _childOutline.SetToggle(false);
            }
        }
    }
}

[System.Serializable]
public class InspectEvidenceDialogue
{
    public PlayCardsSObject triggerCard;
    public Dialogue dialogue;
}

[System.Serializable]
public class progressionDialogue
{
    public TaskSO task;
    public Dialogue dialogue;
}
