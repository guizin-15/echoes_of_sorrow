using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DocUI : MonoBehaviour
{
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

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController2D>();
        }

        docPanel.SetActive(true);  // Garante atualização do layout do texto
        contentText.ForceMeshUpdate();
        docPanel.SetActive(false); // Depois esconde

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
