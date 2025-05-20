using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string sceneToLoad;
    public Vector2 playerPositionInNewScene;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"🚪 Transição de cena para: {sceneToLoad}");
            
            // Salvar o estado atual antes de trocar de cena
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.SalvarAntesDeTrocarCena();
            }
            else
            {
                // Fallback: salvar diretamente
                PlayerController2D player = collision.GetComponent<PlayerController2D>();
                if (player != null)
                {
                    SaveSystem.SaveGame(player);
                    
                    // Atualiza o GameSession
                    GameSession gameSession = GameSession.Instance;
                    if (gameSession != null)
                    {
                        gameSession.moedas = player.coinsCollected;
                        gameSession.SalvarEstado(player);
                    }
                }
            }
            
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}