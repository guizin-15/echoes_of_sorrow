using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject menuPanel;
    public GameObject optionsPanel;

    [Header("Lore Intro (Inactive por padrão)")]
    [Tooltip("Canvas que contém BlackPanel, LoreText, ContinueButton")]
    public GameObject loreCanvas;
    [Tooltip("Componente que faz typewriter + fade + LoadScene")]
    public LoreIntroController loreController;
    [TextArea(3,6)]
    [Tooltip("Parágrafos de lore para mostrar no New Journey")]
    public List<string> introParagraphs = new List<string>();

    [Header("Cena Inicial do Jogo")]
    public string nomeCenaDoJogo = "CenaInicial";

    [Header("Transição com Fade (Continuar/Load direto)")]
    public ScreenFader screenFader;

    // ——— New Journey agora exibe lore antes de carregar cena ———
    public void NewJourney()
    {
        SaveSystem.DeleteSave();

        // 1) esconde o menu de opções/menu principal
        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        // 2) ativa o Canvas de lore e dispara a sequência
        loreCanvas.SetActive(true);

        // passa o nome da cena para o controller, caso queira override via Inspector
        loreController.nextSceneName = nomeCenaDoJogo;

        // inicia o efeito typewriter / fade in + eventual LoadScene
        loreController.StartLore(introParagraphs);
    }

    // ——— ContinueGame permanece igual, vai direto pra cena salva ———
    public void ContinueGame()
    {
        if (SaveSystem.HasSave())
        {
            var data = SaveSystem.LoadGame();
            screenFader.FadeOutAndLoadScene(data.sceneName);
        }
        else
        {
            screenFader.FadeOutAndLoadScene(nomeCenaDoJogo);
        }
    }

    // ——— Resto dos métodos de menu/opções/sair… ———

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

    public void VoltarAoMenuViaBotao()
    {
        Debug.Log("🔙 Botão Voltar ao Menu clicado!");
        screenFader.FadeOutAndLoadScene("Menu");
    }

    public void SairDoJogo()
    {
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }
}