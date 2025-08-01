using Photon.Pun;
using UnityEngine;
using TMPro;

public class RC_RPCManager : MonoBehaviourPunCallbacks
{
    public static RC_RPCManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI leftPlayerScoreText;  // For MasterClient
    public TextMeshProUGUI rightPlayerScoreText; // For other player
    public GameObject winnerLeftGO;  // Assign in Inspector
    public GameObject winnerRightGO;
    private int masterScore = 0;
    private int otherScore = 0;
    private PhotonView photonView;

    private bool winnerIsLeft;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        photonView = GetComponent<PhotonView>();
    }
    private void Start()
    {
        InitScroeText();
    }
    public void InitScroeText()
       
    {
        if (PhotonManager.Instance.singlePlayermatch)
        {
            leftPlayerScoreText.text = "00";
            rightPlayerScoreText.text = "";
        }
        else
        {

            leftPlayerScoreText.text = "00";
            rightPlayerScoreText.text = "00";
        }
    }
    public int GetLeftPlayerScore() => PhotonNetwork.IsMasterClient ? masterScore : otherScore;
    public int GetRightPlayerScore() => PhotonNetwork.IsMasterClient ? otherScore : masterScore;
    

    public bool GetWinnerIsLeft()
    {
        return winnerIsLeft;
    }
    [PunRPC]
    public void ConfirmCarSpawned()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameController.Instance.spawnedPlayerCount++;

            if (GameController.Instance.spawnedPlayerCount >= 2)
            {
                CountDown();
            }
        }
    }
    public void ConfirmPlayers()
    {
        photonView.RPC("ConfirmCarSpawned", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void RPC_StartCountdown()
    {
        StartCountdown();
    }
    public void CountDown()
    {
        photonView.RPC("RPC_StartCountdown", RpcTarget.All);
    }
    public void StartCountdown()
    {
        StartCoroutine(RC_UIManager.Instance.CountdownRoutine());
    }
    public void ShowResult(bool leftWins)
    {
        winnerIsLeft = leftWins;
        photonView.RPC("ShowWinner", RpcTarget.All, leftWins);
    }
    [PunRPC]
    public void ShowWinner(bool leftPlayerWins)
    {
        winnerLeftGO.SetActive(leftPlayerWins);
        winnerRightGO.SetActive(!leftPlayerWins);
    }
    private void UpdateScoreUI(int actorNumber, int score)
    {
        if (PhotonNetwork.MasterClient != null && actorNumber == PhotonNetwork.MasterClient.ActorNumber)
        {
            // Score belongs to the MasterClient → always show on left
            leftPlayerScoreText.text = score.ToString();
            Debug.Log($"[ScoreUI] MasterClient score updated: {score}");
        }
        else
        {
            // Score belongs to the non-master player → always show on right
            rightPlayerScoreText.text = score.ToString();
            Debug.Log($"[ScoreUI] Other player's score updated: {score}");
        }

        StoreScore(actorNumber, score);
    }


    [PunRPC]
    public void UpdateScoreForPlayer(int actorNumber, int newScore)
    {
        // 🚫 Prevent self from re-updating their own score through the RPC
        if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Debug.LogWarning("Received RPC for own actor — skipping update.");
            return;
        }

        // ✅ Update opponent's score only
        UpdateScoreUI(actorNumber, newScore);
    }


    public void StoreScore(int actorNumber, int score)
    {
        if (PhotonNetwork.IsMasterClient == true)
            masterScore = score;
        else
            otherScore = score;
    }

    public void SetMyScore(int newScore)
    {
        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        // 1. Update score locally
        UpdateScoreUI(myActorNumber, newScore);

        // 2. Send score to opponent only (RpcTarget.Others)
        photonView.RPC("UpdateScoreForPlayer", RpcTarget.Others, myActorNumber, newScore);
    }


}
