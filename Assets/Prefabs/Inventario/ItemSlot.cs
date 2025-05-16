using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public enum Kind { Inventory, Consume }
    public Kind kind = Kind.Inventory;

    // Dados
    [HideInInspector] public string   itemName;
    [HideInInspector] public Sprite   itemSprite;
    [HideInInspector] public string   description;
    [HideInInspector] public bool     isFull;
    [HideInInspector] public CardData storedCard;

    // UI
    [SerializeField] Image      itemImage;
    [SerializeField] GameObject highlight;

    InventarioController controller;

    void Awake()
    {
        if (highlight == null || !highlight.transform.IsChildOf(transform))
            highlight = transform.Find("itemSelecionado")?.gameObject;

        controller = GetComponentInParent<InventarioController>();
        Clear();
    }

    /* ---------- API pública ---------- */

    // (1) VERSÃO USADA PELOS ITENS ANTIGOS
    public void AddItem(string nome, Sprite sprite, string desc)
    {
        AddItem(nome, sprite, desc, null);   // repassa para a de 4 parâmetros
    }

    // (2) VERSÃO COMPLETA, aceita CardData
    public void AddItem(string nome, Sprite sprite, string desc, CardData card)
    {
        itemName    = nome;
        itemSprite  = sprite;
        description = desc;
        storedCard  = card;

        itemImage.enabled = true;
        itemImage.sprite  = sprite;
        isFull = true;
    }

    public void Clear()
    {
        itemName = description = null;
        itemSprite = null;
        storedCard = null;
        isFull = false;

        if (itemImage)  { itemImage.sprite = null;  itemImage.enabled = false; }
        if (highlight)  highlight.SetActive(false);
    }

    /* ---------- seleção ---------- */
    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left)
            Debug.Log($"Slot {itemName} clicado!");
            controller.SelectSlot(this);
    }

    public void SetSelected(bool on) => highlight?.SetActive(on);
}
