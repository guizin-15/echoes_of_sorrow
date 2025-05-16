using UnityEngine;

public enum CardEffect
{
    Health,
    Damage,
    SpeedBoost,
    Shield
}

[CreateAssetMenu(
    fileName = "CardData",
    menuName = "Cards/Nova Carta",  
    order    = 0)]
public class CardData : ScriptableObject
{
    public string     cardName;
    [TextArea] public string description;
    public Sprite     artwork;
    public CardEffect effect;
}
