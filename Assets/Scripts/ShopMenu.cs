using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopMenu : MonoBehaviour
{
    [Header("UI Root")]
    public GameObject          shopPanel;
    public bool IsOpen => shopPanel != null && shopPanel.activeSelf;

    [Header("Detalhes do Item")]
    public TMP_Text            nomeText;
    public TMP_Text            poderText;
    public TMP_Text            descricaoText;
    public Button              buyButton;

    [Header("Referências de Jogo")]
    public PlayerController2D  playerController;  

    private ItemSlotLoja[]     allSlots;
    private ItemSlotLoja       selectedSlot;

    void Awake()
    {
        allSlots = FindObjectsOfType<ItemSlotLoja>();

        shopPanel.SetActive(false);
        buyButton.interactable = false;
        buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        ClearDetails();
        ClearSlotHighlights();

        // desabilita controles do jogador
        EnablePlayerControls(false);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);

        // reabilita controles do jogador
        EnablePlayerControls(true);
    }

    void OnBuyButtonClicked()
    {
        if (selectedSlot == null) return;

        // 1) adiciona o card ao InventoryManager
        InventoryManager.Instance.AddCard(selectedSlot.cardData);

        // 2) debita as moedas
        playerController.coinsCollected -= selectedSlot.price;

        // 3) marca slot como vendido e limpa seleção
        selectedSlot.MarkAsSold();
        ClearSlotHighlights();
        ClearDetails();
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

    private void ClearDetails()
    {
        selectedSlot = null;
        nomeText.text = "";
        poderText.text = "";
        descricaoText.text = "";
        buyButton.interactable = false;
    }

    /// <summary>
    /// Liga/desliga o componente PlayerController2D para permitir ou bloquear o movimento do jogador.
    /// </summary>
    private void EnablePlayerControls(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;
    }
}
