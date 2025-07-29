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
    }
    public int GetLeftPlayerScore() => PhotonNetwork.IsMasterClient ? masterScore : otherScore;
    public int GetRightPlayerScore() => PhotonNetwork.IsMasterClient ? otherScore : masterScore;

    [PunRPC]
    public void RPC_StartCountdown()
    {
        StartCountdown();
    }
    public void StartCountdown()
    {
        StartCoroutine(RC_UIManager.Instance. CountdownRoutine());
    }

    [PunRPC]
    public void ShowWinner(bool leftPlayerWins)
    {
        winnerLeftGO.SetActive(leftPlayerWins);
        winnerRightGO.SetActive(!leftPlayerWins);
    }

    [PunRPC]
    public void UpdateScoreForSide(bool isMasterClient, int newScore)
    {
        if (PhotonNetwork.IsMasterClient == isMasterClient)
            leftPlayerScoreText.text = newScore.ToString();
        else
            rightPlayerScoreText.text = newScore.ToString();

        StoreScore(isMasterClient, newScore);
    }

    public void StoreScore(bool isMaster, int score)
    {
        if (isMaster)
            masterScore = score;
        else
            otherScore = score;
    }

    public void SetMyScore(int newScore)
    {
        bool isMaster = PhotonNetwork.IsMasterClient;
        photonView.RPC("UpdateScoreForSide", RpcTarget.All, isMaster, newScore);
    }

}
