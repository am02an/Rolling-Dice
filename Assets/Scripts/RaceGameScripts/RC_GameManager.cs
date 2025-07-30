using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

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

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (PhotonManager.Instance.isAIMatch)
        {
            SpawnForAIMatch();
        }
        else if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnCarForPlayer();
        }

        GameController.Instance.Initcars(); // Common init
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
        }
        else
        {
            // Non-master spawns opponent (master)'s car at other player spawn point
            carToSpawn = opponentPrefab;
            spawnPoint = spawnPointOtherPlayer;
        }

        GameObject car = PhotonNetwork.Instantiate(carToSpawn.name, spawnPoint.position, spawnPoint.rotation);

        // ActorNumber of owner (who controls the car)
        int actorNumber = car.GetComponent<PhotonView>().Owner.ActorNumber;

        Debug.Log($"[Multiplayer] Spawned car for ActorNumber: {actorNumber} at {spawnPoint.name}");

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

}
