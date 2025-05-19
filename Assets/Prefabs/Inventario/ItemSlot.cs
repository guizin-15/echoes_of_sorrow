using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public enum Kind { Inventory, Consume }
    public Kind kind = Kind.Inventory;

    // Dados do slot
    [HideInInspector] public string itemName;
    [HideInInspector] public Sprite itemSprite;
    [HideInInspector] public string description;
    [HideInInspector] public bool isFull;
    [HideInInspector] public CardData storedCard;

    [Header("UI References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject highlight;  // painel ou imagem de seleção

    private InventarioController controller;

    private void Awake()
    {
        // Encontra o InventarioController em qualquer lugar da cena
        controller = FindObjectOfType<InventarioController>();
        if (controller == null)
            Debug.LogError("ItemSlot: não encontrou InventarioController na cena!", this);

        // Se highlight não foi arrastado, busca por child chamado "itemSelecionado"
        if (highlight == null)
            highlight = transform.Find("itemSelecionado")?.gameObject;

        // Inicializa o slot vazio
        Clear();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log($"[ItemSlot] {(itemName ?? name)} clicado (kind={kind})", this);
            if (controller == null)
            {
                Debug.LogError("ItemSlot: controller é null, não posso selecionar.", this);
                return;
            }
            controller.SelectSlot(this);
        }
    }

    /// <summary>
    /// Adiciona um item ao slot (vazio → cheio).
    /// </summary>
    public void AddItem(string name, Sprite sprite, string desc, CardData card = null)
    {
        itemName    = name;
        itemSprite  = sprite;
        description = desc;
        storedCard  = card;

        if (itemImage != null)
        {
            itemImage.sprite  = sprite;
            itemImage.enabled = sprite != null;
        }

        isFull = true;
    }

    /// <summary>
    /// Limpa o slot (cheio → vazio).
    /// </summary>
    public void Clear()
    {
        itemName     = null;
        itemSprite   = null;
        description  = null;
        storedCard   = null;
        isFull       = false;

        if (itemImage != null)
        {
            itemImage.sprite  = null;
            itemImage.enabled = false;
        }

        if (highlight != null)
            highlight.SetActive(false);
    }

    /// <summary>
    /// Marca/desmarca o highlight de seleção.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (highlight != null)
            highlight.SetActive(selected);
    }

    /// <summary>
    /// Helper caso queira usar ClearSlotHighlights() no controller.
    /// </summary>
    public void Highlight(bool on) => SetSelected(on);
}
