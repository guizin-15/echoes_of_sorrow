using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic; 


public class InventarioController : MonoBehaviour
{
    public static InventarioController Instance { get; private set; }

    [Header("Inventory UI Prefab")]
    [Tooltip("Prefab containing the inventory UI (root GameObject) with slots configured as children.")]
    [SerializeField] private GameObject inventoryPrefab;
    
    [Header("Gameplay References")]
    [SerializeField] private PlayerController2D playerController;

    private GameObject inventoryRoot;

    // Persist equipped cards per consume slot index
    private CardData[] equippedCards;
    private DescriptionPanel descPanel;
    private Button equipButton;

    private ItemSlot[] inventorySlots;
    private ItemSlot[] consumeSlots;

    private ItemSlot selectedInventorySlot;
    private ItemSlot selectedConsumeSlot;
    private bool    isInventoryOpen;
    private List<CardData> inventoryCards;


    private void Awake()
{
    // Singleton setup
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);

    // Inicializa a “base” do inventário
    inventoryCards = new List<CardData>();

    // Instancia e configura a UI de inventário
    if (inventoryPrefab == null)
    {
        Debug.LogError("[InventarioController] inventoryPrefab não atribuído!");
        return;
    }
    inventoryRoot = Instantiate(inventoryPrefab);
    DontDestroyOnLoad(inventoryRoot);
    inventoryRoot.SetActive(false);

    // Encontra o painel de descrição e o botão de equipar
    descPanel = inventoryRoot.GetComponentInChildren<DescriptionPanel>();
    equipButton = inventoryRoot
        .GetComponentsInChildren<Button>(true)
        .FirstOrDefault(b => b.name.ToLower().Contains("Button"));

    if (descPanel == null)
        Debug.LogError("[InventarioController] DescriptionPanel não encontrado no prefab");
    if (equipButton == null)
        Debug.LogError("[InventarioController] Botão de equipar não encontrado no prefab");

    // Associa listener ao botão de equipar
    if (equipButton != null)
    {
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(EquipSelectedItem);
        equipButton.interactable = false;
    }

    // Sempre que uma cena for carregada, atualiza slots e esconde UI
    SceneManager.sceneLoaded += OnSceneLoaded;

    // Inicializa slots e array de consumíveis
    RefreshSlots();
    equippedCards = new CardData[consumeSlots.Length];
    RefreshSlots();
}


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSlots();
        inventoryRoot.SetActive(false);
    }

private void RefreshSlots()
{
    // 1) Reobtenha referências aos slots
    inventorySlots = inventoryRoot
        .GetComponentsInChildren<ItemSlot>(true)
        .Where(s => s.kind == ItemSlot.Kind.Inventory)
        .ToArray();
    consumeSlots = inventoryRoot
        .GetComponentsInChildren<ItemSlot>(true)
        .Where(s => s.kind == ItemSlot.Kind.Consume)
        .ToArray();

    Debug.Log($"[InventarioController] Found {inventorySlots.Length} inventory and {consumeSlots.Length} consume slots", this);

    // 2) Limpa e repopula INVENTÁRIO
    foreach (var slot in inventorySlots)
        slot.Clear();
    for (int i = 0; i < inventoryCards.Count && i < inventorySlots.Length; i++)
    {
        var card = inventoryCards[i];
        inventorySlots[i].AddItem(
            card.cardName,
            card.artwork,
            card.description,
            card
        );
    }

    // 3) Limpa e repopula CONSUMÍVEIS (equipados)
    for (int i = 0; i < consumeSlots.Length; i++)
    {
        consumeSlots[i].Clear();
        if (equippedCards != null
            && i < equippedCards.Length
            && equippedCards[i] != null)
        {
            var card = equippedCards[i];
            consumeSlots[i].AddItem(
                card.cardName,
                card.artwork,
                card.description,
                card
            );
        }
    }
}


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryRoot.SetActive(isInventoryOpen);
        if (playerController != null)
            playerController.enabled = !isInventoryOpen;
        if (!isInventoryOpen)
            DeselectAll();

        Debug.Log($"[InventarioController] Inventory open={isInventoryOpen}");
    }

    public void AddItem(string name, Sprite sprite, string description)
    {
        foreach (var slot in inventorySlots)
            if (!slot.isFull) { slot.AddItem(name, sprite, description); return; }
        Debug.LogWarning("Inventário cheio!");
    }

    public void AddCard(CardData cardData)
{
    // guarda no “banco” antes de desenhar na UI
    inventoryCards.Add(cardData);

    // debug dos slots
    for (int i = 0; i < inventorySlots.Length; i++)
        Debug.Log($"[AddCard] slot[{i}] {inventorySlots[i].name} isFull={inventorySlots[i].isFull}");

    // tenta colocar no primeiro slot vazio
    foreach (var slot in inventorySlots)
    {
        if (!slot.isFull)
        {
            slot.AddItem(
                cardData.cardName,      // 1º argumento
                cardData.artwork,       // 2º argumento
                cardData.description,   // 3º argumento
                cardData               // 4º argumento
            );                         // ← aqui fecha o AddItem(...)
            return;
        }
    }

    Debug.LogWarning("Inventário cheio!");
}





    public void SelectSlot(ItemSlot slot)
    {
        if (slot.kind == ItemSlot.Kind.Inventory)
        {
            selectedInventorySlot?.SetSelected(false);
            selectedInventorySlot = slot;
            slot.SetSelected(true);
        }
        else
        {
            selectedConsumeSlot?.SetSelected(false);
            selectedConsumeSlot = slot;
            slot.SetSelected(true);
        }

        descPanel?.Show(slot);
        if (equipButton != null)
            equipButton.interactable = selectedInventorySlot != null && selectedConsumeSlot != null;
    }

    public void DeselectAll()
    {
        selectedInventorySlot?.SetSelected(false);
        selectedConsumeSlot?.SetSelected(false);
        selectedInventorySlot = selectedConsumeSlot = null;
        descPanel?.Show(null);
        if (equipButton != null)
            equipButton.interactable = false;
    }

    public void EquipSelectedItem()
    {
        Debug.Log($"[Equip] Inv={(selectedInventorySlot?.name)} Cons={(selectedConsumeSlot?.name)}", this);
        if (selectedInventorySlot == null || selectedConsumeSlot == null) return;
        if (!selectedInventorySlot.isFull) return;

        // Remove old effect
        selectedConsumeSlot.storedCard?.RemoveEffect(playerController);
        // Assign new
        selectedConsumeSlot.AddItem(
            selectedInventorySlot.itemName,
            selectedInventorySlot.itemSprite,
            selectedInventorySlot.description,
            selectedInventorySlot.storedCard);
        // Apply new effect
        selectedInventorySlot.storedCard?.ApplyEffect(playerController);
        // Persist equipped 
        int idx = System.Array.IndexOf(consumeSlots, selectedConsumeSlot);
        if (idx >= 0)
            equippedCards[idx] = selectedInventorySlot.storedCard;
        DeselectAll();
    }
}
