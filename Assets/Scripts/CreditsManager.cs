using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
    public ScrollRect scrollRect;
    public GameObject backButton;

    void Start()
    {
        backButton.SetActive(false); // Esconde no início
    }

    void Update()
    {
        // Quando o scroll chega ao topo (0)
        if (scrollRect.verticalNormalizedPosition <= 0.001f)
        {
            backButton.SetActive(true);
        }
    }
}
