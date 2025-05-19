// Assets/Scripts/UI/InventoryToggle.cs
using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] GameObject inventarioRoot;      // painel do inventário
    [SerializeField] PlayerController2D player;      // script de movimento

    [Header("Input")]
    [SerializeField] KeyCode openCloseKey = KeyCode.I;
    [SerializeField] KeyCode closeKey     = KeyCode.Escape;

    /* --------- cache --------- */
    bool _open;

    void Awake()
    {
        Debug.Log("InventoryToggle Awake");
        if (!player)
            player = FindFirstObjectByType<PlayerController2D>();
        if (inventarioRoot)
            inventarioRoot.SetActive(false);         // garante fechado
    }

    void Update()
{
    if (Input.GetKeyDown(openCloseKey))
    {
        if (_open)
        {
            CloseInventory();
        }
        else
        {
            Debug.Log("Abrindo inventário");
            OpenInventory();
        }
    }

    if (_open && Input.GetKeyDown(closeKey))
    {
        CloseInventory();
    }
}


    /* -------------------------------------------------------------- */
    void OpenInventory()
    {
        if (inventarioRoot) inventarioRoot.SetActive(true);
        if (player)         player.enabled = false;
        _open = true;
    }

    void CloseInventory()
    {
        if (inventarioRoot) inventarioRoot.SetActive(false);
        if (player)         player.enabled = true;
        _open = false;
    }
}