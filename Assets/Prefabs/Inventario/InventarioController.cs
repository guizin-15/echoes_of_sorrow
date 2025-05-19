using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventarioController : MonoBehaviour
{
    [Header("Root da UI (tagged InventoryUI)")]
    [SerializeField] private GameObject inventoryRoot;

    [Header("Botão e Descrição")]
    [SerializeField] private Button equipButton;
    [SerializeField] private DescriptionPanel descPanel;

    [Header("Botão Remover")]
    [SerializeField] private Button removeButton;


    private ItemSlot[] inventorySlots;
    private ItemSlot[] consumeSlots;

    private ItemSlot selectedInventorySlot;
    private ItemSlot selectedConsumeSlot;

    void Awake()
    {
        if (InventoryManager.Instance == null)
            Debug.LogError("Não achei InventoryManager na cena!");

        // já popula os fields de slots
        inventorySlots = inventoryRoot
            .GetComponentsInChildren<ItemSlot>(true)
            .Where(s => s.kind == ItemSlot.Kind.Inventory)
            .ToArray();
        consumeSlots = inventoryRoot
            .GetComponentsInChildren<ItemSlot>(true)
            .Where(s => s.kind == ItemSlot.Kind.Consume)
            .ToArray();

        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        equipButton.interactable = false;

        inventoryRoot.SetActive(false);

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnRemoveButtonClicked);
            removeButton.interactable = false;
        }
    }

    private void UpdateRemoveButton()
    {
        // só ativa se houver um slot de inventário selecionado
        removeButton.interactable = selectedInventorySlot != null;
    }

    /// <summary> Chamado pelo InventoryManager para abrir/fechar UI </summary>
    public void ToggleInventory()
    {
        bool open = !inventoryRoot.activeSelf;
        inventoryRoot.SetActive(open);
        if (open)
            RefreshSlots();
        else
            DeselectAll();
    }

    public void RefreshSlots()
    {
        var cards    = InventoryManager.Instance.InventoryCards;
        var equipped = InventoryManager.Instance.EquippedCards;

        // Atualiza os arrays de slots DOS FIELDS, não locais
        inventorySlots = inventoryRoot
            .GetComponentsInChildren<ItemSlot>(true)
            .Where(s => s.kind == ItemSlot.Kind.Inventory)
            .ToArray();
        consumeSlots = inventoryRoot
            .GetComponentsInChildren<ItemSlot>(true)
            .Where(s => s.kind == ItemSlot.Kind.Consume)
            .ToArray();

        // limpa e preenche inventário
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].Clear();
            if (i < cards.Count)
                inventorySlots[i].AddItem(cards[i].cardName, cards[i].artwork, cards[i].description, cards[i]);
        }

        // limpa e preenche slots de consumíveis
        for (int i = 0; i < consumeSlots.Length; i++)
        {
            consumeSlots[i].Clear();
            if (i < equipped.Length && equipped[i] != null)
                consumeSlots[i].AddItem(equipped[i].cardName, equipped[i].artwork, equipped[i].description, equipped[i]);
        }
    }

    public void SelectSlot(ItemSlot slot)
{
    // 1) Se for um slot de inventário, desmarca só o inventory antigo
    if (slot.kind == ItemSlot.Kind.Inventory)
    {
        selectedInventorySlot?.SetSelected(false);
        selectedInventorySlot = slot;
    }
    // 2) Se for um slot de consumo, desmarca só o consume antigo
    else
    {
        selectedConsumeSlot?.SetSelected(false);
        selectedConsumeSlot = slot;
    }

    // 3) Marca o novo slot
    slot.SetSelected(true);
    
    // 4) Atualiza painel de descrição e botão
    descPanel.Show(slot);
    equipButton.interactable = selectedInventorySlot != null
                          && selectedConsumeSlot != null;
    UpdateRemoveButton();
}


    public void OnEquipButtonClicked()
    {
        if (selectedInventorySlot == null || selectedConsumeSlot == null) return;
        var data = selectedInventorySlot.storedCard;

        int idx = System.Array.IndexOf(consumeSlots, selectedConsumeSlot);
        InventoryManager.Instance.EquipCard(idx, data);

        data.ApplyEffect(FindObjectOfType<PlayerController2D>());

        RefreshSlots();
        DeselectAll();
        UpdateRemoveButton();
    }

        private void OnRemoveButtonClicked()
    {
        if (selectedInventorySlot == null)
            return;

        // obtém o CardData a remover
        var toRemove = selectedInventorySlot.storedCard;
        if (toRemove != null)
        {
            // remove da lista persistente
            InventoryManager.Instance.InventoryCards.Remove(toRemove);
        }

        // limpa seleção e repopula UI
        DeselectAll();
        RefreshSlots();

        // desabilita o próprio botão
        UpdateRemoveButton();
    }


    

    private void DeselectAll()
    {
        foreach (var s in inventorySlots) s.SetSelected(false);
        foreach (var s in consumeSlots)   s.SetSelected(false);
        selectedInventorySlot = selectedConsumeSlot = null;
        descPanel.Show(null);
        equipButton.interactable = false;
    }
}
