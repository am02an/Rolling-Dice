using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    #region UI Elements
    public Slider player1Slider;
    public Slider player2Slider;
    public TMP_Text countdownText;
    public TextMeshProUGUI player1Score;
    public TextMeshProUGUI player2Score;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    #endregion

    #region Player Spawn Setup
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    #endregion

    #region Internal References
    private GameObject player1Obj;
    private GameObject player2Obj;
    public DiceRoller[] diceRoller;
    #endregion

    #region Unity Callbacks
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
    #endregion

    #region Initialization
    void InitPlayers()
    {
        if (PhotonManager.Instance.isAIMatch)
        {
            player1Obj = PhotonNetwork.Instantiate("Player1", player1SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
            Camera.main.GetComponent<Dice_CameraController>().player1 = player1Obj.transform;
            diceRoller[0].playerObject = player1Obj.transform;

            player2Obj = Instantiate(player2Prefab, player2SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
            Camera.main.GetComponent<Dice_CameraController>().player2 = player2Obj.transform;
            diceRoller[1].playerObject = player2Obj.transform;
        }
        else
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                player1Obj = PhotonNetwork.Instantiate("Player1", player1SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
                Camera.main.GetComponent<Dice_CameraController>().player1 = player1Obj.transform;
                diceRoller[0].playerObject = player1Obj.transform;
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                player2Obj = PhotonNetwork.Instantiate("Player2", player2SpawnPoint.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
                Camera.main.GetComponent<Dice_CameraController>().player2 = player2Obj.transform;
                diceRoller[1].playerObject = player2Obj.transform;
            }
        }
    }

    void InitUI()
    {
        player1Slider.maxValue = GameManager.Instance.planeCount;
        player2Slider.maxValue = GameManager.Instance.planeCount;
        player1Slider.value = 0;
        player2Slider.value = 0;
        countdownText.text = "";
        player1Score.text ="0";
        player2Score.text ="0";
    }
    #endregion

    #region Gameplay Helpers
    public DiceRoller GetDiceByID(int id)
    {
        foreach (var dice in diceRoller)
        {
            if (dice != null && dice.DiceID == id)
                return dice;
        }
        return null;
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

    public void UpdatePlayerProgress(int diceID, int currentTileIndex, int totalTiles)
    {
        float progress = currentTileIndex;

        if (diceID == 1)
        {
            player1Slider.DOKill();
            player1Slider.DOValue(progress, 0.4f).SetEase(Ease.OutQuad);
            player1Score.text =  currentTileIndex.ToString();
        }
        else if (diceID == 2)
        {
            player2Slider.DOKill();
            player2Slider.DOValue(progress, 0.4f).SetEase(Ease.OutQuad);
            player2Score.text =  currentTileIndex.ToString();
        }
    }

   
    public void UpdateDiceFace(int diceID, int faceIndex)
    {
        diceRoller[diceID - 1].diceImage.sprite = diceRoller[diceID - 1].diceFaces[faceIndex];
    }
    #endregion

    #region Countdown Animation
    public IEnumerator StartCountdown()
    {
        Transform container = countdownText.transform.parent;
        container.gameObject.SetActive(true);

        string[] countdownSteps = { "3", "2", "1", "GO!" };

        foreach (string step in countdownSteps)
        {
            countdownText.text = step;
            countdownText.color = new Color(1, 1, 1, 0);

            countdownText.transform.localScale = Vector3.zero;
            countdownText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            countdownText.DOFade(1f, 0.3f);

            yield return new WaitForSeconds(1f);
        }

        countdownText.DOFade(0f, 0.3f);
        countdownText.transform.DOScale(Vector3.zero, 0.3f);
        yield return new WaitForSeconds(0.4f);

        container.gameObject.SetActive(false);
        countdownText.text = "";
    }
    #endregion

    #region Game End
    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
        GameManager.Instance.BackToLobby();
    }

    public void ShowDefeat()
    {
        defeatPanel.SetActive(true);
        GameManager.Instance.BackToLobby();
    }
    #endregion
}
