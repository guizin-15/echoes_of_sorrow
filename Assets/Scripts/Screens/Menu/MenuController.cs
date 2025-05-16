using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject menuPanel;
    public GameObject optionsPanel;

    [Header("Cena do Jogo")]
    public string nomeCenaDoJogo;

    public void IniciarJogo()
    {
        if (SaveSystem.HasSave())
        {
            SaveData data = SaveSystem.LoadGame();
            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            SceneManager.LoadScene(nomeCenaDoJogo);
        }
    }

    public void SairDoJogo()
    {
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }

    public void AbrirOpcoes()
    {
        menuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void VoltarAoMenu()
    {
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}
