using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BaseMainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dragPointText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI playerName;

    protected virtual void Awake()
    {
        // Common Awake logic (can be extended by child)
    }

    protected virtual void Start()
    {
        Debug.Log("UpdatingUI");
       // GameManager.Instance.SetState(GameState.MainMenu);
        UpdateUI();
    }

    // Abstract UI update (must be implemented in child)
    protected abstract void UpdateUI();

    // Common scene loading
    protected void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Match-start logic (child can override if needed)
    public virtual void StartSinglePlayerMatch(string gameName)
    {
        PhotonManager.Instance.singlePlayermatch = true;
        Debug.Log("SopundCheck");
       
        LobbyUI.Instance.SetGameToPlay(gameName);
    }

    public virtual void StartFreeRoam()
    {
        PhotonManager.Instance.isFreeRoam = true;
        LoadingScreenManager.Instance.ShowLoadingScreen(true,"RacingCity");
        LoadScene("RacingCity");
    }
    public virtual void MoveToSelectionScene()
    {
        GameManager.Instance.SetGame(GameName.None);
        GameManager.Instance.SetState(GameState.GameSelection);

        LoadScene("Lobby");
        StartCoroutine(UIUtils.FadeCanvasGroup("Lobby", 1, 0.2f, true));
    }
   
    public virtual void Start1v1Match()
    {
        PhotonManager.Instance.singlePlayermatch = false;
        LobbyUI.Instance.SetGameToPlay("RacingGame");
    }
}
