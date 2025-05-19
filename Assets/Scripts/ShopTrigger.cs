using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [SerializeField] private ShopMenu shopMenu;
    [SerializeField] private KeyCode openKey = KeyCode.E;
    [SerializeField] private GameObject openPromptUI;

    private bool playerInRange;

    void Awake()
    {
        if (openPromptUI != null)
            openPromptUI.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange || shopMenu == null) return;

        if (Input.GetKeyDown(openKey))
        {
            if (shopMenu.IsOpen)
                shopMenu.CloseShop();
            else
                shopMenu.OpenShop();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
            if (openPromptUI != null)
                openPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = false;
            if (openPromptUI != null)
                openPromptUI.SetActive(false);

            if (shopMenu != null && shopMenu.IsOpen)
                shopMenu.CloseShop();
        }
    }
}

