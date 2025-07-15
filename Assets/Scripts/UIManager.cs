using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Slider player1Slider;
    public Slider player2Slider;

    public TMP_Text countdownText;

    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    public GameObject player1Prefab;
    public GameObject player2Prefab;
    public GameObject victoryPanel;
    public GameObject defeatPanel;


    private GameObject player1Obj;
    private GameObject player2Obj;
    public DiceRoller[] diceRoller;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        InitPlayers();
        InitUI();
        StartCoroutine(StartCountdown());

    }

    void InitPlayers()
    {
        // Real player (Player 1) instantiated using Photon
        player1Obj = PhotonNetwork.Instantiate("Player1", player1SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
        Camera.main.GetComponent<CameraController>().player1 = player1Obj.transform;
        diceRoller[0].playerObject = player1Obj.transform;

        // Bot (Player 2) instantiated locally
        player2Obj = Instantiate(player2Prefab, player2SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
        diceRoller[1].playerObject = player2Obj.transform;
    }


    public void SetPlayerThroughDice(DiceRoller diceRoller, int diceID)
    {
        if (diceID == 1)
        {
            diceRoller.playerObject = player1Obj.transform;
        }
        else if (diceID == 2)
        {
            diceRoller.playerObject = player2Obj.transform;
        }
    }

    void InitUI()
    {
        player1Slider.maxValue = GameManager.Instance.planeCount;
        player2Slider.maxValue = GameManager.Instance.planeCount;
        player1Slider.value = 0;
        player2Slider.value = 0;
        countdownText.text = "";
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.transform.parent.gameObject.SetActive(true);
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.transform.parent.gameObject.SetActive(false);
        countdownText.text = "";

        // Here you can trigger movement start
        // e.g., player1Obj.GetComponent<PlayerController>().StartMoving();
    }

    // Optional: update slider progress
    public void UpdatePlayerProgress(int diceID, int currentTileIndex, int totalTiles)
    {
        float progress =currentTileIndex;
       

        if (diceID == 1)
        {
            player1Slider.DOKill(); // Stop any ongoing tween
            player1Slider.DOValue(progress, 0.4f).SetEase(Ease.OutQuad);
        }
        else if (diceID == 2)
        {
            player2Slider.DOKill();
            player2Slider.DOValue(progress, 0.4f).SetEase(Ease.OutQuad);
        }
    }
    public void ShowVictory()
    {
        // Activate your victory UI
        victoryPanel.SetActive(true);
        GameManager.Instance.BackToLobby();
    }

    public void ShowDefeat()
    {
        // Activate your defeat UI
        defeatPanel.SetActive(true); // example
        GameManager.Instance.BackToLobby();
    }
    


}
