using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

namespace NarrativeGame.Dialogue
{
    public class InspectSpot : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera inspectCamera;
        [SerializeField] private RectTransform interactIcon;
        [SerializeField] Dialogue dialogue = null;

        PlayerConversant playerConversant;

        public bool IsDialogueAvaliable() { return dialogue; }

        private void OnEnable()
        {
            if (interactIcon) interactIcon.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (interactIcon) interactIcon.gameObject.SetActive(false);
        }

        void Start()
        {
            playerConversant = FindObjectOfType<PlayerConversant>();

            if (!interactIcon) return;
            interactIcon = Instantiate(interactIcon, HUDManager.instance.GetStatusIconParent());
            interactIcon.GetChild(0).GetComponent<Button>().onClick.AddListener(StartInnerDialogue);
        }

        // Update is called once per frame
        void Update()
        {
            IconPositionUpdate();
        }

        public void StartInnerDialogue()
        {
            Debug.Log("Inner Dialogue " + transform.name);
            if (!dialogue) return;

            playerConversant.StartDialogue(dialogue);
            NavigationCamera.instance.SetInspectCamera(inspectCamera);
            NavigationCamera.instance.ActivateCamera(inspectCamera);
            HUDManager.instance.DisableUIElements();
            GameManager.instance.SetStatus(GameManager.GameStatus.Inspect);
        }

        public void IconPositionUpdate()
        {
            if (!interactIcon) return;

            interactIcon.position = Camera.main.WorldToScreenPoint(transform.position);
        }
    }
}