using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopMenu : MonoBehaviour
{
    [Header("UI Root")]
    public GameObject          shopPanel;

    [Header("Detalhes do Item")]
    public TMP_Text            nomeText;
    public TMP_Text            poderText;
    public TMP_Text            descricaoText;
    public Button              buyButton;

    [Header("Referências de Jogo")]
    public InventarioController inventoryController;  // seu script de inventário
    public PlayerController2D  playerController;     // script que guarda coinsCollected

    private ItemSlotLoja[]     allSlots;
    private ItemSlotLoja       selectedSlot;

    void Awake()
    {
        allSlots = FindObjectsOfType<ItemSlotLoja>();
    }

    void Start()
    {
        shopPanel.SetActive(false);
        buyButton.interactable = false;
        buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        ClearDetails();
        ClearSlotHighlights();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    public void SelectItem(ItemSlotLoja slot)
    {
        selectedSlot = slot;
        nomeText.text      = slot.cardData.cardName;
        poderText.text     = slot.cardData.effect.ToString();
        descricaoText.text = slot.cardData.description;
        buyButton.interactable = true;
    }

    public void ClearSlotHighlights()
    {
        foreach (var slot in allSlots)
            slot.Highlight(false);
    }

    private void OnBuyButtonClicked()
    {
        if (selectedSlot == null) return;
        playerController.coinsCollected = 1000;
        // usa coinsCollected como "dinheiro"
        if (playerController.coinsCollected >= selectedSlot.price)
        {
            // 1) adiciona a carta ao inventário
            inventoryController.AddCard(selectedSlot.cardData);

            // 2) debita as moedas
            playerController.coinsCollected -= selectedSlot.price;

            // 3) marca slot como vendido e limpa seleção
            selectedSlot.MarkAsSold();
            ClearSlotHighlights();
            ClearDetails();
        }
        else
        {
            Debug.LogWarning("Você não tem moedas suficientes!");
        }
    }

    private void ClearDetails()
    {
        selectedSlot = null;
        nomeText.text = "";
        poderText.text = "";
        descricaoText.text = "";
        buyButton.interactable = false;
    }
}
