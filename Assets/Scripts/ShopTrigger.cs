using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [Header("Liga/Desliga Loja")]
    [SerializeField] private ShopMenu shopMenu;         // arraste aqui o seu ShopMenu
    [SerializeField] private KeyCode   openKey = KeyCode.E;

    [Header("UI")]
    [SerializeField] private GameObject openPromptUI;   // arraste aqui o painel “Pressione E para abrir”

    private bool playerInRange = false;

    void Awake()
    {
        if (openPromptUI != null)
            openPromptUI.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(openKey))
        {
            Debug.Log("[ShopTrigger] Tecla de abrir loja pressionada", this);
            if (shopMenu != null)
            {
                shopMenu.OpenShop();
            }
            else
            {
                Debug.LogError("[ShopTrigger] shopMenu não atribuído!", this);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("[ShopTrigger] Jogador entrou na área da loja", this);
            if (openPromptUI != null)
                openPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("[ShopTrigger] Jogador saiu da área da loja", this);
            if (openPromptUI != null)
                openPromptUI.SetActive(false);

            // opcional: fecha a loja se ela estiver aberta
            if (shopMenu != null)
                shopMenu.CloseShop();
        }
    }
}
