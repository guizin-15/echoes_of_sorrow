using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float delayBeforeMenu = 5f; // tempo após o fim do scroll

    private bool hasReachedEnd = false;

    void Update()
    {
        if (!hasReachedEnd && scrollRect.verticalNormalizedPosition <= 0.001f)
        {
            hasReachedEnd = true;
            Invoke(nameof(GoToMenu), delayBeforeMenu);
        }
    }

    void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
