using UnityEngine;
using UnityEngine.UI;

public class RespawnButton : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnRespawnClick);
        }
        else
        {
            Debug.LogError("Componente Button não encontrado no objeto RespawnButton");
        }
    }

    public void OnRespawnClick()
    {
        Debug.Log("Botão de respawn clicado!");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Respawn();
            Debug.Log("Chamando método Respawn do GameManager");
        }
        else
        {
            Debug.LogError("GameManager.Instance é nulo! Tentando encontrar pelo método tradicional...");
            
            // Tentativa de fallback
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.Respawn();
                Debug.Log("Chamando Respawn via referência direta");
            }
            else
            {
                Debug.LogError("Nenhum GameManager encontrado na cena!");
            }
        }
    }
}