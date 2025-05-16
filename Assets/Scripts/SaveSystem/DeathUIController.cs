using UnityEngine;
using UnityEngine.UI;

public class DeathUIController : MonoBehaviour
{
    public GameObject deathPanel;

    public void ShowDeathScreen()
    {
        deathPanel.SetActive(true);
    }

    public void HideDeathScreen()
    {
        deathPanel.SetActive(false);
    }
}
