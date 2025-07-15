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
    public float matchmakingTimeout = 5f; // seconds
    [SerializeField] private TextMeshProUGUI startTimeText;
    [SerializeField] private GameObject StartButton;


    [Header("Selection")]
    public Image player2Image;
    public List<Sprite> opponentSprites; // Assign 5–7 sprites in Inspector
    public float spriteChangeInterval = 0.3f;
    public bool isAIMatch;
    private Coroutine characterCycleCoroutine;

    public CanvasGroup myPanelCanvasGroup;
    public float moveSpeed = 20f;
    public float moveRange = 30f;

    private bool isSearching = false;
    private Vector2 startPos;


    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
       
    }

    public void StartMatch()
    {
       

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.Server == ServerConnection.MasterServer)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Photon not connected. Connecting...");
            PhotonNetwork.ConnectUsingSettings(); // Ensure reconnecting if disconnected
        }
        else
        {
            Debug.LogWarning("Not on Master Server yet. Waiting...");
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


    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon");

        // Ensure the button starts hidden
        StartButton.transform.localScale = Vector3.zero;
        StartButton.gameObject.SetActive(true);

        // Tween scale up with a bounce
        StartButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: waiting for players...");
        StartCoroutine(UIUtils.FadeCanvasGroup(myPanelCanvasGroup, 1f, 0.5f, true));
        StartCoroutine(WaitForOpponent());
    }

    private IEnumerator CycleOpponentImages()
    {
        int index = 0;
        while (isSearching)
        {
            player2Image.sprite = opponentSprites[index];
            index = (index + 1) % opponentSprites.Count;
            yield return new WaitForSeconds(spriteChangeInterval);
        }
    }

    private IEnumerator WaitForOpponent()
    {
        isSearching = true;
        startPos = player2Image.rectTransform.anchoredPosition;

        // Start cycling character images
        characterCycleCoroutine = StartCoroutine(CycleOpponentImages());

        float timer = 0f;
        float timeLeft = matchmakingTimeout;

        while (PhotonNetwork.CurrentRoom.PlayerCount < 2 && timer < matchmakingTimeout)
        {
            timer += Time.deltaTime;
            timeLeft = Mathf.Max(0f, matchmakingTimeout - timer);

            // Countdown text
            startTimeText.text = $"Searching for player... <color=#00FF00>{Mathf.FloorToInt(timeLeft)}s</color>";



            // Motion (left/right)
            float offset = Mathf.PingPong(Time.time * moveSpeed, moveRange);
            player2Image.rectTransform.anchoredPosition = startPos + new Vector2(offset, 0);

            yield return null;
        }

        isSearching = false;

        // Stop cycling images
        if (characterCycleCoroutine != null)
            StopCoroutine(characterCycleCoroutine);

        // Smoothly stop motion and lock final character
        yield return StartCoroutine(SmoothStopPlayerImage());

        // Pick a final sprite to stop on (optional)
        int finalIndex = UnityEngine.Random.Range(0, opponentSprites.Count);
        player2Image.sprite = opponentSprites[finalIndex];

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            isAIMatch = false;
            startTimeText.text = "Player found!";
            yield return new WaitForSeconds(2f);
            PhotonNetwork.LoadLevel("MainScene");
        }
        else
        {
            isAIMatch = true;
            startTimeText.text = "No player found. Starting with bot...";
            PlayerPrefs.SetInt("PlayWithBot", 1);
            yield return new WaitForSeconds(2f);
            PhotonNetwork.LoadLevel("MainScene");
        }
    }


    private IEnumerator SmoothStopPlayerImage()
    {
        Vector2 currentPos = player2Image.rectTransform.anchoredPosition;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            player2Image.rectTransform.anchoredPosition = Vector2.Lerp(currentPos, startPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player2Image.rectTransform.anchoredPosition = startPos;
    }


}
