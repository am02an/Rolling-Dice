using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class RC_GameManager : MonoBehaviourPunCallbacks
{
    public static RC_GameManager Instance;

    public TextMeshProUGUI fpsText;

    [Header("Spawn Settings")]
    public Transform spawnPointMasterClient;
    public Transform spawnPointOtherPlayer;

    [Header("Car Prefab")]
    public GameObject carPrefab;
    public GameObject opponentPrefab;
    public GameObject Aicar;
    [Header("Results")]

    public GameObject SingleplayerResult;
    public GameObject win;
    public GameObject lose;
    [Header("SinglePlayer Results")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI Dptext;
    public TextMeshProUGUI XpText;
    [Header("PVP Lose Results")]
    public TextMeshProUGUI LosecoinText;
    public TextMeshProUGUI LoseDptext;
    public TextMeshProUGUI LoseXpText;
    [Header("PVP Win Results")]
    public TextMeshProUGUI WincoinText;
    public TextMeshProUGUI WinDptext;
    public TextMeshProUGUI WinXpText;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.SetState(GameState.InGame);

        LoadingScreenManager.Instance.HideLoadingScreen();
        if (PhotonManager.Instance.isAIMatch)
        {
            SpawnForAIMatch();
        }
        else if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnCarForPlayer();
        }

        GameController.Instance.Initcars();
       // Common init
    }

    private float deltaTime;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();
    }

    public float GetCurrentFPS()
    {
        return 1.0f / deltaTime;
    }

    private GameObject myCarInstance;
    private GameObject opponentCarInstance;

    void SpawnCarForPlayer()
    {
        GameObject carToSpawn;
        Transform spawnPoint;
        if (PhotonNetwork.IsMasterClient)
        {
            // Master spawns their own car at master spawn point
            carToSpawn = carPrefab;
            spawnPoint = spawnPointMasterClient;
            RC_RPCManager.Instance.ConfirmPlayers();
        }
        else
        {
            // Non-master spawns opponent (master)'s car at other player spawn point
            RC_RPCManager.Instance.ConfirmPlayers();
            carToSpawn = opponentPrefab;
            spawnPoint = spawnPointOtherPlayer;
        }

        GameObject car = PhotonNetwork.Instantiate(carToSpawn.name, spawnPoint.position, spawnPoint.rotation);

        // ActorNumber of owner (who controls the car)
        int actorNumber = car.GetComponent<PhotonView>().Owner.ActorNumber;

        Debug.Log($"[Multiplayer] Spawned car for ActorNumber: {actorNumber} at {spawnPoint.name}");
        //RC_RPCManager.Instance.ConfirmPlayers();
        // Optionally store the instance
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            myCarInstance = car;
        else
            opponentCarInstance = car;
    }
    void SpawnForAIMatch()
    {
        // Local player car (photonView.IsMine == true)
        GameObject playerCar = PhotonNetwork.Instantiate(carPrefab.name, spawnPointMasterClient.position, spawnPointMasterClient.rotation);

        // AI opponent (can be non-networked since it's controlled locally)
        // GameObject aiCar = Instantiate(Aicar, spawnPointOtherPlayer.position, spawnPointOtherPlayer.rotation);

        Debug.Log("[AI Match] Spawned player car and AI opponent.");
    }
    public void CheckForSoloWin()
    {
        StartCoroutine(WaitAndCheckWinner());
    }

    private IEnumerator WaitAndCheckWinner()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            Debug.Log("Only one player detected. Waiting 5 seconds...");

            yield return new WaitForSeconds(5f);

            // Check again after waiting, in case someone joined
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                Debug.Log("Still only one player. Declaring win.");
                //winText.text = "You Win! (Opponent left)";
                //winText.gameObject.SetActive(true);
                win.gameObject.SetActive(true);
                yield return new WaitForSeconds(2);
               RC_UIManager.Instance. MoveToRaceMainMenu();
                // You can also end game or return to lobby
                // PhotonNetwork.LeaveRoom();
                // SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.Log("Another player joined within 5 seconds. Continue game.");
            }
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        CheckForSoloWin();
    }

}
