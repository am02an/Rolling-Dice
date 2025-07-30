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
    [Header("CountDown")]
    public TextMeshProUGUI countdownText;
    public float delayBetweenCounts = 1f;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;

    public float totalTimeInSeconds = 90f; // 1 minute 30 seconds

    private float currentTime;
    private bool isTimerRunning = false;

    public int dragPoints = 0; 
    private float totalDriftPoints = 0f;
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
            RC_RPCManager.Instance.CountDown();
          
        }

    }
    public IEnumerator CountdownRoutine()
    {
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
        resultText.transform.parent.gameObject.SetActive(true);

        int myScore = (int)totalDriftPoints;

        // Send your score to everyone
        RC_RPCManager.Instance.SetMyScore(myScore);

        // Wait a bit for RPCs to sync
        yield return new WaitForSeconds(1f);

        // Only MasterClient decides winner
        if (PhotonNetwork.IsMasterClient)
        {
            int leftScore = RC_RPCManager.Instance.GetLeftPlayerScore();
            int rightScore = RC_RPCManager.Instance.GetRightPlayerScore();

            resultText.text = $"Total Drag Points:\nLeft: {leftScore}\nRight: {rightScore}";

            // Send winner info to all clients
            bool leftWins = leftScore > rightScore;
           // photonView.RPC("ShowWinner", RpcTarget.All, leftWins);
        }

        yield return new WaitForSeconds(3);
        MoveToRaceMainMenu();
    }
    public void ShowDrift(float currentDrift)
    {


        driftPoint.text = $"+{Mathf.FloorToInt(currentDrift)}";

        if (currentDrift >= 1000)
            driftPoint.color = Color.red;
        else if (currentDrift >= 500)
            driftPoint.color = Color.yellow;
        else
            driftPoint.color = Color.white;

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
