using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [Header("Painel de Pause")]
    [SerializeField] private GameObject pausePanel;

    [Header("Sons")]
    [SerializeField] private AudioClip hoverSound;

    [Header("Botões")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button menuButton;

    [Header("Dependências")]
    [SerializeField] private PlayerController2D playerController;
    [SerializeField] private MenuController    menuController;

    private AudioSource audioSource;
    private bool        isPaused;

    void Awake()
    {
        // configura AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // início: painel fechado
        pausePanel.SetActive(false);

        // configura hover visuals/som em cada botão
        foreach (Transform t in pausePanel.transform)
        {
            var buttonGO = t.gameObject;
            var crown = buttonGO.transform.Find("Coroa")?.gameObject;
            if (crown != null) crown.SetActive(false);

            var gfx = buttonGO.GetComponent<Graphic>();
            if (gfx != null) gfx.raycastTarget = true;

            var trigger = buttonGO.GetComponent<EventTrigger>() 
                          ?? buttonGO.AddComponent<EventTrigger>();

            // pointer enter
            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ =>
            {
                if (crown != null) crown.SetActive(true);
                if (hoverSound != null) audioSource.PlayOneShot(hoverSound);
            });
            trigger.triggers.Add(enter);

            // pointer exit
            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => { if (crown != null) crown.SetActive(false); });
            trigger.triggers.Add(exit);
        }

        // hook dos botões
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnContinueClicked);

        saveButton.onClick.RemoveAllListeners();
        saveButton.onClick.AddListener(OnSaveClicked);

        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(OnMenuClicked);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        if (playerController != null)
            playerController.enabled = !isPaused;
        Debug.Log($"[Pause] Paused={(isPaused)}");
    }

    private void OnContinueClicked()
    {
        Debug.Log("[Pause] Continue button clicked");
        if (isPaused)
            TogglePause();
    }

    private void OnSaveClicked()
    {
        Debug.Log("[Pause] Save button clicked");
        if (playerController != null)
            SaveSystem.SaveGame(playerController);
        else
            Debug.LogError("PlayerController2D não atribuído em Pause!");
    }

    private void OnMenuClicked()
    {
        Debug.Log("[Pause] Menu button clicked");
        // fecha o pause
        if (isPaused)
            TogglePause();
        // abre o menu principal
        if (menuController != null)
            menuController.VoltarAoMenuViaBotao();
        else
            Debug.LogError("MenuController não atribuído em Pause!");
    }
}
