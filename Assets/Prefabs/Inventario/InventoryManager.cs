using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Configuração de UI")]
    [SerializeField] private GameObject inventoryPrefab; 
    private GameObject inventoryRoot;
    private InventarioController uiController;
    private PlayerController2D   playerController;
    private bool                 isOpen;      // <— nosso estado único

    // Persistência de dados
    public List<CardData> InventoryCards { get; private set; }
    public CardData[]     EquippedCards  { get; private set; }

    [SerializeField] private int consumeSlotCount = 3;

    void Awake()
{
    // ——— Singleton ———
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);

    // ——— Inicializa dados ———
    InventoryCards = new List<CardData>();
    EquippedCards  = new CardData[consumeSlotCount];

    if (inventoryPrefab == null)
    {
        Debug.LogError("[InventoryManager] inventoryPrefab não foi atribuído!");
        return;
    }
    inventoryRoot = Instantiate(inventoryPrefab);
    inventoryRoot.tag = "InventoryUI";
    DontDestroyOnLoad(inventoryRoot);
    inventoryRoot.SetActive(false);

    uiController = inventoryRoot.GetComponent<InventarioController>();
    if (uiController == null)
        Debug.LogError("[InventoryManager] InventarioController não encontrado no prefab instanciado!");

    // ——— Localiza o jogador para bloquear controles ———
    var playerGO = GameObject.FindWithTag("Player");
    if (playerGO != null)
        playerController = playerGO.GetComponent<PlayerController2D>();
    if (playerController == null)
        Debug.LogWarning("[InventoryManager] PlayerController2D não encontrado em objeto com tag 'Player'!");
}

    public void ToggleUI()
    {
        // Inverte o estado interno
        isOpen = !isOpen;

        // Reflete na UI
        inventoryRoot.SetActive(isOpen);

        // Pausa / despausa
        Time.timeScale = isOpen ? 0f : 1f;

        // Bloqueia / libera player
        if (playerController != null)
            playerController.enabled = !isOpen;

        // Atualiza ou limpa
        if (isOpen)
            uiController.RefreshSlots();
        else
            uiController.DeselectAll();
    }

        void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleUI();
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
