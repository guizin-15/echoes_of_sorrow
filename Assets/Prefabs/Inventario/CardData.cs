using UnityEngine;

public enum CardEffect
{
    Heal,
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
