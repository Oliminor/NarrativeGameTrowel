using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NarrativeGame.Dialogue
{
    public class PlayerConversant : MonoBehaviour
    {
        public static PlayerConversant instance;

        [SerializeField] string playerName;
        [SerializeField] AudioSource dialogueAudioSource;
        [SerializeField] float playbackVolume = 1;
        [SerializeField] TargetGroupManager targetManager;

        Dialogue currentDialogue;
        DialogueNode currentNode = null;
        NPCController currentConversant = null;
        bool isChoosing = false;

        public event Action onConversationUpdated;

        public bool GetIsAudioPlaying() { return dialogueAudioSource.isPlaying; }

        void Awake()
        {
            if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (PlayerConversant)");
            instance = this;
        }

        // Start Dialogue (Called from the NPCController when hitting "Speak")
        public void StartDialogue(NPCController newConversant, Dialogue newDialogue)
        {
            currentConversant = newConversant;
            currentDialogue = newDialogue;
            currentNode = currentDialogue.GetRootNode();
            TriggerEnterAction();
            onConversationUpdated();
        }

        public void StartDialogue(Dialogue newDialogue)
        {
            currentDialogue = newDialogue;
            currentNode = currentDialogue.GetRootNode();
            TriggerEnterAction();
            onConversationUpdated();
        }

            // Quit Dialogue (Called from the Quit button when no more dialogue texts in the sequence)
        public void Quit()
        {
            switch (currentNode.GetDialogueStatus())
            {
                case DialogueStatus.Player:
                    BlackBars.instance.BlackBarOff();
                    Lamp.instance.ChangeLampTarget(LampTarget.UP);
                    StartCoroutine(MovePiecesBack());
                    break;


                case DialogueStatus.NPC:
                    BlackBars.instance.BlackBarOff();
                    Lamp.instance.ChangeLampTarget(LampTarget.UP);
                    StartCoroutine(MovePiecesBack());
                    break;


                case DialogueStatus.Inspection:
                    GameManager.instance.SetStatus(GameManager.GameStatus.Diorama);
                    break;
            }

            currentDialogue = null;
            TriggerExitAction();
            currentNode = null;
            isChoosing = false;
            currentConversant = null;
            onConversationUpdated();
            dialogueAudioSource.Stop();

            UI.DialogueUI.instance.SetIsDialogueFinished(true);

            if (GameManager.instance.GetRemainingCigarettes() < 0) BlackSmokeScreen.instance.TriggerGameOver();
        }

        // Delaying the dialogue until the pieces moved in the right position
        IEnumerator MovePiecesBack()
        {
            HandManager.instance.MovePiecesBack();

            yield return new WaitForSeconds(0.1f);

            while (HandManager.instance.IsAnimating()) yield return null;

            GameManager.instance.SetStatus(GameManager.GameStatus.Diorama);
        }

        // Checks if there is an active dialogue in the scene
        public bool IsActive()
        {
            return currentDialogue != null;
        }

        // Returns true when there are multiple children for the next node
        public bool IsChoosing()
        {
            return isChoosing;
        }

        // Returns the text from the currently active node
        public string GetText()
        {
            if (currentNode == null)
            {
                return "";
            }
            
            return currentNode.GetText();
        }

        // Returns the name of the person that is currently speaking, either the current conversantName of the NPC (anonymous until discovered) or the playerName
        public string GetCurrentConversantName()
        {
            switch (currentNode.GetDialogueStatus())
            {
                case DialogueStatus.Player:
                    return playerName;

                case DialogueStatus.NPC:
                    return currentConversant.GetName();

                case DialogueStatus.Inspection:
                    return null;

                default:
                    if(isChoosing) return playerName;
                    break;
            }

            return null;
        }

        // Returns all children of the currently active node
        public IEnumerable<DialogueNode> GetChoices()
        {
            return currentDialogue.GetAllChildren(currentNode);
        }

        // Lambda function added to each button instantiated in UpdateUI
        public void SelectChoice(DialogueNode chosenNode)
        {
            currentNode = chosenNode;
            TriggerEnterAction();
            isChoosing = false;
            Next();
        }

        // Next button available when there are children available in the current node
        public void Next()
        {
            DialogueNode[] children = currentDialogue.GetAllChildren(currentNode).ToArray();
            if (children.Length > 1)
            {
                isChoosing = true;
                TriggerExitAction();
                onConversationUpdated();
                targetManager.SetFocusTarget(0, 1);
                return;
            }
            
            int randomIndex = UnityEngine.Random.Range(0, children.Length);
            if (UI.DialogueUI.instance.GetIsDialogueFinished()) TriggerExitAction();
            if (UI.DialogueUI.instance.GetIsDialogueFinished()) currentNode = children[randomIndex];
            TriggerEnterAction();
            onConversationUpdated();
        }

        // returns true when there is an available child node
        public bool HasNext()
        {
            return currentDialogue.GetAllChildren(currentNode).Count() > 0;
        }

        // Trigger actions are strings available on each node for either Entry or Exit, can be used to call specific functions by adding a DialogueTrigger with the same string on the gameobject
        private void TriggerEnterAction()
        {
            if(currentNode != null) 
            {
                TriggerAction(currentNode.GetOnEnterAction());

                dialogueAudioSource.Stop();

                if (currentNode.GetAudio() != null)
                {
                    if (UI.DialogueUI.instance.GetIsDialogueFinished()) dialogueAudioSource.PlayOneShot(currentNode.GetAudio(), playbackVolume);
                }

                targetManager.ResetFocus();
                
                int[] targets = currentNode.GetAimTargets();

                if (targets != null)
                {
                    switch (targets.Length)
                    {
                        case 1:
                            targetManager.SetFocusTarget(targets[0]);
                            return;
                        //break;
                        case 2:
                            targetManager.SetFocusTarget(targets[0], targets[1]);
                            return;
                        //break;
                        case 3:
                            targetManager.SetFocusTarget(targets[0], targets[1], targets[2]);
                            return;
                            //break;
                    }
                }

                if (currentNode.GetTaskSO()) ProgressionManager.instance.RefreshTasks(currentNode.GetTaskSO());

                if (currentNode.GetCardSO())
                {
                    PlayingCardManager.instance.AddCard(currentNode.GetCardSO());
                }

                switch (currentNode.GetDialogueStatus())
                {
                    case DialogueStatus.Player:
                        HandManager.instance.StopUpAndDownMovement();
                        targetManager.SetFocusTarget(0);
                        HandManager.instance.MovePiecesUpAndDown(0);
                        break;
                    case DialogueStatus.NPC:
                        HandManager.instance.StopUpAndDownMovement();
                        targetManager.SetFocusTarget(1);
                        HandManager.instance.MovePiecesUpAndDown(1);
                        break;
                    case DialogueStatus.Inspection:
                        break;
                    default:
                        break;
                }
            }
        }

        private void TriggerExitAction()
        {
            if(currentNode != null)
            {
                TriggerAction(currentNode.GetOnExitAction());
            }
        }

        private void TriggerAction(string action)
        {
            if (action == "") return;
            foreach (DialogueTrigger trigger in currentConversant.GetComponents<DialogueTrigger>())
            {
                trigger.Trigger(action);
            }
        }
    }
}