using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayingCard : MonoBehaviour
{
    private CardStatus status;
    private CardType type;

    private PlayCardsSObject cardData;

    [SerializeField] private Transform image;
    [SerializeField] private Transform title;
    [SerializeField] private Transform description;
    
    public void SetCardData(PlayCardsSObject _data) { cardData = _data; }
    public PlayCardsSObject GetCardData() { return cardData; }

    public CardType GetCardType() { return type; }
    public void SetType(CardType _type) { type = _type; }
    public CardStatus GetCardStatus() { return status; }
    public void SetCardStatus(CardStatus _status) { status = _status; }

    private bool isLerpingUP = false;
    private bool isLerpingDown = false;
    private bool isLerping = false;

    private bool isDragging = false;
    private bool isMouseEnterWhileMoving = false;

    private bool isCardHovered = false;
    private bool isEvidenceShowed = false;

    private Vector3 tempStartPos;
    private Vector3 startPosition;
    private Vector3 clickPosition;

    private float distanceFromCamera;
    private Vector3 lastMousePos;

    IEnumerator lerpIE;

    public void SetTempStartPos(Vector3 _pos) { tempStartPos = _pos; }
    public Vector3 getTempStartPos() { return tempStartPos; }
    public void SetIsCardHovered(bool _bool) { isCardHovered = _bool; }

    private void Start()
    {
        gameObject.AddComponent<Outline>();

        type = cardData.Type;
        image.GetComponent<Image>().sprite = cardData.Image;
        title.GetComponent<TextMeshProUGUI>().text = cardData.Title;
        description.GetComponent<TextMeshProUGUI>().text = cardData.Description;
    }

    private void OnMouseDown()
    {
        if (isLerping) return;

        if (PlayingCardManager.instance.GetCurrentCardType() != type) return;

        PlayingCardManager.instance.SetIsDragging(true);

        clickPosition = GameManager.instance.GetInputs().GameInput.MousePosition.ReadValue<Vector2>();
    }

    private void OnMouseUp()
    {
        isDragging = false;
        PlayingCardManager.instance.SetIsDragging(false);

        if (PlayingCardManager.instance.GetCurrentCardType() != type) return;

        PlayingCardManager.instance.ToggleSlotOutline(false, cardData.Type);

        CheckUnderMouse();

        if (isEvidenceShowed)
        {
            isEvidenceShowed = false;
            return;
        }

        if (status == CardStatus.Drag) status = CardStatus.Hand;

        if (isLerping) return;

        Vector3 mousePosition = GameManager.instance.GetInputs().GameInput.MousePosition.ReadValue<Vector2>();

        if (mousePosition == clickPosition && status != CardStatus.CloseUp)
        {
            if (status == CardStatus.CardSlot) GameManager.instance.GetSelectedNPC().RemoveCardFromNPCSlot((int)cardData.Type);

            PlayingCardManager.instance.CheckCloseUpCards();
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            transform.SetParent(PlayingCardManager.instance.GetCloseUpCardPosition());
            StartCoroutine(LerpCard(Vector3.zero, true));
            status = CardStatus.CloseUp;
        }
        else
        {
            if (status == CardStatus.CloseUp) PlayingCardManager.instance.CheckCloseUpCards();
        }

        PlayingCardManager.instance.PlaceCardBack();
    }

    private void OnMouseDrag()
    {
        if (PlayingCardManager.instance.GetCurrentCardType() != type) return;

        //if (GameManager.instance.GetStatus() != GameManager.GameStatus.PlayingCard) return;

        PlayingCardManager.instance.ToggleSlotOutline(true, cardData.Type);

        Vector3 mousePosition = GameManager.instance.GetInputs().GameInput.MousePosition.ReadValue<Vector2>();

        if (mousePosition == clickPosition) return;

        if (!isDragging)
        {
            if (isDragging) return;
            isDragging = true;

            PrepareForDragging();
        }

        Vector3 deltaMouse = MouseDelta();

        transform.position += transform.right * deltaMouse.x;
        transform.position -= transform.up * deltaMouse.z;
    }

    private void OnMouseEnter()
    {
        if (status == CardStatus.Hand) return;

        if (transform.TryGetComponent(out Outline _outline))
        {
            _outline.enabled = true;
            _outline.SetToggle(true);
            _outline.SetOutlineWidth(GlobalOutlineManager.instance.GetOutlineWIdth());
        }
    }

    private void OnMouseExit()
    {
        OutlineToggleOff();
    }

    public void MouseEnter()
    {
        if (isCardHovered) return;

        isCardHovered = true;

        if (PlayingCardManager.instance.GetCurrentCardType() != type) return;


        if (!PlayingCardManager.instance.IsCardFaningDone())
        {
            isMouseEnterWhileMoving = true;
            return;
        }

        if (PlayingCardManager.instance.IsDragging()) return;

        if (isLerpingDown || isLerpingUP) return;

        if (transform.TryGetComponent(out Outline _outline))
        {
            _outline.enabled = true;
            _outline.SetToggle(true);
            _outline.SetOutlineWidth(GlobalOutlineManager.instance.GetOutlineWIdth());
        }

        if (status != CardStatus.Hand) return;

        startPosition = transform.position;

        if (isLerping) return;////

        isLerpingUP = true;

        lerpIE = LerpCard(transform.position + transform.up * 0.2f + transform.forward * 0.15f, false);
        StartCoroutine(lerpIE);
    }

    public void MouseExit()
    {
        if (!isCardHovered) return;

        isCardHovered = false;

        OutlineToggleOff();

        if (isMouseEnterWhileMoving)
        {
            isMouseEnterWhileMoving = false;
            return;
        }

        if (PlayingCardManager.instance.GetCurrentCardType() != type) return;

        if (!PlayingCardManager.instance.IsCardFaningDone()) return;

        if (PlayingCardManager.instance.IsDragging()) return;

        if (status != CardStatus.Hand) return;

        if (isLerpingDown) return;

        if (startPosition == Vector3.zero) return;

        isLerpingDown = true;

        if (lerpIE != null) StopCoroutine(lerpIE);
        lerpIE = LerpCard(startPosition, false);
        StartCoroutine(lerpIE);
    }

    public void OutlineToggleOff()
    {
        if (transform.TryGetComponent(out Outline _outline))
        {
            _outline.enabled = false;
            _outline.SetToggle(false);
        }
    }

    IEnumerator LerpCard(Vector3 lerpPos, bool isLocal)
    {
        isLerping = true;

        float lerp = 0;
        Vector3 startPosition = transform.position;
        if (isLocal) startPosition = transform.localPosition;

        while (lerp < 1)
        {
            yield return new WaitForSeconds(0.01f);

            lerp += 0.25f;

            transform.position = Vector3.Lerp(startPosition, lerpPos, lerp);
            if (isLocal) transform.localPosition = Vector3.Lerp(startPosition, lerpPos, lerp);
        }

        isLerpingDown = false;
        isLerpingUP = false;
        isLerping = false;
    }

    private Vector3 MouseDelta()
    {
        Vector3 mousePosition = GameManager.instance.GetInputs().GameInput.MousePosition.ReadValue<Vector2>();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceFromCamera));

        Vector3 delta = new Vector3(mouseWorldPosition.x - lastMousePos.x, mouseWorldPosition.y - lastMousePos.y, mouseWorldPosition.z - lastMousePos.z);

        lastMousePos = mouseWorldPosition;

        return delta;
    }

    private void PrepareForDragging()
    {
        if (status == CardStatus.CardSlot) GameManager.instance.GetSelectedNPC().RemoveCardFromNPCSlot((int)cardData.Type);

        PlayingCardManager.instance.CheckCloseUpCards();

        if (status == CardStatus.Hand) transform.SetParent(PlayingCardManager.instance.GetFanOutCardsPosition());

        status = CardStatus.Drag;

        transform.localRotation = Quaternion.Euler(Vector3.zero);

        Vector3 pos = new Vector3(transform.localPosition.x, transform.localPosition.y, 0.15f);
        transform.localPosition = pos;
        transform.SetParent(null);

        PlayingCardManager.instance.PlaceCardBack();

        distanceFromCamera = 1.66f;

        Vector3 mousePosition = GameManager.instance.GetInputs().GameInput.MousePosition.ReadValue<Vector2>();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceFromCamera));

        lastMousePos = mouseWorldPosition;
    }

    private void CheckUnderMouse()
    {
        if (status != CardStatus.Drag) return;

        Ray ray = Camera.main.ScreenPointToRay(GameManager.instance.GetInputs().GameInput.MousePosition.ReadValue<Vector2>());

        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (var item in hits)
        {
            if (item.transform.TryGetComponent(out EvidenceSlot _evidenceSlot))
            {
                GameManager.instance.GetSelectedNPC().InspectEvidence(cardData);
                isEvidenceShowed = true;
                PlayingCardManager.instance.SetIsDragging(false);
                return;
            }

            if (item.transform.TryGetComponent(out CardSlot _cardSlot))
            {
                if (_cardSlot.GetSlotType() != cardData.Type) return;

                status = CardStatus.CardSlot;

                if (_cardSlot.transform.childCount > 0)
                {
                    _cardSlot.transform.GetChild(0).transform.position = transform.position;
                    _cardSlot.transform.GetChild(0).GetComponent<PlayingCard>().SetCardStatus(CardStatus.Hand);
                    PlayingCardManager.instance.PlaceCardBack();
                }

                PlayingCardManager.instance.AddCardToSlot(this.transform);
                transform.SetParent(_cardSlot.transform);
                Vector3 pos = _cardSlot.transform.position + transform.forward * 0.02f;
                StartCoroutine(LerpCard(pos, false));
            }
        }

    }
}
