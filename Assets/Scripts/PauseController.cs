using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;  // para Graphic (Image/TMP)

public class Pause : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject pausePanel;  // o pai que contém todos os botões
    [SerializeField] private AudioClip hoverSound;   // som de hover

    private AudioSource audioSource;

    void Awake()
    {
        // configura AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // para cada botão (filho) do painel
        foreach (Transform t in pausePanel.transform)
        {
            var buttonGO = t.gameObject;

            // encontra a Coroa dentro dele e garante que comece desativada
            var crown = buttonGO.transform.Find("Coroa")?.gameObject;
            if (crown != null)
                crown.SetActive(false);

            // garante RaycastTarget para receber eventos
            var gfx = buttonGO.GetComponent<Graphic>();
            if (gfx != null)
                gfx.raycastTarget = true;

            // EventTrigger
            var trigger = buttonGO.GetComponent<EventTrigger>() 
                          ?? buttonGO.AddComponent<EventTrigger>();

            // PointerEnter
            var entryEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            entryEnter.callback.AddListener(evt => OnHoverEnter(buttonGO));
            trigger.triggers.Add(entryEnter);

            // PointerExit
            var entryExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            entryExit.callback.AddListener(evt => OnHoverExit(buttonGO));
            trigger.triggers.Add(entryExit);
        }
    }

    private void OnHoverEnter(GameObject buttonGO)
    {
        // ativa a coroa deste botão
        var crown = buttonGO.transform.Find("Coroa")?.gameObject;
        if (crown != null)
            crown.SetActive(true);

        // toca o som
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    private void OnHoverExit(GameObject buttonGO)
    {
        // desativa a coroa deste botão
        var crown = buttonGO.transform.Find("Coroa")?.gameObject;
        if (crown != null)
            crown.SetActive(false);
    }
}
