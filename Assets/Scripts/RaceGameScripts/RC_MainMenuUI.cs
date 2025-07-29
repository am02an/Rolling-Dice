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

    private int dragPoints;
    private int coins;
    private int xp;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        LoadPlayerData();
        UpdateUI();
    }

    // Called when clicking the start button
    public void StartSinglePlayerMatch()
    {
        PhotonManager.Instance.singlePlayermatch = true;
        LobbyUI.Instance.SetGameToPlay("RacingGame");
    }
    public void Start1v1Match()
    {

        PhotonManager.Instance.singlePlayermatch = false;
        LobbyUI.Instance.SetGameToPlay("RacingGame");
    }

    #region Add Functions

    public void AddDragPoints(int amount)
    {
        dragPoints += amount;
        PlayerPrefs.SetInt("DP", dragPoints);
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("Coins", coins);
        UpdateUI();
    }

    public void AddXP(int amount)
    {
        xp += amount;
        PlayerPrefs.SetInt("XP", xp);
        UpdateUI();
    }

    #endregion

    #region Load, Save, Set

    private void LoadPlayerData()
    {
        dragPoints = PlayerPrefs.GetInt("DP", 0);
        coins = PlayerPrefs.GetInt("Coins", 0);
        xp = PlayerPrefs.GetInt("XP", 0);
    }

    public void SetDragPoints(int value)
    {
        dragPoints = value;
        PlayerPrefs.SetInt("DP", dragPoints);
        UpdateUI();
    }

    public void SetCoins(int value)
    {
        coins = value;
        PlayerPrefs.SetInt("Coins", coins);
        UpdateUI();
    }

    public void SetXP(int value)
    {
        xp = value;
        PlayerPrefs.SetInt("XP", xp);
        UpdateUI();
    }

    #endregion

    private void UpdateUI()
    {
        dragPointText.text = "DP: " + dragPoints;
        coinsText.text = "Coins: " + coins;
        xpText.text = "XP: " + xp;
    }
}
