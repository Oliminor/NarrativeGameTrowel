using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CardStatus
{
    Hand = 0,
    CloseUp = 1,
    CardSlot = 2,
    Drag = 3
}

public enum CardType
{
    None = 0,
    Character = 1,
    Motive = 2,
    Weapon = 3,
    All = 4
}

public class PlayingCardManager : MonoBehaviour
{
    public static PlayingCardManager instance;

    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private Transform characterPosition;
    [SerializeField] private Transform weaponPosition;
    [SerializeField] private Transform motivePosition;

    [SerializeField] private Transform fanOutCardsPosition;
    [SerializeField] private Transform closeUpCardPosition;
    [SerializeField] private Transform cardSlots;
    [SerializeField] private Transform evidenceSlot;
    [SerializeField] private Transform selectedCardsOnTablePositions;

    [Header("Card Sound")]
    [SerializeField] AudioSource cardAudioSource;
    [SerializeField] AudioClip cardAddAudio;
    [SerializeField] float playbackVolume = 1f;

    public List<PlayCardsSObject> cardList = new();
    private List<Transform> characterList = new();
    private List<Transform> weaponList = new();
    private List<Transform> motiveList = new();
    private List<Transform> allCardList = new();

    private Transform[] slotCards = new Transform[3];

    private List<Vector3> cardPositions = new();
    private List<float> cardRotationZ = new();


    private int testIndex = 0;
    private float cardThickness;

    private bool isCardFaningDone = true;
    private bool isDragging = false;

    private Transform currentSelectedCard;
    private Transform tempCurrentSelectedCard;

    private CardType currentCardType;

    private IEnumerator fanOutCards;

    public CardType GetCurrentCardType() { return currentCardType; }
    public Transform GetFanOutCardsPosition() { return fanOutCardsPosition; }
    public Transform GetCloseUpCardPosition() { return closeUpCardPosition; }
    public bool IsCardFaningDone() { return isCardFaningDone; }
    public bool IsDragging() { return isDragging; }
    public void SetIsDragging(bool _bool) { isDragging = _bool; }
    public int CurrentCardNumber() {
        currentCardType = CardType.All;
        return ReturnCurrentCardList().Count;
            }

    void Awake()
    {
        if (instance != null) Debug.Log("Error: There are multiple instances exits at the same time (PlayingCardManager)");
        instance = this;
    }

    private void Start()
    {
        cardSlots.gameObject.SetActive(false);
        evidenceSlot.gameObject.SetActive(false);
        cardThickness = cardPrefab.GetComponent<MeshRenderer>().bounds.size.z;

        //LoadCardsForTesting();
        //for (int i = 0; i < 20; i++) AddCardTest();
    }

    private void Update()
    {
        if (GameManager.instance.GetStatus() != GameManager.GameStatus.PlayingCard &&
           GameManager.instance.GetStatus() != GameManager.GameStatus.InspectEvidence) return;

        SelectCard();
    }

    private void SelectCard()
    {
        if (ReturnCurrentCardList().Count == 0) return;

        currentSelectedCard = ClosestCardToTheMouse();

        Vector2 mousePosition = GameManager.instance.GetInputs().GameInput.MousePosition.ReadValue<Vector2>();

        if (mousePosition.y > Screen.height / 3.6f || mousePosition.x < Screen.width / 5.5f || mousePosition.x > Screen.width - Screen.width / 5.5f)
        {
            if(tempCurrentSelectedCard) tempCurrentSelectedCard.GetComponent<PlayingCard>().MouseExit();
            return;
        }

        if (!IsCardFaningDone()) return;

        currentSelectedCard.GetComponent<PlayingCard>().MouseEnter();

        if (tempCurrentSelectedCard == currentSelectedCard) return;

        if (tempCurrentSelectedCard) tempCurrentSelectedCard.GetComponent<PlayingCard>().MouseExit();

        tempCurrentSelectedCard = currentSelectedCard;
    }

    private Transform ClosestCardToTheMouse()
    {
        float distanceFromCamera = Vector3.Distance(ReturnCurrentCardList()[ReturnCurrentCardList().Count / 2].position, Camera.main.transform.position);
        Vector2 mousePosition = GameManager.instance.GetInputs().GameInput.MousePosition.ReadValue<Vector2>();
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceFromCamera));
        float distance = Mathf.Infinity;

        int index = 0;

        for (int i = 0; i < ReturnCurrentCardList().Count; i++)
        {
            if (ReturnCurrentCardList()[i].GetComponent<PlayingCard>().GetCardStatus() != CardStatus.Hand) continue;

            Vector2 card2DPos = new Vector2(ReturnCurrentCardList()[i].GetComponent<PlayingCard>().getTempStartPos().x, 0);
            Vector2 mousePos = mouseWorldPosition;
            float currentDistance = Vector2.Distance(card2DPos, mousePos) * 1000000;

            if (currentDistance < distance)
            {
                distance = currentDistance;
                index = i;
            }
        }

        return ReturnCurrentCardList()[index];
    }

    private void AddCardTest()
    {
        AddCard(cardList[testIndex]);
        testIndex++;

        if (testIndex > cardList.Count - 1) testIndex = 0;
    }

    private void LoadCardsForTesting()
    {
        Object[] Items = Resources.LoadAll("Cards", typeof(PlayCardsSObject));
        for (int i = 0; i < 3; i++)
        {
            foreach (PlayCardsSObject item in Items)
            {
                Instantiate(item);
                cardList.Add(item);
            }
        }
    }

    public List<Transform> ReturnCurrentCardList()
    {
        switch (currentCardType)
        {
            case CardType.Character:
                return characterList;

            case CardType.Motive:
                return motiveList;

            case CardType.Weapon:
                return weaponList;

            case CardType.All:
                return allCardList;

            default:
                return null;
        }
    }

    public void CloseCardMenu()
    {
        cardSlots.gameObject.SetActive(false);
        evidenceSlot.gameObject.SetActive(false);
        if (ReturnCurrentCardList() != null) SetCardsBack();
    }

    public void InteractWithDeck(CardType _type)
    {
        if (ReturnCurrentCardList() != null) SetCardsBack();

        currentCardType = _type;

        if (_type == CardType.All)
        {
            allCardList = allCardList.OrderBy(x => x.GetComponent<PlayingCard>().GetCardData().Type).ToList();
            if (GameManager.instance.GetStatus() == GameManager.GameStatus.InspectEvidence) evidenceSlot.gameObject.SetActive(true);
        }

        //if (GameManager.instance.GetStatus() == GameManager.GameStatus.PlayingCard) cardSlots.gameObject.SetActive(true);

        PlaceCardsToHand();
        LoadCards();
        SetCardTypes();
        CheckCloseUpCards();
        RearrangeCards();
        FanOutCards(true);

        currentSelectedCard = ReturnCurrentCardList()[0];
        if(ReturnCurrentCardList().Count > 1) tempCurrentSelectedCard = ReturnCurrentCardList()[1];
    }

    private void SetCardTypes()
    {
        foreach (var card in ReturnCurrentCardList())
        {
            if (GameManager.instance.GetStatus() == GameManager.GameStatus.InspectEvidence) card.GetComponent<PlayingCard>().SetType(CardType.All);
            else card.GetComponent<PlayingCard>().SetType(card.GetComponent<PlayingCard>().GetCardData().Type);
        }

    }

    public void PlaceCardBack()
    {
        RearrangeCards();
        FanOutCards(false);
    }

    private Transform GetParentDeckByType(CardType _type)
    {
        Transform _parent = transform;

        switch (_type)
        {
            case CardType.Character:
                _parent = characterPosition;
                break;
            case CardType.Weapon:
                _parent = weaponPosition;
                break;
            case CardType.Motive:
                _parent = motivePosition;
                break;
        }

        return _parent;
    }

    public void SetCardsBack()
    {
        SetCardTypes();

        int counter = 0;

        foreach (var card in ReturnCurrentCardList())
        {
            card.GetComponent<PlayingCard>().SetIsCardHovered(false);
            card.GetComponent<PlayingCard>().OutlineToggleOff();
            if (card.GetComponent<PlayingCard>().GetCardStatus() == CardStatus.CardSlot) continue;

            CardType listCardType = card.GetComponent<PlayingCard>().GetCardType();

            Transform _parent = GetParentDeckByType(listCardType);

            card.SetParent(_parent);

            card.transform.localPosition = Vector3.zero;
            card.transform.rotation = Quaternion.Euler(new Vector3(90, 180, _parent.eulerAngles.y + Random.Range(-10.0f, 10.0f)));

            if (counter > 0) card.transform.localPosition = ReturnCurrentCardList()[counter - 1].localPosition + new Vector3(0, cardThickness, 0);

            counter++;
        }
    }

    public void CheckCloseUpCards()
    {
        foreach (var card in ReturnCurrentCardList())
        {
            if (card.GetComponent<PlayingCard>().GetCardStatus() == CardStatus.CloseUp) card.GetComponent<PlayingCard>().SetCardStatus(CardStatus.Hand);
            if (card.GetComponent<PlayingCard>().GetCardStatus() == CardStatus.Drag) card.GetComponent<PlayingCard>().SetCardStatus(CardStatus.Hand);
        }
    }

    private void PlaceCardsToHand()
    {
        foreach (var card in ReturnCurrentCardList())
        {
            if (card.GetComponent<PlayingCard>().GetCardStatus() == CardStatus.CardSlot) continue;
            if (card.GetComponent<PlayingCard>().GetCardStatus() != CardStatus.Hand) card.GetComponent<PlayingCard>().SetCardStatus(CardStatus.Hand);
            card.SetParent(null);
            card.SetParent(fanOutCardsPosition);
        }
    }

    private void LoadCards()
    {
        if (GameManager.instance.GetStatus() == GameManager.GameStatus.PlayingCard) return;

        for (int i = 0; i < GameManager.instance.GetSelectedNPC().GetPlayingCardsOnSlot().Length; i++)
        {
            foreach (var card in ReturnCurrentCardList())
            {
                if (card.GetComponent<PlayingCard>().GetCardData() == GameManager.instance.GetSelectedNPC().GetPlayingCardsOnSlot()[i])
                {
                    if (cardSlots.GetChild(i).childCount != 0) continue;
                    card.GetComponent<PlayingCard>().SetCardStatus(CardStatus.CardSlot);
                    card.SetParent(cardSlots.GetChild(i));
                    card.position = cardSlots.GetChild(i).transform.position + transform.forward * 0.02f;
                    card.localRotation = Quaternion.identity;
                }
            }
        }
    }

    private void RearrangeCards()
    {
        foreach (var card in ReturnCurrentCardList())
        {
            if (card.GetComponent<PlayingCard>().GetCardStatus() != CardStatus.Hand) continue;
            card.SetParent(null);
            card.SetParent(fanOutCardsPosition);
        }
    }

    private void FanOutCards(bool _deckInteraction)
    {
        cardPositions.Clear();
        cardRotationZ.Clear();

        float radius = 3;

        int cardsNumber = fanOutCardsPosition.childCount;
        float fromToAngle = 30;
        if (cardsNumber < 4) fromToAngle = cardsNumber * 10;

        float angleStep = fromToAngle / cardsNumber;
        float currentAngle = -(fromToAngle / 2) + 90 + (angleStep / 2);
        float startZ = 0;

        for (int i = 0; i < cardsNumber; i++)
        {
            float x = radius * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float y = radius * Mathf.Sin(currentAngle * Mathf.Deg2Rad);

            Vector3 position = new Vector3(x, y, startZ);

            if (_deckInteraction && i == 0) InstantiateDeckBeforeFanOut(position, currentAngle - 90);

            cardPositions.Add(position);
            cardRotationZ.Add(currentAngle - 90);

            currentAngle += angleStep;
            //startZ += cardThickness * 2.0f;
            startZ += 0.01f;
        }

        LerpCards();
    }

    private void InstantiateDeckBeforeFanOut(Vector3 _position, float _rotationZ)
    {
        float startZ = 0;
        for (int i = 0; i < fanOutCardsPosition.childCount; i++)
        {
            fanOutCardsPosition.GetChild(i).localPosition = _position + new Vector3(0, 0, startZ);
            fanOutCardsPosition.GetChild(i).localRotation = Quaternion.Euler(new Vector3(0, 0, _rotationZ));
            startZ += cardThickness * 1.5f;
        }
    }

    public void LerpCards()
    {
        if (fanOutCards != null) StopCoroutine(fanOutCards);
        fanOutCards = LerpCardsIE();
        StartCoroutine(fanOutCards);
    }

    IEnumerator LerpCardsIE()
    {
        isCardFaningDone = false;

        List<Vector3> startPositions = new();
        List<float> startRotation = new();

        for (int i = 0; i < fanOutCardsPosition.childCount; i++)
        {
            startPositions.Add(fanOutCardsPosition.GetChild(i).localPosition);
            startRotation.Add(fanOutCardsPosition.GetChild(i).localRotation.eulerAngles.z);
        }

        float lerp = 0;

        while (lerp < 1)
        {
            yield return new WaitForSeconds(0.01f);

            lerp += 0.035f;

            for (int i = 0; i < fanOutCardsPosition.childCount; i++)
            {
                fanOutCardsPosition.GetChild(i).localPosition = Vector3.Lerp(startPositions[i], cardPositions[i], lerp);
                fanOutCardsPosition.GetChild(i).localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.LerpAngle(startRotation[i], cardRotationZ[i], lerp)));
            }
        }

        for (int i = 0; i < fanOutCardsPosition.childCount; i++)
        {
            fanOutCardsPosition.GetChild(i).GetComponent<PlayingCard>().SetTempStartPos(fanOutCardsPosition.GetChild(i).position);
        }

        isCardFaningDone = true;
    }

    public void AddCard(PlayCardsSObject _playCards)
    {
        CardType type = _playCards.Type;
        GameObject go = Instantiate(cardPrefab);

        go.GetComponent<PlayingCard>().SetCardData(_playCards);

        foreach (var item in allCardList)
        {
            if (go.GetComponent<PlayingCard>().GetCardData().ToString() == item.GetComponent<PlayingCard>().GetCardData().ToString())
            {
                Destroy(go);
                return;
            }
        }

        allCardList.Add(go.transform);
        cardAudioSource.PlayOneShot(cardAddAudio, playbackVolume);

        switch (type)
        {
            case CardType.Character:
                SetCardPosition(ref go, ref characterList, characterPosition);
                break;
            case CardType.Weapon:
                SetCardPosition(ref go, ref weaponList, weaponPosition);
                break;
            case CardType.Motive:
                SetCardPosition(ref go, ref motiveList, motivePosition);
                break;
        }

        HUDManager.instance.NewCardObtainedAnimTrigger();
    }

    private void SetCardPosition(ref GameObject _card, ref List<Transform> _list, Transform _parent)
    {
        _list.Add(_card.transform);
        _card.transform.SetParent(_parent);
        _card.transform.localPosition = Vector3.zero;
        _card.transform.rotation = Quaternion.Euler(new Vector3(90, 180, _parent.eulerAngles.y + Random.Range(-10.0f, 10.0f)));

        if (_list.Count > 1) _card.transform.localPosition = _list[_list.Count - 2].localPosition + new Vector3(0, cardThickness, 0);
    }

    public void ToggleSlotOutline(bool _toggle, CardType _type)
    {
        for (int i = 0; i < cardSlots.childCount; i++)
        {
            if (cardSlots.GetChild(i).GetComponent<CardSlot>().GetSlotType() == _type)
            {
                GameManager.instance.ToggleOutline(cardSlots.GetChild(i).gameObject, _toggle);
            }
        }
    }

    public void RemoveCardFromSlot(int _index)
    {
        _index--;

        if (slotCards[_index] == null) return;

        slotCards[_index] = null;
        Destroy(selectedCardsOnTablePositions.GetChild(_index).GetChild(0).gameObject);
    }

    public void AddCardToSlot(Transform _card)
    {
        int typeIndex = (int)_card.GetComponent<PlayingCard>().GetCardData().Type - 1;
        slotCards[typeIndex] = _card;

        if (selectedCardsOnTablePositions.GetChild(typeIndex).childCount > 0) Destroy(selectedCardsOnTablePositions.GetChild(typeIndex).GetChild(0).gameObject);

        GameObject go = Instantiate(cardPrefab, selectedCardsOnTablePositions.GetChild(typeIndex));
        go.GetComponent<PlayingCard>().SetCardData(_card.GetComponent<PlayingCard>().GetCardData());
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.Euler(Vector3.zero);

        if (go.transform.TryGetComponent(out BoxCollider collider))
        {
            Destroy(collider);
        }
    }
}
