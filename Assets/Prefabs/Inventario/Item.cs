using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Dados do item")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite sprite;
    [TextArea]       [SerializeField] private string description;

    private InventarioController inventario;

    private void Start()
    {
        var root = GameObject.FindWithTag("InventoryUI");
        if (root == null)
        {
            Debug.LogError("[Item] could not find InventoryUI root!");
            return;
        }

        inventario = root.GetComponent<InventarioController>();
        if (inventario == null)
            Debug.LogError("[Item] InventarioController missing on InventoryUI!");
    }
}
