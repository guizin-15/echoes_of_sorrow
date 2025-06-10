using UnityEngine;
using UnityEngine.SceneManagement;

public class AdsController : MonoBehaviour
{
    // Nome da cena de destino (defina no Inspector)
    public string nextSceneName;

    // Método para o botão chamar
    public void OnCloseButtonClicked()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("Next Scene Name não foi definido!");
    }
}