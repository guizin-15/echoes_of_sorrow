using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlayerController2D player;
    public DeathUIController deathUI;

    void Start()
    {
        if (SaveSystem.HasSave())
        {
            LoadGame();
        }
    }

    public void LoadGame()
    {
        SaveData data = SaveSystem.LoadGame();
        if (data != null)
        {
            if (SceneManager.GetActiveScene().name != data.sceneName)
            {
                SceneManager.LoadScene(data.sceneName);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                ApplySaveData(data);
            }
        }
    }

    public void Respawn()
    {
        deathUI.HideDeathScreen();
        LoadGame();
    }

    public void PlayerMorreu()
    {
        Debug.Log("☠️ Jogador morreu!");
        deathUI.ShowDeathScreen();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveData data = SaveSystem.LoadGame();
        ApplySaveData(data);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void ApplySaveData(SaveData data)
    {
        player.transform.position = new Vector2(data.playerX, data.playerY);
        player.currentHealth = player.maxHealth;
        player.coinsCollected = 0;

        player.isDead = false;
        player.isTakingDamage = false;
        player.enabled = true;

        // MUITO IMPORTANTE:
        player.gameObject.layer = LayerMask.NameToLayer("Default");
        player.tag = "Player";

        // Reativa todos os colliders
        foreach (var col in player.GetComponents<Collider2D>())
            col.enabled = true;

        foreach (var enemy in FindObjectsOfType<EnemyBase>())
        {
            enemy.ResetEnemy();
        }

        // Reativa o Rigidbody
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        player.GetComponent<Animator>().Play("Idle");

        FindAnyObjectByType<VidaUIController>()?.UpdateVida();
    }

    public void TesteRespawnBotao()
    {
        Debug.Log("Botão funciona!");
    }



}
