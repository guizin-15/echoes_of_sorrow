using UnityEngine;
using System.Linq;
using System.Collections.Generic;  // para List<T>

/// <summary>Gerencia slots, seleção, equipar e painel de descrição.</summary>
public class InventarioController : MonoBehaviour
{
    [Header("Referências na UI")]
    [SerializeField] private DescriptionPanel descPanel; // painel à direita
    
    [Header("Slots de inventário (6)")]
    public ItemSlot[] inventorySlots;

    [Header("Slots de consumo (3)")]
    public ItemSlot[] consumeSlots;

    // slots atualmente selecionados
    private ItemSlot selectedInventorySlot;
    private ItemSlot selectedConsumeSlot;

    // =====================================================================
    #region  Adicionar item (chamado por Item.cs)
        private void Awake()
    {
        // pega todos os ItemSlot filhos deste objeto (ou mude o caminho conforme sua hierarquia)
        inventorySlots = GetComponentsInChildren<ItemSlot>(true)
            .Where(s => s.kind == ItemSlot.Kind.Inventory)
            .ToArray();

    }

    public void AddItem(string nome, Sprite sprite, string descricao)
    {
        foreach (var slot in inventorySlots)
        {
            if (!slot.isFull)
            {
                slot.AddItem(nome, sprite, descricao);
                return;
            }
        }
        Debug.LogWarning("Inventário cheio!");
    }

    #endregion
    // =====================================================================
    #region  Seleção de slots

    public void SelectSlot(ItemSlot slot)
    {
        if (slot.kind == ItemSlot.Kind.Inventory)
        {
            // desmarca o inventário antigo
            if (selectedInventorySlot)
                selectedInventorySlot.SetSelected(false);

            // marca o novo inventário
            selectedInventorySlot = slot;
            selectedInventorySlot.SetSelected(true);
        }
        else // Kind.Consume
        {
            // desmarca o consumível antigo
            if (selectedConsumeSlot)
                selectedConsumeSlot.SetSelected(false);

            // marca o novo consumível
            selectedConsumeSlot = slot;
            selectedConsumeSlot.SetSelected(true);
        }

        // atualiza o painel de descrição
        descPanel.Show(selectedInventorySlot ?? selectedConsumeSlot);
    }



    public void DeselectAll()
    {
        if (selectedInventorySlot) selectedInventorySlot.SetSelected(false);
        if (selectedConsumeSlot)   selectedConsumeSlot.SetSelected(false);

        selectedInventorySlot = null;
        selectedConsumeSlot   = null;

        descPanel.Show(null); // limpa painel
    }

    #endregion
    // =====================================================================
    #region  Equipar

    /// <summary>Chamado no botão “EQUIPAR”.</summary>
    public void EquipSelectedItem()
    {
        if (selectedInventorySlot == null || selectedConsumeSlot == null)
        {
            Debug.LogWarning("Selecione um slot de inventário e um de consumo.");
            return;
        }

        if (!selectedInventorySlot.isFull)
        {
            Debug.LogWarning("O slot de inventário selecionado está vazio.");
            return;
        }

        // copia o item para o slot de consumo
        selectedConsumeSlot.AddItem(
            selectedInventorySlot.itemName,
            selectedInventorySlot.itemSprite,
            selectedInventorySlot.description);
        // selectedConsumeSlot.storedCard.ApplyEffect(playerStats);   APLICA A CONSUMO VER MECANICA COM PLAYER

        DeselectAll();
    }

public void AddCard(CardData cardData)
{
    // 1) LOGA o estado real de cada slot antes de qualquer coisa
    for (int i = 0; i < inventorySlots.Length; i++)
        Debug.Log($"[AddCard] slot[{i}] {inventorySlots[i].name}  isFull={inventorySlots[i].isFull}");

    // 2) Tenta inserir normalmente
    foreach (var slot in inventorySlots)
    {
        if (!slot.isFull)
        {
            Debug.Log($"[AddCard] Inserindo em {slot.name}");
            slot.AddItem(cardData.cardName,
                         cardData.artwork,
                         cardData.description,
                         cardData);
            return;
        }
    }
    Debug.LogWarning("Inventário cheio!");
}





    #endregion
}
