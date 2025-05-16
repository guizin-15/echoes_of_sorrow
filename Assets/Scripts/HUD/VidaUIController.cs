using UnityEngine;
using UnityEngine.UI;

public class VidaUIController : MonoBehaviour
{
    public Image[] fragmentos;
    public Sprite fragmentoCheio;
    public Sprite fragmentoVazio;
    public PlayerController2D player;

    public void UpdateVida()
    {
        for (int i = 0; i < fragmentos.Length; i++)
        {
            if (i < player.currentHealth)
            {
                fragmentos[i].sprite = fragmentoCheio;
            }
            else
            {
                fragmentos[i].sprite = fragmentoVazio;
            }
        }
    }
}
