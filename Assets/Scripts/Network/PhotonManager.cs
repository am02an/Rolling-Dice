using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;

    #region Matchmaking Settings
    public float matchmakingTimeout = 5f;
    public bool allowAIMatch = true;
    public bool isAIMatch;
    public bool singlePlayermatch;


    public string stringGameName="RollingGame";
    #endregion

    #region UI References
    public GameObject matchMaking;
    [SerializeField] private GameObject StartButton;
    public List<Sprite> opponentSprites;
    public float spriteChangeInterval = 0.3f;
    private Coroutine characterCycleCoroutine;

    #endregion

    #region Opponent Animation
    public float moveSpeed = 20f;
    public float moveRange = 30f;
    private bool isSearching = false;
    private Vector2 startPos;
    #endregion

    #region Photon Role Flags
    public bool IsMasterClient;
    public bool IsOtherPlayer;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(matchMaking);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartButton.transform.localScale = Vector3.zero;
        StartCoroutine(ShowStartButtonWhenReady());
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion

    #region Match Start Logic
    private IEnumerator ShowStartButtonWhenReady()
    {
        // Wait until the input field is not empty
        yield return new WaitUntil(() => !string.IsNullOrWhiteSpace(stringGameName));

        // Animate the button
        if (StartButton != null)
        {
            StartButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }
    public void StartMatch()
    {
        if (StartButton != null)
        {

            StartButton.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
        }
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ClickSound);

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.Server == ServerConnection.MasterServer)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            StartCoroutine(WaitForPhotonConnectionThenJoin());
        }
    }

    private IEnumerator WaitForPhotonConnectionThenJoin()
    {
        while (PhotonNetwork.Server != ServerConnection.MasterServer || !PhotonNetwork.IsConnectedAndReady)
        {
            yield return null;
        }

        PhotonNetwork.JoinRandomRoom();
    }
    #endregion

    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connect to master");
      StartCoroutine(  UIUtils.FadeCanvasGroup("Lobby", 1, 0.2f, true));
        //if (StartButton == null)
        //{
        //    StartButton = GameObject.FindGameObjectWithTag("StartButton");

        //    if (StartButton != null)
        //    {
        //        StartButton.GetComponent<Button>().onClick.AddListener(StartMatch);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("StartButton (Button_Play) not found in the scene.");
        //        return;
        //    }
        //}

        //StartButton.transform.localScale = Vector3.zero;
        //StartButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        IsMasterClient = PhotonNetwork.IsMasterClient;
        IsOtherPlayer = !PhotonNetwork.IsMasterClient;
        matchMaking.GetComponent<Canvas>().sortingOrder = 2;
        StartCoroutine(UIUtils.FadeCanvasGroup("Play_Battle", 1f, 0.5f, true));
        if (singlePlayermatch)
        {
            matchMaking.GetComponent<Canvas>().sortingOrder = -1;
            StartCoroutine(UIUtils.FadeCanvasGroup("Play_Battle", 0f, 0.5f, false));

            PhotonNetwork.LoadLevel(stringGameName);
        }
        else
        {
            StartCoroutine(WaitForOpponent());
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            DeclareVictoryToRemainingPlayer();
        }
    }

    private void DeclareVictoryToRemainingPlayer()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            UIManager.Instance.ShowVictory();
        }
    }
    #endregion

    #region Opponent Search
    private IEnumerator WaitForOpponent()
    {
        while (true)
        {
            isSearching = true;
            startPos = LobbyUI.Instance.player2Image.rectTransform.anchoredPosition;
            characterCycleCoroutine = StartCoroutine(CycleOpponentImages());

            float timer = 0f;
            float timeLeft = matchmakingTimeout;

            while (PhotonNetwork.CurrentRoom.PlayerCount < 2 && timer < matchmakingTimeout)
            {
                timer += Time.deltaTime;
                timeLeft = Mathf.Max(0f, matchmakingTimeout - timer);
                LobbyUI.Instance.startTimeText.text = $"Searching for player... <color=#00FF00>{Mathf.FloorToInt(timeLeft)}s</color>";

                float offset = Mathf.PingPong(Time.time * moveSpeed, moveRange);
                LobbyUI.Instance.player2Image.rectTransform.anchoredPosition = startPos + new Vector2(offset, 0);

                yield return null;
            }

            isSearching = false;

            if (characterCycleCoroutine != null)
                StopCoroutine(characterCycleCoroutine);

            yield return StartCoroutine(SmoothStopPlayerImage());

            int finalIndex = Random.Range(0, opponentSprites.Count);
            LobbyUI.Instance.player2Image.sprite = opponentSprites[finalIndex];

            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                isAIMatch = false;
                LobbyUI.Instance.startTimeText.text = "Player found!";
                yield return new WaitForSeconds(2f);
                matchMaking.GetComponent<Canvas>().sortingOrder = -1;
                StartCoroutine(UIUtils.FadeCanvasGroup("Play_Battle", 0f, 0.5f, false));
                LoadingScreenManager.Instance.ShowLoadingScreen();

                PhotonNetwork.LoadLevel(stringGameName);
                yield break;
            }
            else if (allowAIMatch)
            {
                isAIMatch = true;
                PlayerPrefs.SetInt("PlayWithBot", 1);
                LobbyUI.Instance.startTimeText.text = "No player found. Starting with bot...";
                yield return new WaitForSeconds(2f);
                matchMaking.GetComponent<Canvas>().sortingOrder = -1;
                StartCoroutine(UIUtils.FadeCanvasGroup("Play_Battle", 0f, 0.5f, false));
                PhotonNetwork.LoadLevel(stringGameName);
                yield break;
            }
            else
            {
                LobbyUI.Instance.startTimeText.text = "No player found. Retrying matchmaking...";
                yield return new WaitForSeconds(2f);
            }
        }
    }

    private IEnumerator CycleOpponentImages()
    {
        int index = 0;
        while (isSearching)
        {
            LobbyUI.Instance.player2Image.sprite = opponentSprites[index];
            index = (index + 1) % opponentSprites.Count;
            yield return new WaitForSeconds(spriteChangeInterval);
        }
    }

    private IEnumerator SmoothStopPlayerImage()
    {
        Vector2 currentPos = LobbyUI.Instance.player2Image.rectTransform.anchoredPosition;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            LobbyUI.Instance.player2Image.rectTransform.anchoredPosition = Vector2.Lerp(currentPos, startPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        LobbyUI.Instance.player2Image.rectTransform.anchoredPosition = startPos;
    }
    #endregion
}
