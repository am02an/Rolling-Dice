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

    [PunRPC]
    public void ShowWinner(bool leftPlayerWins)
    {
        winnerLeftGO.SetActive(leftPlayerWins);
        winnerRightGO.SetActive(!leftPlayerWins);
    }
    private void UpdateScoreUI(int actorNumber, int score)
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            leftPlayerScoreText.text = score.ToString();
            Debug.Log($"[Local] Updated own score: {score}");
        }
        else
        {
            rightPlayerScoreText.text = score.ToString();
            Debug.Log($"[Local] Updated opponent score: {score}");
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
