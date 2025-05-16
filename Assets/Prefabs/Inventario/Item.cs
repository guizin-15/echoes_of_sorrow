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
            .Find("Inventario")              
            .GetComponent<InventarioController>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            inventario.AddItem(itemName, sprite, description);
            Destroy(gameObject);              
        }
    }
}
