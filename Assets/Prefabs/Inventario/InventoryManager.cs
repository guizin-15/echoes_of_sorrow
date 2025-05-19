using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Configuração de UI")]
    [Tooltip("O root do seu inventário (tagged InventoryUI)")]
    [SerializeField] private GameObject inventoryRoot;

    private InventarioController uiController;
    private bool                 isOpen;

    // Persistência de dados
    public List<CardData> InventoryCards { get; private set; }
    public CardData[]     EquippedCards  { get; private set; }

    [SerializeField] private int consumeSlotCount = 3;

    void Awake()
    {
        // singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // init dados
        InventoryCards = new List<CardData>();
        EquippedCards  = new CardData[consumeSlotCount];

        // encontra a UI e o controller
        if (inventoryRoot == null)
            inventoryRoot = GameObject.FindWithTag("InventoryUI");

        if (inventoryRoot == null)
            Debug.LogError("[InventoryManager] inventoryRoot não atribuído nem encontrado por tag!");

        inventoryRoot.SetActive(false);
        DontDestroyOnLoad(inventoryRoot);

        uiController = inventoryRoot.GetComponent<InventarioController>();
        if (uiController == null)
            Debug.LogError("[InventoryManager] InventarioController não encontrado no inventoryRoot!");
    }

    void Update()
    {
        // aqui tratamos o input “I” para abrir/fechar
        if (Input.GetKeyDown(KeyCode.I) && inventoryRoot != null)
        {
            isOpen = !isOpen;
            inventoryRoot.SetActive(isOpen);

            // quando abrir, força a UI a recarregar
            if (isOpen) uiController.RefreshSlots();
        }
    }

    // métodos de dados (chamados pelo ShopMenu, CardPickup etc)
    public void AddCard(CardData card)
    {
        if (card != null)
            InventoryCards.Add(card);
    }

    public void EquipCard(int slotIndex, CardData card)
    {
        if (slotIndex >= 0 && slotIndex < EquippedCards.Length)
            EquippedCards[slotIndex] = card;
    }
}
