using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;

    [Header("Login Settings")]
    public TMP_InputField usernameInput;

    [Header("Title ID (from PlayFab Game Manager)")]
    public string playFabTitleId = "D0001";

    private string playFabId; // store for later use
    [Header("Loader UI")]
    [SerializeField] private GameObject loader;
    [SerializeField] private Image loaderFill; // Image type must be "Filled", Fill Method = Radial360

    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "RacingMainMenu";
    [SerializeField] private float minFillSpeed = 0.2f;
    [SerializeField] private float maxFillSpeed = 0.5f;
    [SerializeField] private float smoothTransitionTime = 0.5f;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = playFabTitleId;

        AutoLogin();
    }

    public void AutoLogin()
    {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        },
        result =>
        {
            playFabId = result.PlayFabId;

            string displayName = result.InfoResultPayload?.PlayerProfile?.DisplayName;

            if (!string.IsNullOrEmpty(displayName))
            {
                Debug.Log("User already has a username: " + displayName);

                // Load data
                SaveManager.Instance.LoadData(displayName);
                GameManager.Instance.SetState(GameState.MainMenu);

                // Show lobby
                StartCoroutine(UIUtils.FadeCanvasGroup("Popup_SignIn", 0, 0.2f, false));
                //StartCoroutine(UIUtils.FadeCanvasGroup("Lobby", 1, 0.2f, true));
                StartCoroutine(WaitForLoadTheScene());
            }
            else
            {
                Debug.Log("No username found. Show sign-in panel.");
                SaveManager.Instance.LoadData(null);

                // Show sign-in panel
                GameManager.Instance.SetState(GameState.SignIn);
                StartCoroutine(UIUtils.FadeCanvasGroup("Lobby", 0, 0.2f, false));
                StartCoroutine(UIUtils.FadeCanvasGroup("Popup_SignIn", 1, 0.2f, true));
            }
        },
        error =>
        {
            Debug.LogError("Login failed: " + error.GenerateErrorReport());
        });
    }

    private IEnumerator WaitForLoadTheScene()
    {
        loader.SetActive(true);
        loaderFill.fillAmount = 0f;

        float fillSpeed = UnityEngine.Random.Range(minFillSpeed, maxFillSpeed); // random fill speed

        // Fill the circle
        while (loaderFill.fillAmount < 1f)
        {
            loaderFill.fillAmount += fillSpeed * Time.deltaTime;
            yield return null;
        }

        // Smooth transition after fill complete
        yield return StartCoroutine(SmoothSceneTransition());

        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator SmoothSceneTransition()
    {
        // Optional fade-out effect for loader (smooth transition)
        CanvasGroup cg = loader.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = loader.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
        }

        float t = 0;
        while (t < smoothTransitionTime)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / smoothTransitionTime);
            yield return null;
        }
    }
    /// <summary>
    /// Called by UI button when user enters a name and presses "Confirm".
    /// </summary>
    public void SubmitUsername()
    {
        if (string.IsNullOrEmpty(usernameInput.text))
        {
            Debug.LogWarning("Username cannot be empty.");
            return;
        }

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = usernameInput.text
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, result =>
        {
            Debug.Log("Username saved: " + result.DisplayName);

            // Save initial data if first-time user
            SaveInitialDataToCloud();

            // Go to lobby
            StartCoroutine(UIUtils.FadeCanvasGroup("Popup_SignIn", 0, 0.2f, false));
            StartCoroutine(UIUtils.FadeCanvasGroup("Lobby", 1, 0.2f, true));
            GameManager.Instance.SetState(GameState.GameSelection);

            SaveManager.Instance.LoadData(result.DisplayName);
        },
        error =>
        {
            Debug.LogError("Failed to save username: " + error.GenerateErrorReport());
        });
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

        PlayFabClientAPI.UpdateUserData(request,
        result => Debug.Log("Initial data saved to cloud."),
        error => Debug.LogError("Failed to save data: " + error.GenerateErrorReport()));
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
        error => Debug.LogError("Failed to load user data: " + error.GenerateErrorReport()));
    }
}

[Serializable]
public class UsernameListWrapper
{
    public List<string> usernames;
}
