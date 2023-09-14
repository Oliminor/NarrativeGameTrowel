using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NarrativeGame.Dialogue
{
    public class NPCController : MonoBehaviour, IObjectInteraction
    {
        [SerializeField] private Transform handPosOffset;
        [SerializeField] Dialogue startDialogue = null;
        [SerializeField] string conversantName;

        [Header("These are for the Inspect card:")]
        [Space]
        [SerializeField] private Dialogue wrongCardDialogue;
        [SerializeField] private List<InspectEvidenceDialogue> InspectCardDialogueList;

        [Header("This is for the progression dialogue change:")]
        [Space]
        [SerializeField] private List<progressionDialogue> progressionDialogueList;

        PlayerConversant playerConversant;

        private RectTransform statusIcons;

        private PlayCardsSObject[] playingCardsOnSlot = new PlayCardsSObject[3];

        public PlayCardsSObject[] GetPlayingCardsOnSlot() { return playingCardsOnSlot; }

        public Transform GetHandOffsetPos() { return handPosOffset; }

        public enum InteractStatus // NPC status based on if the player already interacted with it or it has new dialogue active
        {
            Unknown = 0,
            Interacted = 1,
            NewDialogue = 2
        }

        private InteractStatus characterStatus;

        void Start()
        {
            playerConversant = FindObjectOfType<PlayerConversant>();
            ChangeStatus(InteractStatus.Unknown);
            ProgressionManager.instance.AddNPC(this);
            if (!statusIcons) statusIcons = Instantiate(GameManager.instance.GetNPCIcons(), HUDManager.instance.GetStatusIconParent());
        }

        private void OnEnable() // Shows the NPC icon when the NPC object is active
        {
            if (statusIcons) statusIcons.gameObject.SetActive(true);
        }

        private void OnDisable() // Hides the NPC icon when the NPC object is active
        {
            if (statusIcons) statusIcons.gameObject.SetActive(false);
        }

        private void Update()
        {
            IconPositionUpdate();
        }

        /// <summary>
        /// Disable all icons
        /// </summary>
        private void DisableAllIcons()
        {
            for (int i = 0; i < statusIcons.childCount; i++)
            {
                statusIcons.GetChild(i).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Insert here any code to trigger when the player clicks on the NPC
        /// </summary>
        public void Interact()
        {
            if (GameManager.instance.GetStatus() != GameManager.GameStatus.Diorama) return;

            if (!GetComponent<InteractableObjects>().isActiveAndEnabled) return;

            GameManager.instance.SetNPC(this);

            if (transform.TryGetComponent(out Outline _outline))
            {
                _outline.enabled = true;
                _outline.SetToggle(true);
                _outline.SetOutlineWidth(GlobalOutlineManager.instance.GetOutlineWIdth());
            }

            HUDManager.instance.ActivateNPCInteractiveButtons(true);

            if (PlayingCardManager.instance.CurrentCardNumber() == 0) HUDManager.instance.GetNPCInteractiveButtons().GetChild(2).gameObject.SetActive(false);
            else HUDManager.instance.GetNPCInteractiveButtons().GetChild(2).gameObject.SetActive(true);

            if (InspectCardDialogueList.Count == 0) HUDManager.instance.GetNPCInteractiveButtons().GetChild(2).gameObject.SetActive(false);

            HUDManager.instance.GetNPCInteractiveButtons().GetChild(1).gameObject.SetActive(GetComponent<InspectSpot>().IsDialogueAvaliable());
        }

        public bool isObjectActive()
        {
            return true;
        }

        public string GetName()
        {
            return conversantName;
        }

        public void SetName(string name)
        {
            conversantName = name;
        }

        public void Speak()
        {
            Debug.Log("NPC Speak");

            StartCoroutine(StartDialogue(startDialogue));
            GameManager.instance.SetStatus(GameManager.GameStatus.Dialogue);
            ChangeStatus(InteractStatus.Interacted);
        }

        IEnumerator StartDialogue(Dialogue _dialogue)
        {
            yield return new WaitForSeconds(0.1f);

            while (HandManager.instance.IsAnimating()) yield return null;

            playerConversant.StartDialogue(this, _dialogue);
        }

        public void Inspect()
        {
            Debug.Log("NPC Inspect");
            GetComponent<InspectSpot>().StartInnerDialogue();
        }

        public void ShowCard()
        {
            Debug.Log("NPC Show Object");
            GameManager.instance.SetStatus(GameManager.GameStatus.InspectEvidence);
            PlayingCardManager.instance.InteractWithDeck(CardType.All);
        }

        public void InspectEvidence(PlayCardsSObject _card)
        {
            Debug.Log("Inspect this: " + _card + " -" + transform.name);

            Dialogue tempDialogue = null;

            foreach (var item in InspectCardDialogueList)
            {
                if (item.triggerCard.ToString() == _card.ToString()) tempDialogue = item.dialogue;
            }

            if (tempDialogue == null)
            {
                GameManager.instance.ConsumeCigarette();
                tempDialogue = wrongCardDialogue;
            }

            StartCoroutine(StartDialogue(tempDialogue));
            GameManager.instance.SetStatus(GameManager.GameStatus.Dialogue);
        }

        public void ChangeDialogueIfPorgress(TaskSO _task)
        {
            foreach (var progress in progressionDialogueList)
            {
                if (progress.task.ToString() == _task.ToString())
                {
                    startDialogue = progress.dialogue;
                    ChangeStatus(InteractStatus.NewDialogue);
                }
            }
        }

        /// <summary>
        /// Positions the NPC icons according to the NPC position (+ shifting so doesn't cover the NPC object itself)
        /// </summary>
        private void IconPositionUpdate()
        {
            if (statusIcons) statusIcons.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.85f);
        }

        /// <summary>
        /// Changes the NPC status and activates the right Icon
        /// </summary>
        public void ChangeStatus(InteractStatus _status)
        {
            if (!statusIcons) return;

            characterStatus = _status;

            DisableAllIcons();

            statusIcons.GetChild((int)_status).gameObject.SetActive(true);
        }

        public void AddCardToNPCSlot(Transform _card)
        {
            int typeIndex = (int)_card.GetComponent<PlayingCard>().GetCardData().Type - 1;
            playingCardsOnSlot[typeIndex] = _card.GetComponent<PlayingCard>().GetCardData();

            CheckSlot();
        }

        public void RemoveCardFromNPCSlot(int _index)
        {
            _index--;

            if (playingCardsOnSlot[_index] == null) return;

            playingCardsOnSlot[_index] = null;

            CheckSlot();
        }

        private void CheckSlot()
        {
            for (int i = 0; i < playingCardsOnSlot.Length; i++)
            {
                if (playingCardsOnSlot[i] == null)
                {
                    HUDManager.instance.ShowFinalButton(false);
                    return;
                }
            }
            HUDManager.instance.ShowFinalButton(true);
        }

    }
}
