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
            Debug.Log("🔄 Destruindo GameSession duplicado");
            Destroy(gameObject); // Evita duplicados
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre cenas
        Debug.Log("✅ GameSession inicializado");
    }
    
    void Start()
    {
        // Verificar se existe um save e sincronizar moedas
        if (SaveSystem.HasSave())
        {
            SaveData data = SaveSystem.LoadGame();
            if (data != null)
            {
                Debug.Log($"💰 GameSession carregando moedas do save: {data.moedasColetadas}");
                moedas = data.moedasColetadas;
                vida = data.vidaAtual;
            }
        }
    }

    public void SalvarEstado(PlayerController2D player)
    {
        if (player != null)
        {
            moedas = player.coinsCollected;
            vida = player.currentHealth;
            Debug.Log($"💾 GameSession salvou estado: {moedas} moedas, {vida} vida");
            // inventario = player.inventario;
        }
    }

    public void AplicarEstado(PlayerController2D player)
    {
        if (player != null)
        {
            Debug.Log($"🔄 GameSession aplicando estado: {moedas} moedas, {vida} vida");
            player.coinsCollected = moedas;
            
            // Só aplica vida se não estiver com vida máxima
            if (vida > 0 && vida < player.maxHealth)
            {
                player.currentHealth = vida;
            }

            if (player.healthBar != null)
                player.healthBar.SetHealth(player.currentHealth);
                
            // Atualiza UI de moedas se existir
            CoinUIController coinUI = FindObjectOfType<CoinUIController>();
            if (coinUI != null && coinUI.coinText != null)
            {
                coinUI.coinText.text = $"{player.coinsCollected}";
            }

            // player.inventario = new List<string>(inventario);
        }
    }
}