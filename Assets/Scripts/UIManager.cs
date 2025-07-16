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

        if (PhotonManager.Instance.isAIMatch)
        {
            StartCoroutine(StartCountdown());
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            RPCManager.Instance.SendStartCountdown();
        }



    }

    void InitPlayers()
    {
        if (PhotonManager.Instance.isAIMatch)
        {
            // AI Match: Player 1 (real), Player 2 (bot)
            player1Obj = PhotonNetwork.Instantiate("Player1", player1SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
            Camera.main.GetComponent<CameraController>().player1 = player1Obj.transform;
            diceRoller[0].playerObject = player1Obj.transform;

            player2Obj = Instantiate(player2Prefab, player2SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
            Camera.main.GetComponent<CameraController>().player2 = player2Obj.transform;
            diceRoller[1].playerObject = player2Obj.transform;
        }
        else
        {
            // Multiplayer match: decide player prefab based on ActorNumber
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                player1Obj = PhotonNetwork.Instantiate("Player1", player1SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
                Camera.main.GetComponent<CameraController>().player1 = player1Obj.transform;
                diceRoller[0].playerObject = player1Obj.transform;
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                player2Obj = PhotonNetwork.Instantiate("Player2", player2SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
                Camera.main.GetComponent<CameraController>().player2 = player2Obj.transform;
                diceRoller[1].playerObject = player2Obj.transform;
            }
        }
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

    public IEnumerator StartCountdown()
    {
        Transform container = countdownText.transform.parent;
        container.gameObject.SetActive(true);

        string[] countdownSteps = { "3", "2", "1", "GO!" };

        foreach (string step in countdownSteps)
        {
            countdownText.text = step;
            countdownText.color = new Color(1, 1, 1, 0); // transparent

            // Scale pop animation
            countdownText.transform.localScale = Vector3.zero;
            countdownText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            // Fade in
            countdownText.DOFade(1f, 0.3f);

            yield return new WaitForSeconds(1f);
        }

        // Fade out and scale down after "GO!"
        countdownText.DOFade(0f, 0.3f);
        countdownText.transform.DOScale(Vector3.zero, 0.3f);
        yield return new WaitForSeconds(0.4f);

        container.gameObject.SetActive(false);
        countdownText.text = "";
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
    public void UpdateDiceFace(int diceID, int faceIndex)
    {
        diceRoller[diceID - 1].diceImage.sprite = diceRoller[diceID - 1].diceFaces[faceIndex];
    }



}
