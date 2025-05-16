using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DescriptionPanel : MonoBehaviour
{
    [Header("Referências na UI")]
    [SerializeField] private Image     imagemItem;    
    [SerializeField] private TMP_Text  textoDescricao;

    /// <summary>
    /// Exibe informações do <paramref name="slot"/> ou limpa se for null/vazio.
    /// </summary>
    public void Show(ItemSlot slot)
    {
        if (slot == null || !slot.isFull)
        {
            imagemItem.enabled  = false;               // oculta imagem
            textoDescricao.text = string.Empty;
            return;
        }
        
        imagemItem.enabled  = true;
        imagemItem.sprite   = slot.itemSprite;

        // <b> deixa o nome em negrito
        textoDescricao.text = $"<b>{slot.itemName}</b>\n{slot.description}";
    }
}
