using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;

public class RC_UIManager : MonoBehaviourPunCallbacks
{
    public static RC_UIManager Instance;
    public string sceneName;
    [Header("UI Elements")]
    public TextMeshProUGUI driftPoint;
    public TextMeshProUGUI totalDriftText;
    public TextMeshProUGUI driftPopupText;
    public TextMeshProUGUI alertText;
    public TextMeshProUGUI timerforAlert;
    public TextMeshProUGUI resultText;
    [Header("CountDown")]
    public TextMeshProUGUI countdownText;
    public float delayBetweenCounts = 1f;
    public TextMeshProUGUI timerText;

    public float totalTimeInSeconds = 90f; // 1 minute 30 seconds

    private float currentTime;
    private bool isTimerRunning = false;

    public int dragPoints = 0; 
    public float totalDriftPoints = 0f;
    public bool CanStartRace=false;

    private void Awake()
    {
        Instance = this;
        if (sceneName == "RacingGame")
        {
            driftPoint.text = "";
            driftPoint.transform.localScale = Vector3.zero;
           

            totalDriftText.text = "00";
            totalDriftText.transform.localScale = Vector3.one;

            driftPopupText.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitForPlayersThenStartCountdown());
        }

    }
    private IEnumerator WaitForPlayersThenStartCountdown()
    {
        if (!PhotonManager.Instance.singlePlayermatch)
        {
            while (GameController.Instance.spawnedPlayerCount < 2)
            {
                yield return null; // wait 1 frame and check again
            }
        }

        RC_RPCManager.Instance.CountDown();
    }
    public IEnumerator CountdownRoutine()
    {
        AudioManager.Instance.SetBGMVolume(0.1f);
        string[] countdownStrings = { "3", "2", "1", "GO!" };

        foreach (string count in countdownStrings)
        {
            countdownText.text = count;
            countdownText.color = new Color(1, 1, 1, 0);
            countdownText.transform.localScale = Vector3.zero;

            // Animate scale up + fade in
            Sequence seq = DOTween.Sequence();
            seq.Append(countdownText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            seq.Join(countdownText.DOFade(1f, 0.5f));
            seq.AppendInterval(0.5f);
            seq.Append(countdownText.DOFade(0f, 0.3f));
            seq.Play();

            // Play sound
            if (count == "GO!")
                AudioManager.Instance.PlaySound(AudioManager.Instance.goSound); // Replace `goSound` with your actual AudioClip
            else
                AudioManager.Instance.PlaySound(AudioManager.Instance.countDown);

            yield return new WaitForSeconds(delayBetweenCounts);
        }

        countdownText.gameObject.SetActive(false);
        CanStartRace = true;
        StartTimer();
        // Trigger game start logic here
    }

    public void StartMatch()
    {
        LobbyUI.Instance.SetGameToPlay("RacingGame");
    }
    void StartTimer()
    {
        currentTime = totalTimeInSeconds;
        isTimerRunning = true;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                DisplayTime(currentTime);
            }
            else
            {
                currentTime = 0;
                isTimerRunning = false;
                DisplayTime(0);
               StartCoroutine( ShowDragPointTotal());
            }
        }
    }

    void DisplayTime(float timeToShow)
    {
        float minutes = Mathf.FloorToInt(timeToShow / 60);
        float seconds = Mathf.FloorToInt(timeToShow % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    IEnumerator ShowDragPointTotal()
    {
       

        int myScore = (int)totalDriftPoints;

        // Send your score to others


        // Wait for synchronization
        yield return new WaitForSeconds(1f);

        // Only Master decides result
        if (PhotonNetwork.IsMasterClient)
        {
            int leftScore = RC_RPCManager.Instance.GetLeftPlayerScore();
            int rightScore = RC_RPCManager.Instance.GetRightPlayerScore();

            if(PhotonManager.Instance.singlePlayermatch)
            {
                resultText.text = $"Total Drag Points:\nLeft: {leftScore}";
            }
            else
            {

            }
            // Show total points
            resultText.text = $"Total Drag Points:\nLeft: {leftScore}\nRight: {rightScore}";

            bool leftWins = leftScore > rightScore;

            // Send result to all clients
            RC_RPCManager.Instance.ShowResult(leftWins);
        }

        // Wait for RPC to apply result
        yield return new WaitForSeconds(0.5f);

        // Show rewards and win/lose UI
        yield return ShowWinLoseAndReward();

        yield return new WaitForSeconds(5f);

        MoveToRaceMainMenu();
    }

    IEnumerator ShowWinLoseAndReward()
    {
        int myScore = (int)totalDriftPoints;

        // Reward Calculation
        int rewardCoins = Mathf.RoundToInt(myScore * 0.5f); // 0.5 coins per DP
        int rewardXP = Mathf.RoundToInt(myScore * 0.2f);    // 0.2 XP per DP

        if (PhotonManager.Instance.isAIMatch)
        {
            // ---------- SINGLEPLAYER ----------
            RC_GameManager.Instance.SingleplayerResult.SetActive(true);
            RC_GameManager.Instance.coinText.text = rewardCoins.ToString();
            RC_GameManager.Instance.Dptext.text = myScore.ToString();
            RC_GameManager.Instance.XpText.text = rewardXP.ToString();
        }
        else
        {
            // ---------- MULTIPLAYER (PVP) ----------
            bool isMaster = PhotonNetwork.IsMasterClient;
            int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
            int masterActor = PhotonNetwork.MasterClient.ActorNumber;

            bool iWon = false;

            if (RC_RPCManager.Instance.GetWinnerIsLeft())
            {
                iWon = myActor == masterActor;
            }
            else
            {
                iWon = myActor != masterActor;
            }

            if (iWon)
            {
                RC_GameManager.Instance.win.SetActive(true);
                RC_GameManager.Instance.WincoinText.text = rewardCoins.ToString();
                RC_GameManager.Instance.WinDptext.text = myScore.ToString();
                RC_GameManager.Instance.WinXpText.text = rewardXP.ToString();
            }
            else
            {
                RC_GameManager.Instance.lose.SetActive(true);
                RC_GameManager.Instance.LosecoinText.text = rewardCoins.ToString();
                RC_GameManager.Instance.LoseDptext.text = myScore.ToString();
                RC_GameManager.Instance.LoseXpText.text = rewardXP.ToString();
            }
        }

        // Save rewards
        RC_MainMenuUI.Instance.AddCoins(rewardCoins);
        RC_MainMenuUI.Instance.AddXP(rewardXP);

        yield return null;
    }


    public float lastDriftStart = 0f;

    public void ShowDrift(float currentDrift)
    {
        // Difference from where this drift started
        float displayDrift = currentDrift - lastDriftStart;
        int roundedDrift = Mathf.FloorToInt(currentDrift);

        driftPoint.text = $"+{roundedDrift}";

        // Show updated total (live preview, not permanent yet)
        RC_RPCManager.Instance.SetMyScore((int)(totalDriftPoints + roundedDrift));

        // Color logic
        if (displayDrift >= 1000)
            driftPoint.color = Color.red;
        else if (displayDrift >= 500)
            driftPoint.color = Color.yellow;
        else
            driftPoint.color = Color.white;

        // Animate
        driftPoint.transform.DOKill();
        driftPoint.transform.localScale = Vector3.one * 1.4f;
        driftPoint.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }



    public void MoveToRaceMainMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            LoadMainMenu();
        }
    }

    public override void OnLeftRoom()
    {
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("RacingMainMenu");
    }
    public void ShowTotalDrift(float total)
    {
        totalDriftPoints = Mathf.FloorToInt(total);
        totalDriftText.text = $"{totalDriftPoints}";
    }

    public void HideDrift(PhotonView view)
    {
        driftPoint.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
    }

    //public void EndDrift(float score)
    //{
    //    // Add to total only once per drift end
    //    totalDriftPoints += score;

    //    Debug.Log($"[Drift Ended] Score: {score}, Total: {totalDriftPoints}");

    //    totalDriftText.text = $"Total Drift: {Mathf.FloorToInt(totalDriftPoints)}";

    //    HideDrift();

    //    // Optional effects
    //    if (score >= 3000)
    //        ShowPerfectDrift();
    //}

    public void ShowPerfectDrift()
    {
        StartCoroutine(ShowPopup("Perfect Drift!", Color.red));
    }

    public void ShowDriftChain(int chainCount)
    {
        StartCoroutine(ShowPopup($"Drift Chain x{chainCount}!", Color.yellow));
    }

    private IEnumerator ShowPopup(string message, Color color)
    {
        driftPopupText.text = message;
        driftPopupText.color = color;
        driftPopupText.gameObject.SetActive(true);
        driftPopupText.transform.localScale = Vector3.zero;

        driftPopupText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(1.2f);

        driftPopupText.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.3f);
        driftPopupText.gameObject.SetActive(false);
    }
}
