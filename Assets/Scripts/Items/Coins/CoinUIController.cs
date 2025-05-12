using TMPro;
using UnityEngine;

public class CoinUIController : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    private PlayerController2D player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController2D>();
    }

    void Update()
    {
        if (player != null)
        {
            coinText.text = $"{player.coinsCollected}";
        }
    }
}
