using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public enum CardEffect
{
    Health,
    Damage,
    SpeedBoost
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

    [Header("Parâmetros do Efeito")]
    public int    amount   = 1;    // valor base do bônus
    public float  duration = 5f;   // **não usado**, efeito agora é permanente

    public void ApplyEffect(PlayerController2D pc)
    {
        Debug.Log($"[CardData] ApplyEffect → Carta '{cardName}' (effect={effect}, amount={amount})", pc);
        switch (effect)
        {
            case CardEffect.Health:
                pc.maxHealth     += amount;
                pc.currentHealth  = Mathf.Min(pc.currentHealth + amount, pc.maxHealth);
                if (pc.healthBar) pc.healthBar.SetMaxHealth(pc.maxHealth);
                Debug.Log($"   → Health aplicado: maxHealth={pc.maxHealth}, currentHealth={pc.currentHealth}", pc);
                break;

            case CardEffect.Damage:
                ModifyPrivateFloat(pc, "slashCooldown",  -amount * 0.05f);
                ModifyPrivateFloat(pc, "sliceCooldown",  -amount * 0.05f);
                break;

            case CardEffect.SpeedBoost:
                ModifyPrivateFloat(pc, "runMaxSpeed", amount);
                Debug.Log($"   → SpeedBoost aplicado: runMaxSpeed aumentado em {amount}", pc);
                break;
        }
    }

    public void RemoveEffect(PlayerController2D pc)
    {
        Debug.Log($"[CardData] RemoveEffect → Carta '{cardName}' (effect={effect}, amount={amount})", pc);
        switch (effect)
        {
            case CardEffect.Health:
                pc.maxHealth     = Mathf.Max(1, pc.maxHealth - amount);
                pc.currentHealth = Mathf.Min(pc.currentHealth, pc.maxHealth);
                if (pc.healthBar) pc.healthBar.SetMaxHealth(pc.maxHealth);
                Debug.Log($"   → Health removido: maxHealth={pc.maxHealth}, currentHealth={pc.currentHealth}", pc);
                break;

            case CardEffect.Damage:
                ModifyPrivateFloat(pc, "slashCooldown",  amount * 0.05f);
                ModifyPrivateFloat(pc, "sliceCooldown",  amount * 0.05f);
                break;

            case CardEffect.SpeedBoost:
                ModifyPrivateFloat(pc, "runMaxSpeed", -amount);
                Debug.Log($"   → SpeedBoost removido: runMaxSpeed reduzido em {amount}", pc);
                break;
        }
    }

    // helper para alterar campos privados do PlayerController2D
    private void ModifyPrivateFloat(object target, string fieldName, float delta)
    {
        var fi = target.GetType()
                       .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (fi != null && fi.FieldType == typeof(float))
        {
            float old = (float)fi.GetValue(target);
            float neu = old + delta;
            fi.SetValue(target, neu);
            Debug.Log($"   → {fieldName}: {old} → {neu}", target as UnityEngine.Object);
        }
        else
        {
            Debug.LogWarning($"[CardData] Field '{fieldName}' não encontrado em {target.GetType().Name}", this);
        }
    }
}
