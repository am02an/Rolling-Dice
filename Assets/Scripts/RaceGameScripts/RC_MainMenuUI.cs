using System.Collections;
using UnityEngine;
using TMPro;

public class RC_MainMenuUI : MonoBehaviour
{
    public static RC_MainMenuUI Instance;
    [Header("UI References")]
    public TextMeshProUGUI dragPointText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI playerName;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        GameManager.Instance.SetState(GameState.MainMenu);
        UpdateUI();
    }

    // Called when clicking the start button
    public void StartSinglePlayerMatch()
    {
     //   LoadingScreenManager.Instance.ShowLoadingScreen();
        PhotonManager.Instance.singlePlayermatch = true;
        LobbyUI.Instance.SetGameToPlay("RacingGame");
    }
    public void Start1v1Match()
    {

        PhotonManager.Instance.singlePlayermatch = false;
        LobbyUI.Instance.SetGameToPlay("RacingGame");
    }


    #region Load, Save, Set
   

    #endregion
 
    private void UpdateUI()
    {
       SaveManager.Instance. ForUiUpdate("RacingGame",coinsText, xpText, dragPointText,playerName);
      
    }
}
