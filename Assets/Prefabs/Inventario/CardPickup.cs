// Assets/Scripts/CardPickup.cs
using UnityEngine;

public class CardPickup : MonoBehaviour
{
    [Header("Carta concedida ao jogador")]
    public CardData cartaASerDada;           

    private InventarioController inventario;   

    private void Awake()
    {
        inventario = GameObject
            .Find("Inventario")
            .GetComponent<InventarioController>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            inventario.AddCard(cartaASerDada);
            Destroy(gameObject);              
        }
    }
}
