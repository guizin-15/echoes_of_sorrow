using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryToggle : MonoBehaviour
{
    private GameObject          inventoryRoot;
    private PlayerController2D  playerController;

    void Awake()
    {
        // 1) Tenta achar pelo tag “InventoryUI”
        inventoryRoot = GameObject.FindWithTag("InventoryUI");

        // 2) Se não achar por tag, varre todos os roots cujo nome contenha “inventory”
        if (inventoryRoot == null)
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name.ToLower().Contains("inventory"))
                {
                    inventoryRoot = root;
                    break;
                }
            }
        }

        if (inventoryRoot == null)
            Debug.LogError("[InventoryToggle] inventoryRoot não encontrado! " +
                "Marque o seu UI root com tag 'InventoryUI' ou certifique-se que o nome contenha 'inventory'.");

        // 3) Acha o player pelo tag “Player”
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
            playerController = playerGO.GetComponent<PlayerController2D>();
        if (playerController == null)
            Debug.LogError("[InventoryToggle] PlayerController2D não encontrado em objeto com tag 'Player'!");
    }

    void Update()
    {
        if (inventoryRoot == null) 
            return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            // Toggle inventário
            bool isOpen = !inventoryRoot.activeSelf;
            inventoryRoot.SetActive(isOpen);

            // Desativa/ativa controle do player
            if (playerController != null)
                playerController.enabled = !isOpen;
        }
    }
}
