using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class RC_GameManager : MonoBehaviourPunCallbacks
{
    public static RC_GameManager Instance;

    public TextMeshProUGUI fpsText; // Assign a UI Text element in the inspector
    [Header("Spawn Settings")]
    public Transform spawnPointMasterClient;
    public Transform spawnPointOtherPlayer;

    [Header("Car Prefab")]
    public GameObject carPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnCarForPlayer();
        }
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
    void SpawnCarForPlayer()
    {
        Transform spawnPoint = PhotonNetwork.IsMasterClient ? spawnPointMasterClient : spawnPointOtherPlayer;

        GameObject car = PhotonNetwork.Instantiate(carPrefab.name, spawnPoint.position, spawnPoint.rotation);
        FindObjectOfType<SCC_Camera>().playerCar = car.transform;
        FindObjectOfType<SCC_Dashboard>().car = car.GetComponent<SCC_Drivetrain>();
        // Optionally assign ownership, add player-specific color, camera, etc.
        Debug.Log($"Car spawned for {(PhotonNetwork.IsMasterClient ? "MasterClient" : "Other Player")} at {spawnPoint.name}");
    }
}
