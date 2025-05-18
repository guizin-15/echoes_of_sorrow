using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject menuPanel;
    public GameObject optionsPanel;

    [Header("Cena Inicial do Jogo")]
    public string nomeCenaDoJogo = "CenaInicial"; // defina no Inspector

    [Header("Transição com Fade")]
    public ScreenFader screenFader; // arraste no Inspector


    /// <summary>
    /// Começa uma nova jornada apagando qualquer save anterior.
    /// </summary>
    public void NewJourney()
    {
        SaveSystem.DeleteSave();
        screenFader.FadeOutAndLoadScene(nomeCenaDoJogo);
    }

    /// <summary>
    /// Continua a partir do último ponto salvo, se houver.
    /// </summary>
    public void ContinueGame()
    {
        if (SaveSystem.HasSave())
        {
            SaveData data = SaveSystem.LoadGame();
            screenFader.FadeOutAndLoadScene(data.sceneName);
        }
        else
        {
            screenFader.FadeOutAndLoadScene(nomeCenaDoJogo);
        }
    }


    /// <summary>
    /// Abre o painel de opções.
    /// </summary>
    public void AbrirOpcoes()
    {
        menuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    /// <summary>
    /// Retorna ao menu principal.
    /// </summary>
    public void VoltarAoMenu()
    {
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
    
    public void VoltarAoMenuViaBotao()
    {
        Debug.Log("🔙 Botão Voltar ao Menu clicado!");
        screenFader.FadeOutAndLoadScene("Menu");
    }


    /// <summary>
    /// Encerra o jogo.
    /// </summary>
    public void SairDoJogo()
    {
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }
}
