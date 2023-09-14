using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/PlayingCard", order = 1)]
public class PlayCardsSObject : ScriptableObject
{
    [SerializeField] private int itemID;

    [SerializeField] private CardType type;
    [SerializeField] private Sprite image;
    [SerializeField] private string title;
    [TextArea(10, 10)]
    [SerializeField] private string description;

    public int ItemID { get { return itemID; } }
    public CardType Type { get { return type; } }
    public string Title { get { return title; } }
    public string Description { get { return description; } }
    public Sprite Image { get { return image; } }
}