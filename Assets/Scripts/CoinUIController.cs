using TMPro;
using UnityEngine;

public class CoinUIController : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    private PlayerController player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if (player != null)
        {
            coinText.text = "x" + player.coinsCollected;
        }
    }
}
