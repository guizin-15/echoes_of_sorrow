using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [Header("Liga/Desliga Loja")]
    [SerializeField] private ShopMenu shopMenu;         // arraste aqui o seu ShopMenu
    [SerializeField] private KeyCode   openKey = KeyCode.E;

    [Header("UI")]
    [SerializeField] private GameObject openPromptUI;   // arraste aqui o painel “Pressione E para abrir”

    private bool playerInRange = false;
    private bool shopOpen = false;                      // controla estado da loja

    void Awake()
    {
        if (openPromptUI != null)
            openPromptUI.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(openKey))
        {
            if (shopMenu == null)
            {
                Debug.LogError("[ShopTrigger] shopMenu não atribuído!", this);
                return;
            }

            // Se a loja está fechada, abra; senão, feche
            if (!shopOpen)
            {
                Debug.Log("[ShopTrigger] Abrindo loja", this);
                shopMenu.OpenShop();
                shopOpen = true;
            }
            else
            {
                Debug.Log("[ShopTrigger] Fechando loja", this);
                shopMenu.CloseShop();
                shopOpen = false;
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

            // fecha a loja se ainda estiver aberta
            if (shopMenu != null && shopOpen)
            {
                shopMenu.CloseShop();
                shopOpen = false;
                Debug.Log("[ShopTrigger] Loja fechada ao sair da área", this);
            }
        }
    }
}
