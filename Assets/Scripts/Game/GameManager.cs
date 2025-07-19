using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
[System.Serializable]
public class TileData
{
    public int moveOffset = 0;
    public bool isFinish = false;
}

[System.Serializable]
public class TrackWrapper
{
    public List<TileData> tiles;
    public TrackWrapper(List<TileData> t) => tiles = t;
}

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public TextMeshProUGUI fpsText; // Assign a UI Text element in the inspector


    [Header("Track & Gameplay Setup")]
    public GameObject planePrefab;
    public int planeCount = 50;
    public float spacing = 2f;
    public Transform track1StartPoint, track2StartPoint;

    [Header("Dice & Turn Settings")]
    public DiceRoller playerDice, opponentDice;
    public float turnDelay = 1.5f;
    public int currentTurnActorNumber;

    // Internal Track Data
    private List<TileData> player1TrackData, player2TrackData;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerDice.OnRollComplete += OnPlayerRolled;
        opponentDice.OnRollComplete += OnOpponentRolled;

        if (!PhotonManager.Instance.isAIMatch)
            SetupMultiplayer();
        else
            SetupAIMatch();
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

    // ---------------- Multiplayer Setup ----------------

    private void SetupMultiplayer()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Debug.Log("Setting up as MasterClient");

            playerDice.actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            opponentDice.actorNumber = PhotonNetwork.PlayerListOthers[0].ActorNumber;
        }
        else
        {
            playerDice.actorNumber = PhotonNetwork.MasterClient.ActorNumber;
            opponentDice.actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        }

        playerDice.isMine = playerDice.GetComponent<PhotonView>().IsMine;
        opponentDice.isMine = opponentDice.GetComponent<PhotonView>().IsMine;

        if (PhotonNetwork.IsMasterClient)
        {
            player1TrackData = GenerateTrackData();
            player2TrackData = GenerateTrackData();

            RPCManager.Instance.SendTrackData(player1TrackData, player2TrackData);

            playerDice.tilePoints = SpawnTrackFromData(track1StartPoint.position, "Player1Track", player1TrackData);
            opponentDice.tilePoints = SpawnTrackFromData(track2StartPoint.position, "Player2Track", player2TrackData);

            RPCManager.Instance.SetTurnRPC(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    // ---------------- AI Match Setup ----------------

    private void SetupAIMatch()
    {
        player1TrackData = GenerateTrackData();
        player2TrackData = GenerateTrackData();

        playerDice.actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        opponentDice.actorNumber = 9999;

        playerDice.tilePoints = SpawnTrackFromData(track1StartPoint.position, "Player1Track", player1TrackData);
        opponentDice.tilePoints = SpawnTrackFromData(track2StartPoint.position, "Player2Track", player2TrackData);

        OnTurnChanged(playerDice.actorNumber);
    }

    // ---------------- Track Data Sync ----------------

    public void OnTrackDataReceived(List<TileData> data1, List<TileData> data2)
    {
        player1TrackData = data1;
        player2TrackData = data2;

        playerDice.tilePoints = SpawnTrackFromData(track1StartPoint.position, "Player1Track", data1);
        opponentDice.tilePoints = SpawnTrackFromData(track2StartPoint.position, "Player2Track", data2);
    }

    // ---------------- Turn Handling ----------------

    public void OnTurnChanged(int actorNumber)
    {
        currentTurnActorNumber = actorNumber;

        bool isMyTurn = PhotonNetwork.LocalPlayer.ActorNumber == actorNumber;

        Debug.Log($"Turn changed to actor: {actorNumber} (LocalPlayer: {PhotonNetwork.LocalPlayer.ActorNumber})");

        if (PhotonManager.Instance.isAIMatch)
        {
            playerDice.SetInteractable(isMyTurn);

            if (!isMyTurn && opponentDice.isAiControlled)
            {
                opponentDice.SetInteractable(true);
                opponentDice.StartRoll();
            }
            else
            {
                opponentDice.SetInteractable(false);
            }
        }
        else
        {
            playerDice.SetInteractable(false);
            opponentDice.SetInteractable(false);

            if (actorNumber == playerDice.actorNumber &&
                PhotonNetwork.LocalPlayer.ActorNumber == playerDice.actorNumber)
            {
                playerDice.SetInteractable(true);
                Debug.Log("My turn (playerDice)");
            }
            else if (actorNumber == opponentDice.actorNumber &&
                     PhotonNetwork.LocalPlayer.ActorNumber == opponentDice.actorNumber)
            {
                opponentDice.SetInteractable(true);
                Debug.Log("My turn (opponentDice)");
            }
        }
    }

    private void OnPlayerRolled(int value)
    {
        playerDice.SetInteractable(false);
        if (playerDice.actorNumber == currentTurnActorNumber)
            Invoke(nameof(SendNextTurn), turnDelay);
    }

    private void OnOpponentRolled(int value)
    {
        opponentDice.SetInteractable(false);
        if (opponentDice.actorNumber == currentTurnActorNumber)
            Invoke(nameof(SendNextTurn), turnDelay);
    }

    private void SendNextTurn()
    {
        int nextActor = GetNextActor();
        RPCManager.Instance.SetTurnRPC(nextActor);
    }

    public void OnDiceTurnComplete()
    {
        int nextActor = GetNextActor();
        RPCManager.Instance.SetTurnRPC(nextActor);
    }

    private int GetNextActor()
    {
        if (PhotonManager.Instance.isAIMatch)
        {
            return currentTurnActorNumber == PhotonNetwork.LocalPlayer.ActorNumber
                ? 9999
                : PhotonNetwork.LocalPlayer.ActorNumber;
        }

        Player[] players = PhotonNetwork.PlayerList;
        return currentTurnActorNumber == players[0].ActorNumber
            ? players[1].ActorNumber
            : players[0].ActorNumber;
    }

    // ---------------- Track Generation ----------------

    private List<TileData> GenerateTrackData()
    {
        List<TileData> data = new List<TileData>();

        for (int i = 0; i < planeCount; i++)
        {
            TileData tile = new TileData();

            if (i != planeCount - 1)
            {
                float chance = Random.value;

                if (chance < 0.1f && i > 10)
                    tile.moveOffset = -Random.Range(1, 6);
                else if (chance > 0.9f && i < planeCount - 10)
                    tile.moveOffset = Random.Range(1, 6);
            }

            tile.isFinish = (i == planeCount - 1);
            data.Add(tile);
        }

        return data;
    }

    public List<Transform> SpawnTrackFromData(Vector3 startPoint, string parentName, List<TileData> trackData)
    {
        GameObject trackParent = new GameObject(parentName);
        trackParent.transform.position = startPoint;
        trackParent.transform.SetParent(transform);

        List<Transform> tiles = new List<Transform>();

        for (int i = 0; i < trackData.Count; i++)
        {
            Vector3 pos = startPoint + new Vector3(0, 0, i * spacing);
            GameObject plane = Instantiate(planePrefab, pos, Quaternion.identity, trackParent.transform);
            TileAbility ability = plane.GetComponent<TileAbility>();

            if (ability != null)
            {
                if (i <= 2)
                    ability.moveOffset = 0;
                else
                    ability.moveOffset = trackData[i].moveOffset;

                if (trackData[i].isFinish)
                {
                    ability.finish = plane.transform.Find("Finish")?.gameObject;
                    if (ability.finish != null) ability.finish.SetActive(true);
                }
                else
                {
                    if (ability.finish != null) ability.finish.SetActive(false);
                }
            }

            tiles.Add(plane.transform);
        }

        return tiles;
    }

    // ---------------- Lobby Handling ----------------

    public void BackToLobby()
    {
        StartCoroutine(BackToLobbyRoutine());
    }

    private IEnumerator BackToLobbyRoutine()
    {
        yield return new WaitForSeconds(5f);

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        else
            PhotonNetwork.LoadLevel("Lobby");
    }

    public override void OnLeftRoom()
    {
        // This gets called after LeaveRoom is successful
        PhotonNetwork.LoadLevel("Lobby");
    }
}
