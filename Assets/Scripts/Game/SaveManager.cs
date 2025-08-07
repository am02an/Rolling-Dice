using UnityEngine;
using System;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public PlayerData PlayerData = new PlayerData();
    private const string SaveKey = "PLAYER_DATA";
    private const string CloudKey = "PlayerData"; // This is the key used in PlayFab

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        
    }

    #region Local Save

    public void SaveData()
    {
        string json = JsonUtility.ToJson(PlayerData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        SaveDataToCloud();
        Debug.Log("✅ Player data saved locally and to cloud.");
    }

    public void LoadData(string playerName)
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            PlayerData = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("✅ Player data loaded from local.");
        }
        else
        {
            Debug.Log("⚠️ No local data found. Creating new player data.");
            CreateNewPlayerData(playerName);
        }
    }

    public void CreateNewPlayerData(string playerName)
    {
        PlayerData = new PlayerData
        {
            playerName = playerName,
            level = 1,
            lastLoginTime = DateTime.Now.ToString(),
            gameStatsList = new List<GameStatsEntry>()
        };

        // Initialize default stats for one game
        string defaultGameName = "DefaultGame"; // Change as needed

        GameStats defaultStats = new GameStats
        {
            coins = 0,
            xp = 0,
            dpPoints = 0
        };

        PlayerData.gameStatsList.Add(new GameStatsEntry
        {
            gameName = defaultGameName,
            stats = defaultStats
        });

        SaveData();
    }



    public void ResetData()
    {
        PlayerPrefs.DeleteKey(SaveKey);
       // CreateNewPlayerData("");
        Debug.Log("🔄 Player data reset.");
    }

    #endregion

    #region Cloud Save/Load (as JSON)

    public void SaveDataToCloud()
    {
        string json = JsonUtility.ToJson(PlayerData);

        var data = new Dictionary<string, string>
        {
            { CloudKey, json }
        };

        var request = new UpdateUserDataRequest { Data = data };

        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            Debug.Log("☁️ Player data saved to cloud as JSON.");
        },
        error =>
        {
            Debug.LogError("❌ Cloud save failed: " + error.GenerateErrorReport());
        });
    }

    public void LoadDataFromCloud()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey(CloudKey))
            {
                string json = result.Data[CloudKey].Value;
                PlayerData = JsonUtility.FromJson<PlayerData>(json);
                SaveData(); // Also update local save
                Debug.Log("☁️ Player data loaded from cloud.");
            }
            else
            {
                Debug.Log("🆕 No cloud data found. Saving default data.");
                SaveDataToCloud();
            }
        },
        error =>
        {
            Debug.LogError("❌ Cloud load failed: " + error.GenerateErrorReport());
        });
    }
    public GameStats GetGameStats(string gameName)
    {
        foreach (var entry in PlayerData.gameStatsList)
        {
            if (entry.gameName == gameName)
                return entry.stats;
        }

        // If not found, create and add
        GameStats newStats = new GameStats();
        PlayerData.gameStatsList.Add(new GameStatsEntry
        {
            gameName = gameName,
            stats = newStats
        });

        return newStats;
    }


    #endregion

    #region Utility

    public void AddCoins(string gameName, int amount)
    {
        var stats = GetGameStats(gameName);
        stats.coins += amount;
        SaveData();
    }

    public void AddXP(string gameName, int amount)
    {
        var stats = GetGameStats(gameName);
        stats.xp += amount;
        SaveData();
    }

    public void AddDP(string gameName, int amount)
    {
        var stats = GetGameStats(gameName);
        stats.dpPoints += amount;
        SaveData();
    }

    public void ForUiUpdate(
      string gameName,
      TextMeshProUGUI coinsText,
      TextMeshProUGUI xpText,
      TextMeshProUGUI dpPointsText,
      TextMeshProUGUI playerNameText)
    {
        var stats = GetGameStats(gameName);

        coinsText.text = stats.coins.ToString();
        xpText.text = stats.xp.ToString();
        dpPointsText.text = stats.dpPoints.ToString();
        playerNameText.text = PlayerData.playerName;
    }

    #endregion
}
[System.Serializable]
public class PlayerData
{
    public string playerName;
    public string lastLoginTime;
    public int level;

    public List<GameStatsEntry> gameStatsList = new List<GameStatsEntry>();
}


[System.Serializable]
public class GameStats
{
    public int coins;
    public int xp;
    public int dpPoints;
}
[System.Serializable]
public class GameStatsEntry
{
    public string gameName;
    public GameStats stats;
}

