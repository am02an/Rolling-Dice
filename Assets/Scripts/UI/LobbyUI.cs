using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the lobby UI functionality including toggling AI match settings,
/// syncing UI with PhotonManager state, and managing player-related visuals.
/// </summary>
public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance;

    #region UI References
    [Header("AI Match Toggle")]
    public Toggle aiMatchToggle;
    public Image player2Image;
    public TextMeshProUGUI startTimeText;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //if (PhotonManager.Instance != null)
        //    aiMatchToggle.isOn = PhotonManager.Instance.allowAIMatch;

        //aiMatchToggle.onValueChanged.AddListener(OnAIMatchToggleChanged);
    }
    #endregion

    #region Event Handlers
    private void OnAIMatchToggleChanged(bool isOn)
    {
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.allowAIMatch = isOn;
            Debug.Log("AI Matches Allowed: " + isOn);
        }
    }
    #endregion
    #region SetGame
    public void SetGameToPlay(string gameName)
    {
        //UIUtils.FadeCanvasGroup(gamePanel, 0, 0.2f, false);
        PhotonManager.Instance.stringGameName = gameName;
        StartMatch();
    }
    public void StartMatch()
    {
        if (!PhotonManager.Instance.singlePlayermatch)
        {
            PhotonManager.Instance.matchMaking.GetComponent<Canvas>().sortingOrder = 2;
            StartCoroutine(UIUtils.FadeCanvasGroup("Play_Battle", 1, 0.2f, true));
        }
        PhotonManager.Instance.StartMatch();
    }
    public void MoveToRaceMainMenu()
    {
        SceneManager.LoadScene("RacingMainMenu");
    }
   public void QuickMatch()
    {

        SceneManager.LoadScene("RacingGame");
    }
    #endregion
}
