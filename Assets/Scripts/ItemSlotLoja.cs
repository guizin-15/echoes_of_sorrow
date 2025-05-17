using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotLoja : MonoBehaviour, IPointerClickHandler
{
    [Header("Dados do Card")]
    public CardData cardData;
    public int price;

    [Header("Referências de UI")]
    [SerializeField] private Image     artworkImage;
    [SerializeField] private TMP_Text  priceText;
    [SerializeField] private Image     highlightImage;
    [SerializeField] private GameObject vendidoPanel; 

    [HideInInspector] public ShopMenu shopMenu;
    private bool isSold = false;

    void Awake()
    {
        shopMenu = FindObjectOfType<ShopMenu>();

        if (cardData != null && artworkImage != null)
            artworkImage.sprite = cardData.artwork;
        if (priceText != null)
            priceText.text = price.ToString();

        if (highlightImage != null) highlightImage.enabled = false;
        if (vendidoPanel != null)  vendidoPanel.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSold) 
            return;   

        shopMenu.ClearSlotHighlights();
        if (highlightImage != null) highlightImage.enabled = true;

        shopMenu.SelectItem(this);
    }

    public void MarkAsSold()
    {   
        Debug.LogWarning(" VENDIDO " + cardData.cardName);
        isSold = true;
        if (vendidoPanel != null) vendidoPanel.SetActive(true);

        if (highlightImage != null) highlightImage.enabled = false;
    }

    public void Highlight(bool on)
    {
        if (highlightImage != null) highlightImage.enabled = on;
    }
}
