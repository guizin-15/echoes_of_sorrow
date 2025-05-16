using UnityEngine;
using UnityEngine.SceneManagement;
// using System.Collections.Generic;

public static class SaveSystem
{
    public static void SaveGame(PlayerController2D player /*, Inventario inventario */)
    {
        SaveData data = new SaveData();
        data.sceneName = SceneManager.GetActiveScene().name;
        data.playerX = player.transform.position.x;
        data.playerY = player.transform.position.y;
        data.vidaAtual = player.currentHealth;

        // data.itensColetados = inventario.GetItens();

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("save", json);
        PlayerPrefs.Save();

        Debug.Log("✅ Jogo salvo!");
    }

    public static SaveData LoadGame()
    {
        if (!PlayerPrefs.HasKey("save"))
        {
            Debug.LogWarning("⚠️ Nenhum save encontrado.");
            return null;
        }

        string json = PlayerPrefs.GetString("save");
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey("save");
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey("save");
    }
}
