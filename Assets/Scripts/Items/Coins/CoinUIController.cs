using System.Collections;
using TMPro;
using UnityEngine;

public class CoinUIController : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    private PlayerController2D player;
    private int lastCoinCount = -1;

    void Start()
    {
        player = FindObjectOfType<PlayerController2D>();
        // Também verifica o GameSession para garantir sincronização
        GameSession gameSession = GameSession.Instance;
        if (gameSession != null && player != null)
        {
            // Sincronizamos com o maior valor entre player e GameSession
            // para nunca perder moedas entre cenas
            if (gameSession.moedas > player.coinsCollected)
            {
                player.coinsCollected = gameSession.moedas;
                Debug.Log($"💰 CoinUI atualizou moedas do player para: {player.coinsCollected}");
            }
            else if (player.coinsCollected > gameSession.moedas)
            {
                gameSession.moedas = player.coinsCollected;
                Debug.Log($"💰 CoinUI atualizou moedas do GameSession para: {gameSession.moedas}");
            }
        }
        UpdateCoinText();
    }

    void Update()
    {
        if (player != null)
        {
            // Só atualiza se o valor mudou
            if (player.coinsCollected != lastCoinCount)
            {
                UpdateCoinText();
                // Também atualiza o GameSession
                GameSession gameSession = GameSession.Instance;
                if (gameSession != null)
                {
                    gameSession.moedas = player.coinsCollected;
                }
            }
        }
        else
        {
            // Tenta encontrar o player se for nulo
            player = FindObjectOfType<PlayerController2D>();
            if (player != null)
            {
                UpdateCoinText();
            }
        }
    }

    public void UpdateCoinText()
    {
        if (coinText != null && player != null)
        {
            coinText.text = $"{player.coinsCollected}";
            lastCoinCount = player.coinsCollected;
        }
    }

    // Este método foi modificado para evitar contagem duplicada
    // Agora apenas atualiza o texto sem incrementar o valor
    public void OnCoinCollected(int amount = 1)
    {
        // NÃO incrementamos mais aqui, apenas atualizamos o texto
        // A atualização do contador agora é feita apenas nas classes de moeda
        if (player != null)
        {
            UpdateCoinText();
            // Atualiza GameSession
            GameSession gameSession = GameSession.Instance;
            if (gameSession != null)
            {
                gameSession.moedas = player.coinsCollected;
                gameSession.SalvarEstado(player);
            }
        }
    }
}