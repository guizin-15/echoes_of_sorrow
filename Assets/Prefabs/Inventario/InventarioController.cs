using UnityEngine;
using System.Linq;
using System.Collections.Generic;  // para List<T>

/// <summary>Gerencia slots, seleção, equipar e painel de descrição.</summary>
public class InventarioController : MonoBehaviour
{
    [Header("Referências na UI")]
    [SerializeField] private DescriptionPanel descPanel; // painel à direita

    [Header("Referências de Jogo")]
    [SerializeField] private PlayerController2D playerController;
    private CardData currentEquippedCard;
    
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
        // 1) localiza todos os slots de cada tipo em toda a cena
        inventorySlots = FindObjectsOfType<ItemSlot>()
            .Where(s => s.kind == ItemSlot.Kind.Inventory)
            .ToArray();

        consumeSlots = FindObjectsOfType<ItemSlot>()
            .Where(s => s.kind == ItemSlot.Kind.Consume)
            .ToArray();

        Debug.Log($"Achei {inventorySlots.Length} slots de inventário e {consumeSlots.Length} slots de consumo", this);

        // 2) limpa TODOS os slots logo de cara
        foreach (var inv in inventorySlots)
            inv.Clear();    // inv é cada ItemSlot de inventário

        foreach (var con in consumeSlots)
            con.Clear();    // con é cada ItemSlot de consumo
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
    Debug.Log($"[SelectSlot] clicou em {slot.name} (kind={slot.kind})", this);

    if (slot.kind == ItemSlot.Kind.Inventory)
    {
        if (selectedInventorySlot != null)
            selectedInventorySlot.SetSelected(false);
        selectedInventorySlot = slot;
        selectedInventorySlot.SetSelected(true);
    }
    else
    {
        if (selectedConsumeSlot != null)
            selectedConsumeSlot.SetSelected(false);
        selectedConsumeSlot = slot;
        selectedConsumeSlot.SetSelected(true);
    }

    Debug.Log($"   ▶ selecionados → inv: {selectedInventorySlot?.name} | cons: {selectedConsumeSlot?.name}", this);
    descPanel.Show(slot);
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
    Debug.Log($"[Equip] Inv={(selectedInventorySlot?.name)} | Cons={(selectedConsumeSlot?.name)}", this);

    // validações
    if (selectedInventorySlot == null || selectedConsumeSlot == null)
    {
        Debug.LogWarning("Selecione um slot de inventário e um de consumo.", this);
        return;
    }
    if (!selectedInventorySlot.isFull)
    {
        Debug.LogWarning("O slot de inventário selecionado está vazio.", this);
        return;
    }

    // 1) Remove efeito da carta previamente equipada NESTE slot, se houver
    var oldCard = selectedConsumeSlot.storedCard;
    if (oldCard != null)
    {
        Debug.Log($"[Equip] Removendo efeito da carta antiga '{oldCard.cardName}' do slot {selectedConsumeSlot.name}", this);
        oldCard.RemoveEffect(playerController);
    }

    // 2) Copia a nova carta para o slot de consumo
    selectedConsumeSlot.AddItem(
        selectedInventorySlot.itemName,
        selectedInventorySlot.itemSprite,
        selectedInventorySlot.description,
        selectedInventorySlot.storedCard
    );

    // 3) Aplica o efeito da nova carta
    var newCard = selectedInventorySlot.storedCard;
    if (newCard != null)
    {
        Debug.Log($"[Equip] Aplicando efeito da nova carta '{newCard.cardName}' no slot {selectedConsumeSlot.name}", this);
        newCard.ApplyEffect(playerController);
    }
    else
    {
        Debug.LogError("[Equip] storedCard é null – nada a aplicar!", this);
    }

    // 4) Limpa seleção visual
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
