using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    public PlayerController2D player;
    public DeathUIController deathUI;
    public HealthBar healthBar;

    private void Awake()
    {
        // Implementação do Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

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
        if (deathUI != null)
            deathUI.HideDeathScreen();
        LoadGame();
    }

    public void PlayerMorreu()
    {
        Debug.Log("☠️ Jogador morreu!");
        
        // Buscar a referência do deathUI se for nula
        if (deathUI == null)
        {
            deathUI = FindObjectOfType<DeathUIController>();
        }
        
        if (deathUI != null)
        {
            deathUI.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("DeathUIController não encontrado na cena!");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveData data = SaveSystem.LoadGame();
        ApplySaveData(data);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void ApplySaveData(SaveData data)
    {
        // Buscar o player se for nulo
        if (player == null)
        {
            player = FindObjectOfType<PlayerController2D>();
            if (player == null)
            {
                Debug.LogError("PlayerController2D não encontrado na cena!");
                return;
            }
        }

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

        if (healthBar == null)
        {
            healthBar = FindObjectOfType<HealthBar>();
        }

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(player.maxHealth);
            healthBar.SetHealth(player.currentHealth);
        }
    }

    public void TesteRespawnBotao()
    {
        Debug.Log("Botão funciona!");
    }
}