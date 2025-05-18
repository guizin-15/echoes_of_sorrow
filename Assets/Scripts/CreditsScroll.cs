using UnityEngine;
using UnityEngine.UI;

public class CreditsScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 15f;

    void Update()
    {
        if (scrollRect.verticalNormalizedPosition > 0)
        {
            scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime / 100f;
        }
    }
}
