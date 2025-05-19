using UnityEngine;

public class DocTrigger : MonoBehaviour
{
    private bool playerInRange = false;

    [SerializeField] private DocUI docUI; // Agora referenciamos localmente

    void Start()
    {
        // Se não tiver setado no Inspector, busca no próprio prefab
        if (docUI == null)
            docUI = GetComponentInChildren<DocUI>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            docUI.ShowDocument(); 
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            docUI.ShowPrompt(true);
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
