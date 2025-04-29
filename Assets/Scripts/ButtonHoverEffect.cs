using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject leftOrnament;
    public GameObject rightOrnament;

    public void OnPointerEnter(PointerEventData eventData)
    {
        leftOrnament.SetActive(true);
        rightOrnament.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        leftOrnament.SetActive(false);
        rightOrnament.SetActive(false);
    }
}
