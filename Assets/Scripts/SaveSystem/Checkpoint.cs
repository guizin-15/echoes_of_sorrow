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

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (player != null)
            {
                SaveSystem.SaveGame(player);
                Debug.Log("💾 Jogo salvo!");

                if (saveFeedbackUI != null)
                {
                    Debug.Log("✅ Mostrando mensagem de save!");
                    StartCoroutine(ShowSaveMessage());
                }
                else
                {
                    Debug.LogWarning("⚠️ Campo SaveFeedbackUI está vazio!");
                }
            }
        }
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
