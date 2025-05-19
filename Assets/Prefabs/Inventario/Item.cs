using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Dados do item")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite sprite;
    [TextArea]       [SerializeField] private string description;

    private InventarioController inventario;

    private void Awake()
    {
        inventario = GameObject
            .Find("Inventory")              
            .GetComponent<InventarioController>();
    }
}
