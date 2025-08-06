using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class PlayFabLoginManager : MonoBehaviour
{
    [Header("Login Settings")]
    public TMP_InputField usernameInput;

    [Header("Title ID (from PlayFab Game Manager)")]
    public string playFabTitleId = "D0001";

    private void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = playFabTitleId;
    }

    public void OnUsernameSubmit()
    {
        string customId = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(customId))
        {
            Debug.LogWarning("Username is empty.");
            return;
        }

        // Step 1: Call CloudScript to check + register username
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        },
        loginResult =>
        {
        // Now safe to call CloudScript
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "TryRegisterUsername",
                FunctionParameter = new { username = customId },
                GeneratePlayStreamEvent = false
            },
            scriptResult =>
            {
                var resultDict = scriptResult.FunctionResult as Dictionary<string, object>;
                if (resultDict != null && resultDict.ContainsKey("success") && (bool)resultDict["success"])
                {
                    Debug.Log("Login successful with username: " + customId);
                // Proceed to load data, UI, etc.
                SaveInitialDataToCloud();
                    LoadPlayerData();
                    StartCoroutine(UIUtils.FadeCanvasGroup("Popup_SignIn", 0, 0.2f, false));
                    StartCoroutine(UIUtils.FadeCanvasGroup("Lobby", 1, 0.2f, true));
                }
                else
                {
                    Debug.LogWarning("Username already exists. Please choose another.");
                }
            },
            error =>
            {
                Debug.LogError("CloudScript error: " + error.GenerateErrorReport());
            });
        },
        error =>
        {
            Debug.LogError("Initial anonymous login failed: " + error.GenerateErrorReport());
        });
    }


    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful! PlayFab ID: " + result.PlayFabId);
        LoadPlayerData();
        StartCoroutine(UIUtils.FadeCanvasGroup("Popup_SignIn", 0, 0.2f, false));
        StartCoroutine(UIUtils.FadeCanvasGroup("Lobby", 1, 0.2f, true));

    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }

    private void SaveInitialDataToCloud()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "DateCreated", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                { "Coins", "1000" }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            Debug.Log("Initial data saved to cloud.");
        },
        error =>
        {
            Debug.LogError("Failed to save data: " + error.GenerateErrorReport());
        });
    }

    private void LoadPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey("DateCreated"))
            {
                Debug.Log("Account created on: " + result.Data["DateCreated"].Value);
                Debug.Log("Coins: " + result.Data["Coins"].Value);
            }
            else
            {
                Debug.Log("No data found. Saving default data.");
                SaveInitialDataToCloud();
            }
        },
        error =>
        {
            Debug.LogError("Failed to load user data: " + error.GenerateErrorReport());
        });
    }
}
[Serializable]
public class UsernameListWrapper
{
    public List<string> usernames;
}
