using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    private bool playerInRange = false;
    private PlayerController2D player;

    [Header("UI")]
    public GameObject savePromptUI;
    public GameObject saveFeedbackUI;
    public float feedbackDuration = 2f;

    // Remova todo o Update() que usava Input.GetKeyDown(KeyCode.F)

    // 1) Esse método será chamado pelo seu Button.OnClick()
    public void OnSaveButton()
    {
        if (!playerInRange || player == null) 
            return;

        // salva o jogo
        SaveSystem.SaveGame(player);
        Debug.Log("💾 Jogo salvo!");

        // mostra feedback
        if (saveFeedbackUI != null)
            StartCoroutine(ShowSaveMessage());
        else
            Debug.LogWarning("⚠️ Campo SaveFeedbackUI está vazio!");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
            player = col.GetComponent<PlayerController2D>();
            if (savePromptUI != null)
                savePromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            if (savePromptUI != null)
                savePromptUI.SetActive(false);
        }
    }

    private IEnumerator ShowSaveMessage()
    {
        saveFeedbackUI.SetActive(true);
        yield return new WaitForSeconds(feedbackDuration);
        saveFeedbackUI.SetActive(false);
    }
}
