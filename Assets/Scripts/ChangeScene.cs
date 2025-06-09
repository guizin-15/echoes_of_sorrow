using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [Header("Nome da próxima cena")]
    [SerializeField] private string cenaAlvo;

    [Header("UI de aviso (agora será um botão)")]
    [SerializeField] private GameObject promptUI;

    private bool jogadorNaArea = false;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    // Este método será chamado pelo Button.OnClick()
    public void OnChangeSceneButton()
    {
        if (!jogadorNaArea) return;

        var player = FindAnyObjectByType<PlayerController2D>();
        if (player != null && GameSession.Instance != null)
            GameSession.Instance.SalvarEstado(player);

        SceneManager.LoadScene(cenaAlvo);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorNaArea = true;
            if (promptUI != null)
                promptUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorNaArea = false;
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }
}
