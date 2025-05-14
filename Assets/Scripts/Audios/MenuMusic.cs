using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    private void Awake()
    {
        // Evita duplicatas se você voltar ao menu várias vezes
        int musicCount = FindObjectsByType<MenuMusic>(FindObjectsSortMode.None).Length;
        if (musicCount > 1)
        {
            Destroy(gameObject);       // já existe outra tocando
            return;
        }

        DontDestroyOnLoad(gameObject); // mantém a música se trocar de cena
    }
}