using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlot : MonoBehaviour
{
    [SerializeField] private CardType deckType;

    public CardType GetSlotType() { return deckType; }

}
