using UnityEngine;

public class CoinCollectable : MonoBehaviour
{
    // Esta classe foi mantida para compatibilidade, mas seu uso é desencorajado.
    // Use a classe Coin.cs em vez desta, pois ela possui atração de moedas e outras funcionalidades.
    
    // AVISO: Se você estiver usando esta classe e a classe Coin no mesmo objeto,
    // resultará em contagem duplicada de moedas. Use apenas uma delas!
    
    [Header("IMPORTANTE: Não use esta classe junto com Coin.cs")]
    public int value = 1;  // Valor da moeda (sempre 1)
    public AudioClip collectSound;
    
    private bool isCollected = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;
        
        if (collision.CompareTag("Player"))
        {
            // Marca como coletada para evitar coletar mais de uma vez
            isCollected = true;
            
            // Adiciona ao contador do player
            PlayerController2D player = collision.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.coinsCollected += value;
                Debug.Log($"CoinCollectable: Moeda coletada! Total agora: {player.coinsCollected}");
                
                // Toca som se disponível
                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }
                
                // Atualiza GameSession
                GameSession gameSession = GameSession.Instance;
                if (gameSession != null)
                {
                    gameSession.moedas = player.coinsCollected;
                }
                
                // Opcional: Atualiza UI diretamente
                CoinUIController coinUI = FindObjectOfType<CoinUIController>();
                if (coinUI != null)
                {
                    coinUI.UpdateCoinText();  // Chamamos UpdateCoinText diretamente em vez de OnCoinCollected
                }
                
                // Desativa o objeto
                gameObject.SetActive(false);
                Destroy(gameObject, 0.1f);
            }
        }
    }
}