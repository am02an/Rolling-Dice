using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class RC_MainMenuUI : BaseMainMenuUI
{
    public static RC_MainMenuUI Instance;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }
   protected override void Start()
    {
        base.Start();
       
        PhotonManager.Instance.singlePlayermatch = false;
    }
    protected override void UpdateUI()
    {
        SaveManager.Instance.ForUiUpdate("RacingGame", coinsText, xpText, dragPointText, playerName);
    }
    public override void StartSinglePlayerMatch(string gameName)
    {
        base.StartSinglePlayerMatch(gameName);
    }
}

