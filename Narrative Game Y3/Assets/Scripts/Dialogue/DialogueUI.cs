using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NarrativeGame.Dialogue;
using TMPro;
using UnityEngine.UI;

namespace NarrativeGame.UI
{
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI instance;

        [SerializeField] private float typeSpeed;
        [SerializeField] private bool instantType;
        
        PlayerConversant playerConversant;
        [SerializeField] TextMeshProUGUI dialogueText;
        [SerializeField] Button nextButton;
        [SerializeField] GameObject textField;
        [SerializeField] Transform choiceRoot;
        [SerializeField] GameObject choicePrefab;
        [SerializeField] Button quitButton;
        [SerializeField] TextMeshProUGUI conversantName;

        private bool isDialogueFinished;

        public bool GetIsDialogueFinished() { return isDialogueFinished; }
        public void SetIsDialogueFinished(bool _bool) { isDialogueFinished = _bool; }

        private void Awake()
        {
            if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (DialogueUI)");
            instance = this;
        }

        void Start()
        {
            isDialogueFinished = true;
            playerConversant = FindObjectOfType<PlayerConversant>();
            playerConversant.onConversationUpdated += UpdateUI;
            nextButton.onClick.AddListener(() => playerConversant.Next());
            quitButton.onClick.AddListener(() => playerConversant.Quit());

            UpdateUI();
        }

        // Updates all the information inside the Dialogue fields, subscribed to onConversationUpdated which is called inside the Player Conversant Script
        void UpdateUI()
        {
            if (!isDialogueFinished && playerConversant.IsActive())
            {
                isDialogueFinished = true;
                return;
            }

            gameObject.SetActive(playerConversant.IsActive());
            if (!playerConversant.IsActive())
            {
                return;
            }
            conversantName.text = playerConversant.GetCurrentConversantName();

            // Enable/Disable appropriate UI Field
            textField.SetActive(!playerConversant.IsChoosing());
            nextButton.gameObject.SetActive(!playerConversant.IsChoosing());
            choiceRoot.gameObject.SetActive(playerConversant.IsChoosing());

            if (playerConversant.IsChoosing())
            {
                BuildChoiceList();
            }
            else
            {
                if (!instantType) StartCoroutine(TypeWriterDialogue(playerConversant.GetText()));
                else
                {
                    dialogueText.text = playerConversant.GetText(); // Update Textbox with current text
                    nextButton.gameObject.SetActive(playerConversant.HasNext()); // Show Next Button if there is another text in sequence
                    quitButton.gameObject.SetActive(!playerConversant.HasNext()); // Show Quit Button if there is no text in sequence
                }
            }
        }

        IEnumerator TypeWriterDialogue(string text)
        {
            isDialogueFinished = false;

            nextButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ">>";
            nextButton.gameObject.SetActive(true);
            quitButton.gameObject.SetActive(false);

            int counter = 0;
            string currentDialogue = "";
            dialogueText.text = "";
            int characterNumber = text.Length;

            while (!isDialogueFinished)
            {
                yield return new WaitForSecondsRealtime(typeSpeed);

                currentDialogue += text[counter];

                dialogueText.text = currentDialogue;

                counter++;
                if (counter >= characterNumber) isDialogueFinished = true;
            }

            dialogueText.text = text;
            isDialogueFinished = true;
            nextButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Next >";
            nextButton.gameObject.SetActive(playerConversant.HasNext());
            quitButton.gameObject.SetActive(!playerConversant.HasNext());
        }

        private void BuildChoiceList()
        {
            foreach (Transform item in choiceRoot) // Destroy previous choices
            {
                Destroy(item.gameObject);
            }
            foreach (DialogueNode choice in playerConversant.GetChoices()) // Populate current choices
            {
                GameObject choiceInstance = Instantiate(choicePrefab, choiceRoot);
                var textComp = choiceInstance.GetComponentInChildren<TextMeshProUGUI>();
                textComp.text = choice.GetText();

                Button button = choiceInstance.GetComponentInChildren<Button>();

                // Lambda method creates a listener function for each button
                button.onClick.AddListener(() =>
                {
                    playerConversant.SelectChoice(choice);
                });
            }
        }
    }
}