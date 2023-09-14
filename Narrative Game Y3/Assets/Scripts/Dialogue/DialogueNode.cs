using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NarrativeGame.Dialogue
{
    public class DialogueNode : ScriptableObject
    {
        [SerializeField]
        DialogueStatus dialogueStatus;
        [TextArea]
        [SerializeField]
        string text;
        [SerializeField]
        List<string> children = new List<string>();
        [SerializeField]
        Rect rect = new Rect(0, 0, 250, 100);

        [SerializeField]
        private TaskSO completedTask;

        [SerializeField]
        private PlayCardsSObject evidenceCard;

        [SerializeField]
        string onEnterAction;
        [SerializeField]
        string onExitAction;
        [SerializeField]
        int[] aimTarget;

        [SerializeField]
        AudioClip voiceoverAudio;

        public Rect GetRect()
        {
            return rect;
        }

        public string GetText()
        {
            return text;
        }

        public AudioClip GetAudio()
        {
            if(voiceoverAudio != null) return voiceoverAudio;
            return null;
        }

        public int[] GetAimTargets()
        {
            return aimTarget;
        }

        public List<string> GetChildren()
        {
            return children;
        }

        public DialogueStatus GetDialogueStatus()
        {
            return dialogueStatus;
        }

        public TaskSO GetTaskSO()
        {
            return completedTask;
        }

        public PlayCardsSObject GetCardSO()
        {
            return evidenceCard;
        }

        public string GetOnEnterAction()
        {

            return onEnterAction;
        }

        public string GetOnExitAction()
        {

            return onExitAction;
        }

#if UNITY_EDITOR
        public void SetPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Dialogue Node"); //Store undo record
            rect.position = newPosition;
            EditorUtility.SetDirty(this);
        }

        public void SetText(string newText)
        {
            if(newText != text)
            {
                Undo.RecordObject(this, "Update Dialogue Text"); //Store undo record
                text = newText;
                EditorUtility.SetDirty(this);
            }
        }

        public void AddChild(string childID) 
        {
            Undo.RecordObject(this, "Add Dialogue Link"); //Store undo record
            children.Add(childID);
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(string childID)
        {
            Undo.RecordObject(this, "Remove Dialogue Link"); //Store undo record
            children.Remove(childID);
            EditorUtility.SetDirty(this);
        }

        public void SetDialogueStatus(DialogueStatus newDialogueStatus)
        {
            Undo.RecordObject(this, "Change Dialogue Speaker"); //Store undo record
            dialogueStatus = newDialogueStatus;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}