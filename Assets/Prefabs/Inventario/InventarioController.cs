using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

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

        // Instantiate inventory UI prefab if needed
        if (inventoryPrefab == null)
        {
            Debug.LogError("[InventarioController] inventoryPrefab is not assigned!");
            return;
        }
        inventoryRoot = Instantiate(inventoryPrefab);
        DontDestroyOnLoad(inventoryRoot);
        inventoryRoot.SetActive(false);

        // Find references inside the instantiated UI
        descPanel   = inventoryRoot.GetComponentInChildren<DescriptionPanel>();
        equipButton = inventoryRoot.GetComponentsInChildren<Button>(true)
    .FirstOrDefault(b => b.name.ToLower().Contains("equip"));

        if (descPanel == null)   Debug.LogError("[InventarioController] DescriptionPanel not found in inventoryPrefab");
        if (equipButton == null) Debug.LogError("[InventarioController] Equip Button not found in inventoryPrefab");

        // Setup button listener
        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(EquipSelectedItem);
            equipButton.interactable = false;
        }

        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Initial slot refresh and equip array init
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
        // Find slots under the inventoryRoot to avoid stale references
        inventorySlots = inventoryRoot.GetComponentsInChildren<ItemSlot>(true)
            .Where(s => s.kind == ItemSlot.Kind.Inventory)
            .ToArray();
        consumeSlots   = inventoryRoot.GetComponentsInChildren<ItemSlot>(true)
            .Where(s => s.kind == ItemSlot.Kind.Consume)
            .ToArray();

        Debug.Log($"[InventarioController] Found {inventorySlots.Length} inventory and {consumeSlots.Length} consume slots", this);

        // Clear inventory slots
        foreach (var slot in inventorySlots)
            slot.Clear();

        // Clear consume slots then reapply equipped
        for (int i = 0; i < consumeSlots.Length; i++)
        {
            consumeSlots[i].Clear();
            if (equippedCards != null && i < equippedCards.Length && equippedCards[i] != null)
            {
                var cardData = equippedCards[i];
                consumeSlots[i].AddItem(
                    cardData.cardName,
                    cardData.artwork,
                    cardData.description,
                    cardData);
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
        for (int i = 0; i < inventorySlots.Length; i++)
            Debug.Log($"[AddCard] slot[{i}] {inventorySlots[i].name} isFull={inventorySlots[i].isFull}");
        foreach (var slot in inventorySlots)
            if (!slot.isFull)
            {
                slot.AddItem(cardData.cardName, cardData.artwork, cardData.description, cardData);
                return;
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
