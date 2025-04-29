using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public string nomeCenaDoJogo;

    public void IniciarJogo()
    {
        SceneManager.LoadScene(nomeCenaDoJogo);
    }

    public void SairDoJogo()
    {
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }
}
