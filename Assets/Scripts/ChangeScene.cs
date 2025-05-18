using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class ChangeScene : MonoBehaviour
{
    [Header("Nome da próxima cena")]
    [SerializeField] private string cenaAlvo;

    [Header("Tecla para trocar de fase")]
    [SerializeField] private KeyCode teclaTroca = KeyCode.E;

    [Header("UI de aviso")]
    [SerializeField] private GameObject promptTexto;

    private bool jogadorNaArea = false;

    void Start()
    {
        if (promptTexto != null)
            promptTexto.SetActive(false);
    }

    void Update()
    {
        if (jogadorNaArea && Input.GetKeyDown(teclaTroca))
        {
            var player = FindAnyObjectByType<PlayerController2D>();
            if (player != null && GameSession.Instance != null)
            {
                GameSession.Instance.SalvarEstado(player);
            }

            SceneManager.LoadScene(cenaAlvo);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorNaArea = true;
            if (promptTexto != null)
                promptTexto.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorNaArea = false;
            if (promptTexto != null)
                promptTexto.SetActive(false);
        }
    }
}
