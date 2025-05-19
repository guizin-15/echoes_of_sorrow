using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DocUI : MonoBehaviour
{
    public static DocUI Instance;

    [Header("UI Elements")]
    public GameObject promptPanel;
    public GameObject docPanel;

    public Image documentImage;
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI closeHintText;
    public Button closeButton; 

    [Header("Document Content")]
    [TextArea(5, 10)] public string documentText;
    public Sprite documentSprite;

    private GameObject player;
    private PlayerController2D playerController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController2D>();
        }

        docPanel.SetActive(true);
        contentText.ForceMeshUpdate();
        docPanel.SetActive(false);

        closeButton.onClick.AddListener(() => HideDocument());
    }

    void Update()
    {
        if (docPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Fechando o painel com ESC");
            HideDocument();
        }
    }

    public void ShowPrompt(bool show)
    {
        promptPanel.SetActive(show);
    }

    public void ShowDocument()
    {
        docPanel.SetActive(true);
        documentImage.sprite = documentSprite != null ? documentSprite : null;
        contentText.text = documentText;
        closeHintText.text = "";

        EnablePlayerControls(false);
    }

    void HideDocument()
    {
        docPanel.SetActive(false);
        EnablePlayerControls(true);
    }

    void EnablePlayerControls(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;
    }
}
