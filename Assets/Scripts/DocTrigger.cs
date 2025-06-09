using UnityEngine;

public class DocTrigger : MonoBehaviour
{
    private bool playerInRange = false;

    [SerializeField] private DocUI docUI;

    void Start()
    {
        if (docUI == null)
            docUI = GetComponentInChildren<DocUI>();
    }

    public void OnDocButton()
    {
        if (playerInRange)
            docUI.ShowDocument();
    }

    // Remova todo o Update()

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            docUI.ShowPrompt(true);  // aqui mostra o prompt (agora um botão)
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            docUI.ShowPrompt(false);
        }
    }
}
