using UnityEngine;

public class InventoryToggleButton : MonoBehaviour
{
    // Este método vai no OnClick() do seu Button
    public void OnClickToggleInventory()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ToggleUI();
    }
}
