using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager instance;

    [Header("All the progression tasks (Quests) are here:")]
    [Space]
    [SerializeField] private List<ProgressionTasks> progressionTasks;

    [Header("When a certain task is done, it unlocks these objects - SetActive(true):")]
    [Space]
    [SerializeField] private List<UnlockObjects> unlockObjects;

    [Header("When a certain task is done, it hides these objects - SetActive(false):")]
    [Space]
    [SerializeField] private List<UnlockObjects> lockObjects;

    private int currentTask = 0;

    private List<NarrativeGame.Dialogue.NPCController> listNPC = new();

    public void AddNPC(NarrativeGame.Dialogue.NPCController _npc) { listNPC.Add(_npc); }

    private void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (ProgressionManager)");
        instance = this;
    }

    private void Start()
    {
        if (progressionTasks.Count > 0) HUDManager.instance.UpdateTaskManager(progressionTasks[0]);

        DisableObjects();
    }

    private void OnDestroy()
    {
        foreach (var item in progressionTasks)
        {
            foreach (var task in item.taskDescription)
            {
                task.IsCompleted = false;
            }
        }
    }

    public void RefreshTasks(TaskSO _task)
    {
        UpdateObjects(_task);

        ChangeNPCDialogue(_task);

        for (int i = 0; i < progressionTasks.Count; i++)
        {
            int taskNumber = 0;

            foreach (var task in progressionTasks[i].taskDescription)
            {
                if (task.ToString() == _task.ToString())
                {
                    task.IsCompleted = true;
                }

                if (task.IsCompleted) taskNumber++;

                if (taskNumber == progressionTasks[i].taskDescription.Count && i == currentTask) currentTask++;

                if (progressionTasks.Count - 1 < currentTask) currentTask = progressionTasks.Count - 1;

                HUDManager.instance.UpdateTaskManager(progressionTasks[currentTask]);
            }
        }

    }

    private void UpdateObjects(TaskSO _task)
    {
        foreach (var task in unlockObjects)
        {
            if (task.task.ToString() == _task.ToString())
            {
                foreach (var objects in task.objects)
                {
                    if (objects.TryGetComponent(out NarrativeGame.Dialogue.NPCController npc))
                    {
                        npc.GetComponent<NarrativeGame.Dialogue.NPCController>().enabled = true;
                        npc.GetComponent<Outline>().enabled = true;
                        npc.GetComponent<InteractableObjects>().enabled = true;
                    }

                    objects.SetActive(true);
                }
            }
        }

        foreach (var task in lockObjects)
        {
            if (task.task.ToString() == _task.ToString())
            {
                foreach (var objects in task.objects)
                {
                    objects.SetActive(false);
                }
            }
        }
    }

    private void ChangeNPCDialogue(TaskSO _task)
    {
        foreach (var npc in listNPC)
        {
            npc.ChangeDialogueIfPorgress(_task);
        }
    }

    private void DisableObjects()
    {
        foreach (var task in unlockObjects)
        {
            foreach (var objects in task.objects)
            {
                if (objects.TryGetComponent(out NarrativeGame.Dialogue.NPCController npc))
                {
                    npc.GetComponent<NarrativeGame.Dialogue.NPCController>().enabled = false;
                    npc.GetComponent<Outline>().enabled = false;
                    npc.GetComponent<InteractableObjects>().enabled = false;
                }
                else objects.SetActive(false);
            }
        }
    }
}

[System.Serializable]
public class ProgressionTasks
{
    [TextArea(3, 10)]
    public string taskName;
    public List<TaskSO> taskDescription;
}

[System.Serializable]
public class UnlockObjects
{
    public TaskSO task;
    public List<GameObject> objects;
}
