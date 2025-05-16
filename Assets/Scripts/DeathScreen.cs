using UnityEngine;
using UnityEngine.SceneManagement; // se quiser recarregar cena
using TMPro;                       // se usar TMP

public class DeathScreenController : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;    
    [SerializeField] private float fadeTime = .6f;
    [SerializeField] private string sceneToReload = ""; 

    private bool showing;

    public void Show()
    {
        gameObject.SetActive(true);
        showing = true;
        StartCoroutine(Fade(0, 1));          // escurece + mostra textos
    }

    void Update()
    {
        // só reage se tela aberta
        if (!showing) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // opcional: fade-out antes de renascer
            StartCoroutine(RespawnRoutine());
        }
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        showing = false;                 
        yield return Fade(1, 0);        

        if (!string.IsNullOrEmpty(sceneToReload))
            SceneManager.LoadScene(sceneToReload);   // recarrega tudo
            
           // FindObjectOfType<GameManager>().Respawn(); // ou reset player
    }

    private System.Collections.IEnumerator Fade(float a, float b)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.unscaledDeltaTime / fadeTime;   // usa delta sem pausar jogo
            cg.alpha = Mathf.Lerp(a, b, t);
            yield return null;
        }
    }
}
