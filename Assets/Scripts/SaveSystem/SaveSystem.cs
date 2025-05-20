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
        data.moedasColetadas = player.coinsCollected;

        // data.itensColetados = inventario.GetItens();

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("save", json);
        PlayerPrefs.Save();

        Debug.Log($"✅ Jogo salvo! Cena: {data.sceneName}, Posição: ({data.playerX}, {data.playerY}), Moedas: {data.moedasColetadas}");
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
        
        // Log detalhado para debugging
        Debug.Log($"📂 Carregando save: Cena: {data.sceneName}, Posição: ({data.playerX}, {data.playerY})");
        
        return data;
    }

    public static bool HasSave()
    {
        bool hasSave = PlayerPrefs.HasKey("save");
        Debug.Log($"🔍 Verificando save: {(hasSave ? "Encontrado" : "Não encontrado")}");
        return hasSave;
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey("save");
        Debug.Log("🗑️ Save deletado!");
    }
    
    // Método para debugging - mostra informações do save atual
    public static void DebugSaveInfo()
    {
        if (HasSave())
        {
            SaveData data = LoadGame();
            Debug.Log($"==== INFORMAÇÕES DO SAVE ATUAL ====\n" +
                      $"Cena: {data.sceneName}\n" +
                      $"Posição: ({data.playerX}, {data.playerY})\n" +
                      $"Vida: {data.vidaAtual}\n" +
                      $"Moedas: {data.moedasColetadas}\n" +
                      $"================================");
        }
        else
        {
            Debug.Log("==== NENHUM SAVE ENCONTRADO ====");
        }
    }
}