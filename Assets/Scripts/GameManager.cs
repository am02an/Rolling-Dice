using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[System.Serializable]
public class TileData { public int moveOffset = 0; public bool isFinish = false; }

[System.Serializable]
public class TrackWrapper
{
    public List<TileData> tiles;
    public TrackWrapper(List<TileData> t) => tiles = t;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject planePrefab;
    public int planeCount = 50;
    public float spacing = 2f;
    public Transform track1StartPoint, track2StartPoint;
    public DiceRoller playerDice, opponentDice;
    public float turnDelay = 1.5f;

    public int currentTurnActorNumber;
    private List<TileData> player1TrackData, player2TrackData;

    private void Awake() => Instance = this;

    void Start()
    {
        playerDice.OnRollComplete += OnPlayerRolled;
        opponentDice.OnRollComplete += OnOpponentRolled;

        if (!PhotonManager.Instance.isAIMatch)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Debug.Log("masterClient");

                playerDice.actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                opponentDice.actorNumber = PhotonNetwork.PlayerListOthers[0].ActorNumber;

                playerDice.isMine = playerDice.GetComponent<PhotonView>().IsMine;
                opponentDice.isMine = opponentDice.GetComponent<PhotonView>().IsMine;
            }
            else
            {
                playerDice.actorNumber = PhotonNetwork.MasterClient.ActorNumber;
                opponentDice.actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

                playerDice.isMine = playerDice.GetComponent<PhotonView>().IsMine;
                opponentDice.isMine = opponentDice.GetComponent<PhotonView>().IsMine;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                player1TrackData = GenerateTrackData();
                player2TrackData = GenerateTrackData();

                // Send data to other player
                RPCManager.Instance.SendTrackData(player1TrackData, player2TrackData);

                playerDice.tilePoints = SpawnTrackFromData(track1StartPoint.position, "Player1Track", player1TrackData);
                opponentDice.tilePoints = SpawnTrackFromData(track2StartPoint.position, "Player2Track", player2TrackData);

                // Start turn for multiplayer
                RPCManager.Instance.SetTurnRPC(PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }

        else
        {
            // --- AI MATCH SETUP ---

            // Generate tracks
            player1TrackData = GenerateTrackData();
            player2TrackData = GenerateTrackData();

            // Assign actor numbers (local player vs fake AI)
            playerDice.actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            opponentDice.actorNumber = 9999; // Fake actor number for AI

            // Assign tile paths
            playerDice.tilePoints = SpawnTrackFromData(track1StartPoint.position, "Player1Track", player1TrackData);
            opponentDice.tilePoints = SpawnTrackFromData(track2StartPoint.position, "Player2Track", player2TrackData);

            // Start turn with local player (you can randomize if needed)
            OnTurnChanged(playerDice.actorNumber);
        }
    }


    public void OnTrackDataReceived(List<TileData> data1, List<TileData> data2)
    {
        player1TrackData = data1;
        player2TrackData = data2;

        playerDice.tilePoints = SpawnTrackFromData(track1StartPoint.position, "Player1Track", player1TrackData);
        opponentDice.tilePoints = SpawnTrackFromData(track2StartPoint.position, "Player2Track", player2TrackData);
    }

    public void OnTurnChanged(int actorNumber)
    {
        currentTurnActorNumber = actorNumber;

        Debug.Log($"Turn changed to actor: {actorNumber}, LocalPlayer: {PhotonNetwork.LocalPlayer.ActorNumber}");

        if (PhotonManager.Instance.isAIMatch)
        {
            // --- AI TURN LOGIC ---
            bool isMyTurn = PhotonNetwork.LocalPlayer.ActorNumber == actorNumber;
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
            // --- MULTIPLAYER TURN LOGIC ---

            // Default both off
            playerDice.SetInteractable(false);
            opponentDice.SetInteractable(false);

            // Enable only for the current actor AND only on their local client
            if (actorNumber == playerDice.actorNumber &&
                PhotonNetwork.LocalPlayer.ActorNumber == playerDice.actorNumber)
            {
                playerDice.SetInteractable(true);
                Debug.Log("It's my turn (playerDice)");
            }
            else if (actorNumber == opponentDice.actorNumber &&
                     PhotonNetwork.LocalPlayer.ActorNumber == opponentDice.actorNumber)
            {
                opponentDice.SetInteractable(true);
                Debug.Log("It's my turn (opponentDice)");
            }
        }
    }
    void OnPlayerRolled(int value)
    {
        playerDice.SetInteractable(false);

        if (playerDice.actorNumber == currentTurnActorNumber)
        {
            Invoke(nameof(SendNextTurn), turnDelay);
        }
    }

    void OnOpponentRolled(int value)
    {
        opponentDice.SetInteractable(false);

        if (opponentDice.actorNumber == currentTurnActorNumber)
        {
            Invoke(nameof(SendNextTurn), turnDelay);
        }
    }

    public void OnDiceTurnComplete()
    {
        int nextActor = GetNextActor();
        RPCManager.Instance.SetTurnRPC(nextActor);
    }

    void SendNextTurn()
    {
        int nextActor = GetNextActor();
        RPCManager.Instance.SetTurnRPC(nextActor);
    }

    int GetNextActor()
    {
        if (PhotonManager.Instance.isAIMatch)
            return currentTurnActorNumber == PhotonNetwork.LocalPlayer.ActorNumber ? 9999 : PhotonNetwork.LocalPlayer.ActorNumber;

        Player[] players = PhotonNetwork.PlayerList;
        return currentTurnActorNumber == players[0].ActorNumber ? players[1].ActorNumber : players[0].ActorNumber;
    }

    List<TileData> GenerateTrackData()
    {
        List<TileData> data = new List<TileData>();
        for (int i = 0; i < planeCount; i++)
        {
            TileData tile = new TileData();
            if (i != planeCount - 1)
            {
                float chance = Random.value;
                if (chance < 0.1f && i > 10) tile.moveOffset = -Random.Range(1, 6);
                else if (chance > 0.9f && i < planeCount - 10) tile.moveOffset = Random.Range(1, 6);
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

        List<Transform> tileList = new List<Transform>();

        for (int i = 0; i < trackData.Count; i++)
        {
            Vector3 spawnPos = startPoint + new Vector3(0, 0, i * spacing);
            GameObject plane = Instantiate(planePrefab, spawnPos, Quaternion.identity, trackParent.transform);
            TileAbility ability = plane.GetComponent<TileAbility>();

            if (ability != null)
            {
                // Prevent abilities on first 3 tiles
                if (i <= 2)
                    ability.moveOffset = 0;
                else
                    ability.moveOffset = trackData[i].moveOffset;

                // Handle finish tile
                if (trackData[i].isFinish)
                {
                    ability.finish = plane.transform.Find("Finish")?.gameObject;
                    if (ability.finish != null) ability.finish.SetActive(true);
                }
                else if (ability.finish != null)
                {
                    ability.finish.SetActive(false);
                }
            }

            tileList.Add(plane.transform);
        }

        return tileList;
    }


    public void BackToLobby()
    {
        StartCoroutine(Lobby());
    }

    IEnumerator Lobby()
    {
        yield return new WaitForSeconds(5);
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        PhotonNetwork.LoadLevel("Lobby");
    }
}
