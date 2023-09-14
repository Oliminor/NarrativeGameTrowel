using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NarrativeGame.Dialogue;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    [SerializeField] private RectTransform statusIcons;
    [SerializeField] private RectTransform npcInteractiveButtons;
    [SerializeField] private RectTransform mapButton;
    [SerializeField] private RectTransform backButton;
    [SerializeField] private RectTransform finalButton;
    [SerializeField] private RectTransform cigUI;
    [SerializeField] private RectTransform taskWindow;
    [SerializeField] private RectTransform cardObtained;

    private GameManager.GameStatus tempStatus; // for updating the UI elements


    public RectTransform GetStatusIconParent() { return statusIcons; }
    public RectTransform GetNPCInteractiveButtons() { return npcInteractiveButtons; }

    void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (HUDManager)");
        instance = this;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        statusIcons.gameObject.SetActive(false);
        npcInteractiveButtons.gameObject.SetActive(false);
        cigUI.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        finalButton.gameObject.SetActive(false);
        cardObtained.gameObject.SetActive(false);
    }

    /// <summary>
    ///  Disable every UI elements
    /// </summary>
    public void DisableUIElements()
    {
        statusIcons.gameObject.SetActive(false);
        npcInteractiveButtons.gameObject.SetActive(false);
        mapButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        finalButton.gameObject.SetActive(false);
        taskWindow.gameObject.SetActive(false);

        PlayingCardManager.instance.CloseCardMenu();
    }

    public void ActivateNPCInteractiveButtons(bool _bool)
    {
        if (_bool)
        {
            npcInteractiveButtons.position = Camera.main.WorldToScreenPoint(GameManager.instance.GetSelectedNPC().transform.position);
            npcInteractiveButtons.position += new Vector3(Screen.height / 8, 0, 0);
            StartCoroutine(InteractiveButtonsUpdate());

            npcInteractiveButtons.gameObject.SetActive(true);
        }
        else
        {
            npcInteractiveButtons.gameObject.SetActive(false);
        }
    }

    IEnumerator InteractiveButtonsUpdate()
    {
        yield return new WaitForSeconds(0.001f);
        while (!GameManager.instance.ReadyToContinue())
        {
            if (!npcInteractiveButtons) yield break;
            yield return new WaitForSeconds(Time.deltaTime);
            npcInteractiveButtons.position = Camera.main.WorldToScreenPoint(GameManager.instance.GetSelectedNPC().transform.position);
            npcInteractiveButtons.position += new Vector3(Screen.height / 8, 0, 0);
        }
    }

    /// <summary>
    /// I think the Function name is straightforward
    /// </summary>
    public void ActivateUIElementsAccordingToGameStatus(GameManager.GameStatus _status)
    {
        if (_status != tempStatus)

            DisableUIElements();

        NavigationCamera navigationC = NavigationCamera.instance;

        switch (_status)
        {
            case GameManager.GameStatus.Table:
                backButton.gameObject.SetActive(false);
                StartCoroutine(ShowAfterTransition(mapButton));
                navigationC.ActivateCamera(navigationC.GetTableCamera());
                break;

            case GameManager.GameStatus.Map:
                StartCoroutine(ShowAfterTransition(backButton));
                navigationC.ActivateCamera(navigationC.GetMapCamera());
                break;

            case GameManager.GameStatus.Diorama:
                StartCoroutine(ShowAfterTransition(backButton));
                StartCoroutine(ShowAfterTransition(mapButton));
                statusIcons.gameObject.SetActive(true);
                navigationC.ActivateCamera(navigationC.GetDioramaCamera());
                break;

            case GameManager.GameStatus.Newspaper:
                StartCoroutine(ShowAfterTransition(backButton));
                navigationC.ActivateCamera(navigationC.GetNewspaperCamera());
                break;

            case GameManager.GameStatus.PlayingCard:
                backButton.gameObject.SetActive(true);
                break;

            case GameManager.GameStatus.InspectEvidence:
                backButton.gameObject.SetActive(true);
                break;

            case GameManager.GameStatus.Dialogue:
                navigationC.ActivateCamera(navigationC.GetDialogueCamera());
                backButton.gameObject.SetActive(false);
                break;

            case GameManager.GameStatus.Inspect:
                break;

            default:
                navigationC.ActivateCamera(navigationC.GetTableCamera());
                break;
        }

        if (_status != GameManager.GameStatus.Dialogue) taskWindow.gameObject.SetActive(true);
        tempStatus = GameManager.instance.GetStatus();
    }

    IEnumerator ShowAfterTransition(RectTransform _transform)
    {
        yield return new WaitForSeconds(0.1f);
        _transform.gameObject.SetActive(false);
        yield return new WaitUntil(() => GameManager.instance.ReadyToContinue());
        _transform.gameObject.SetActive(true);
    }

    public void ShowFinalButton(bool _bool)
    {
        finalButton.gameObject.SetActive(_bool);
    }

    /// <summary>
    /// Speak interaction with NPC
    /// </summary>
    public void Speak()
    {
        if (!GameManager.instance.GetSelectedNPC()) return;

        GameManager.instance.GetSelectedNPC().Speak();
    }

    /// <summary>
    /// Inspect interaction with NPC
    /// </summary>
    public void Inspect()
    {
        if (!GameManager.instance.GetSelectedNPC()) return;

        GameManager.instance.GetSelectedNPC().Inspect();
    }

    /// <summary>
    /// ShowObject interaction with NPC
    /// </summary>
    public void ShowCard()
    {
        if (!GameManager.instance.GetSelectedNPC()) return;

        GameManager.instance.GetSelectedNPC().ShowCard();
    }

    /// <summary>
    /// Cigaretta UI
    /// </summary>
    public void ShowAndSetCigUI(int _stressLeft, Vector3 _position)
    {
        cigUI.gameObject.SetActive(true);
        cigUI.GetChild(0).GetComponent<TextMeshProUGUI>().text = _stressLeft.ToString();
        cigUI.position = Camera.main.WorldToScreenPoint(_position);
    }

    public void DisableCigUI()
    {
        cigUI.gameObject.SetActive(false);
    }

    public void UpdateTaskManager(ProgressionTasks _tasks)
    {
        taskWindow.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";

        taskWindow.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = _tasks.taskName;

        foreach (var task in _tasks.taskDescription)
        {
            string start = "- ";

            if (task.IsCompleted)
            {
                string prefix = "<s>";
                string suffix = "</s>";

                StartCoroutine(TaskListUpdatedAnim());

                if (!task.IsHidden) taskWindow.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text += prefix + start + task.TaskDescription + suffix + "\n";
            }
            else
            {
               if (!task.IsHidden) taskWindow.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text += start + task.TaskDescription + "\n";
            }

        }
    }

    public void NewCardObtainedAnimTrigger()
    {
        cardObtained.gameObject.SetActive(true);
    }

    IEnumerator TaskListUpdatedAnim()
    {
        while (GameManager.instance.GetStatus() != GameManager.GameStatus.Diorama) yield return null;

        taskWindow.GetChild(1).gameObject.SetActive(true);

        Color originalColor = taskWindow.GetChild(1).GetComponent<TextMeshProUGUI>().color;

        float time = 2.0f;
        float value = 0;

        while (time > 0 || value > 0.1f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            time -= Time.deltaTime;

            value = 0.5f * Mathf.Sin(2f * Mathf.PI * 0.5f * Time.time) + 0.5f;

            taskWindow.GetChild(1).GetComponent<TextMeshProUGUI>().color = new Color(originalColor.r, originalColor.g, originalColor.b, value);
        }

        taskWindow.GetChild(1).gameObject.SetActive(false);
    }
}
