using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



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

    public void Start()
    {
        // Encontra referências importantes se forem nulas
        if (player == null)
            player = FindObjectOfType<PlayerController2D>();

        if (deathUI == null)
            deathUI = FindObjectOfType<DeathUIController>();

        if (healthBar == null)
            healthBar = FindObjectOfType<HealthBar>();

        // Verifica se existe um GameSession e sincroniza moedas se necessário
        GameSession gameSession = GameSession.Instance;
        if (gameSession != null && player != null)
        {
            Debug.Log("💰 Sincronizando moedas com GameSession: " + gameSession.moedas);
            // Se tivermos um save, vamos respeitar as moedas dele
            // Caso contrário, sincronizamos do GameSession
            if (!SaveSystem.HasSave())
            {
                player.coinsCollected = gameSession.moedas;
            }
        }

        // Se não houver save e estivermos na primeira cena do jogo, cria um save inicial
        if (!SaveSystem.HasSave() && IsFirstScene())
        {
            if (player != null)
            {
                Debug.Log("🆕 Criando save inicial para novo jogo");
                SaveSystem.SaveGame(player);
            }
        }
        // Se houver save, carrega o jogo
        else if (SaveSystem.HasSave())
        {
            LoadGame();
        }
    }

    // Verifica se estamos na primeira cena do jogo
    private bool IsFirstScene()
    {
        // Substitua "SeuNomeDaPrimeiraCena" pelo nome real da sua primeira cena
        return SceneManager.GetActiveScene().name == "Level1" ||
               SceneManager.GetActiveScene().buildIndex == 0;
    }

    public void LoadGame()
    {
        Debug.Log("📥 Tentando carregar o jogo...");
        SaveData data = SaveSystem.LoadGame();
        if (data != null)
        {
            Debug.Log($"📥 Dados carregados. Cena alvo: {data.sceneName}, Cena atual: {SceneManager.GetActiveScene().name}");

            if (SceneManager.GetActiveScene().name != data.sceneName)
            {
                Debug.Log($"🔄 Carregando cena: {data.sceneName}");
                SceneManager.LoadScene(data.sceneName);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Debug.Log("🔄 Aplicando dados na cena atual");
                ApplySaveData(data);
            }
        }
        else
        {
            Debug.LogError("❌ Falha ao carregar dados do save!");
        }
    }

    public void Respawn()
    {
        Debug.Log("🔄 Método Respawn do GameManager chamado");

        if (deathUI != null)
            deathUI.HideDeathScreen();
        else
            Debug.LogWarning("⚠️ deathUI é nulo ao tentar esconder a tela de morte");

        // Verifica se existe um save antes de tentar carregar
        if (SaveSystem.HasSave())
        {
            Debug.Log("📂 Save encontrado, carregando jogo...");
            LoadGame();
        }
        else
        {
            Debug.LogWarning("⚠️ Nenhum save encontrado, reiniciando cena atual");
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }

    public void PlayerMorreu()
    {
        Debug.Log("☠️ Jogador morreu!");

        // Buscar a referência do deathUI se for nula
        if (deathUI == null)
        {
            Debug.Log("🔍 Buscando referência do DeathUIController");
            deathUI = FindObjectOfType<DeathUIController>();
        }

        if (deathUI != null)
        {
            Debug.Log("🔄 Mostrando tela de morte");
            deathUI.ShowDeathScreen();

            // Certifique-se de que todos os botões na UI estão configurados
            ConfigureDeathUIButtons();
        }
        else
        {
            Debug.LogError("❌ DeathUIController não encontrado na cena!");
        }
    }

    // Método para configurar os botões da UI de morte
    private void ConfigureDeathUIButtons()
    {
        if (deathUI != null)
        {
            RespawnButton respawnBtn = deathUI.GetComponentInChildren<RespawnButton>(true);
            if (respawnBtn == null)
            {
                Debug.LogWarning("⚠️ Script RespawnButton não encontrado na DeathUI");

                // Tenta encontrar botões regulares e configurá-los manualmente
                Button[] buttons = deathUI.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn.name.Contains("Respawn") || btn.name.Contains("Restart") || btn.name.Contains("Retry"))
                    {
                        Debug.Log($"🔄 Configurando botão {btn.name} manualmente");
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => Respawn());
                    }
                }
            }
            else
            {
                Debug.Log("✅ Script RespawnButton encontrado e configurado");
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 Cena carregada: {scene.name}");

        // Garante que as referências sejam encontradas na nova cena
        player = FindObjectOfType<PlayerController2D>();
        deathUI = FindObjectOfType<DeathUIController>();
        healthBar = FindObjectOfType<HealthBar>();

        SaveData data = SaveSystem.LoadGame();
        if (data != null)
        {
            Debug.Log("📥 Aplicando dados do save na nova cena");
            ApplySaveData(data);
        }
        else
        {
            Debug.LogError("❌ Dados do save não encontrados após carregar cena!");

            // Fallback: Se não tiver save mas tiver GameSession, pelo menos mantém as moedas
            GameSession gameSession = GameSession.Instance;
            if (gameSession != null && player != null)
            {
                Debug.Log("💰 Usando moedas do GameSession como fallback: " + gameSession.moedas);
                player.coinsCollected = gameSession.moedas;

                // Atualiza a UI de moedas
                CoinUIController coinUI = FindObjectOfType<CoinUIController>();
                if (coinUI != null && coinUI.coinText != null)
                {
                    coinUI.coinText.text = $"{player.coinsCollected}";
                }
            }
        }

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

    public void SalvarAntesDeTrocarCena()
    {
        Debug.Log("🔄 Salvando estado antes de trocar de cena");
        
        if (player != null)
        {
            // Salva o estado atual incluindo moedas
            SaveSystem.SaveGame(player);
            
            // Também atualiza o GameSession
            GameSession gameSession = GameSession.Instance;
            if (gameSession != null)
            {
                Debug.Log($"💰 Atualizando GameSession com {player.coinsCollected} moedas");
                gameSession.moedas = player.coinsCollected;
                gameSession.vida = player.currentHealth;
                gameSession.SalvarEstado(player);
            }
        }
    }

    public void TesteRespawnBotao()
    {
        Debug.Log("Botão funciona!");
    }
}