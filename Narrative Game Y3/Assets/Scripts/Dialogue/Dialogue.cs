using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NarrativeGame.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]
    public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<DialogueNode> nodes = new List<DialogueNode>();
        [SerializeField]
        Vector2 newNodeOffset = new Vector2(300, 0);

        Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();

        private void Awake()
        {
            //IMPORTANT - OnValidate is not called in exported builds, needs to be called on awake
            OnValidate();
        }

        private void OnValidate()
        {
            nodeLookup.Clear();

            foreach (DialogueNode node in GetAllNodes())
            {
                nodeLookup[node.name] = node;
            }
        }

        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return nodes;
        }

        public DialogueNode GetRootNode()
        {
            return nodes[0];
        }

        public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
        {
            foreach (string childID in parentNode.GetChildren())
            {
                if (nodeLookup.ContainsKey(childID))
                {
                    yield return nodeLookup[childID];
                }
            }
        }

#if UNITY_EDITOR
        // Functions available inside the Unity Editor, used to modify ScriptableObjects in the Dialogue Editor
        public void CreateNode(DialogueNode parent)
        {
            DialogueNode newNode = MakeNode(parent);
            Undo.RegisterCreatedObjectUndo(newNode, "Created Dialogue Node");
            Undo.RecordObject(this, "Added Dialogue Node"); //Store Undo record before creating
            AddNode(newNode);
        }


        public void DeleteNode(DialogueNode nodeToDelete)
        {
            Undo.RecordObject(this, "Removed Dialogue Node"); //Store Undo record before deleting
            nodes.Remove(nodeToDelete);
            OnValidate();
            CleanDanglingChildren(nodeToDelete);
            Undo.DestroyObjectImmediate(nodeToDelete);
        }
        private void AddNode(DialogueNode newNode)
        {
            nodes.Add(newNode);
            OnValidate();
        }

        private DialogueNode MakeNode(DialogueNode parent)
        {
            DialogueNode newNode = CreateInstance<DialogueNode>();
            newNode.name = Guid.NewGuid().ToString();
            if (parent != null)
            {
                parent.AddChild(newNode.name);
                newNode.SetDialogueStatus(DialogueStatus.NPC);
                newNode.SetPosition(parent.GetRect().position + newNodeOffset); //Offset new node next to parent
            }

            return newNode;
        }

        private void CleanDanglingChildren(DialogueNode nodeToDelete)
        {
            foreach (DialogueNode node in GetAllNodes())
            {
                node.RemoveChild(nodeToDelete.name);
            }
        }
#endif
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (nodes.Count == 0)
            {
                DialogueNode newNode = MakeNode(null);
                AddNode(newNode);
            }

            if (AssetDatabase.GetAssetPath(this) != "")
            {
                foreach (DialogueNode node in GetAllNodes())
                {
                    if(AssetDatabase.GetAssetPath(node) == "")
                    {
                        AssetDatabase.AddObjectToAsset(node, this);
                    }
                }
            }
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }
}