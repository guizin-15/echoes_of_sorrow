using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance;

    [Header("Status do Jogador")]
    public int moedas;
    public int vida;
    
    //[Header("Inventário (em breve)")]
    //public List<string> inventario = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Evita duplicados
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre cenas
    }

    public void SalvarEstado(PlayerController2D player)
    {
        moedas = player.coinsCollected;
        vida = player.currentHealth;
        // inventario = player.inventario;
    }

    public void AplicarEstado(PlayerController2D player)
    {
        player.coinsCollected = moedas;
        player.currentHealth = vida;

        if (player.healthBar != null)
            player.healthBar.SetHealth(vida);

        // player.inventario = new List<string>(inventario);
    }
}
