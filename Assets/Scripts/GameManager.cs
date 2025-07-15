using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject planePrefab;
    public int planeCount = 50;
    public float spacing = 2f;

    [Header("Starting points for each track")]
    public Transform track1StartPoint;
    public Transform track2StartPoint;
    public DiceRoller playerDice;
    public DiceRoller opponentDice; // could be another player or AI
    public float turnDelay = 1.5f;

    private bool isPlayerTurn = true;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        List<Transform> player1Tiles = SpawnTrack(track1StartPoint.position, "Player1Track");
        List<Transform> player2Tiles = SpawnTrack(track2StartPoint.position, "Player2Track");

        // Assign tiles to each DiceRoller
        UIManager.Instance.diceRoller[0].tilePoints = player1Tiles;
        UIManager.Instance.diceRoller[1].tilePoints = player2Tiles;


        playerDice.OnRollComplete += OnPlayerRolled;
        opponentDice.OnRollComplete += OnOpponentRolled;

        // Start with player
        StartPlayerTurn();
    }


    public List<Transform> SpawnTrack(Vector3 startPoint, string parentName)
    {
        GameObject trackParent = new GameObject(parentName);
        trackParent.transform.position = startPoint;
        trackParent.transform.SetParent(transform);

        List<Transform> tileList = new List<Transform>();

        for (int i = 0; i < planeCount; i++)
        {
            Vector3 spawnPos = startPoint + new Vector3(0, 0, i * spacing);
            GameObject plane = Instantiate(planePrefab, spawnPos, Quaternion.identity, trackParent.transform);
            TileAbility ability = plane.GetComponent<TileAbility>();

            if (ability != null)
            {
                // ✅ Ensure no ability is assigned to last tile
                if (i != planeCount - 1)
                {
                    float chance = Random.value;

                    if (chance < 0.1f && i > 10)
                    {
                        int offset = -Random.Range(1, 6);
                        if (i + offset >= 0)
                        {
                            ability.moveOffset = offset;
                        }
                    }
                    else if (chance > 0.9f && i < planeCount - 10)
                    {
                        int offset = Random.Range(1, 6);
                        if (i + offset < planeCount)
                        {
                            ability.moveOffset = offset;
                        }
                    }
                }

                // ✅ Assign finish object only on last tile
                if (i == planeCount - 1 && ability != null)
                {
                    if (ability.finish == null)
                    {
                        ability.finish = plane.transform.Find("Finish")?.gameObject;
                    }

                    if (ability.finish != null)
                        ability.finish.SetActive(true);
                    else
                        Debug.LogWarning("Finish object not found on last tile.");
                }
                else
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
        if (PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            // If still in a room, leave it first
            PhotonNetwork.LeaveRoom();
            Debug.Log("Leaving previous room before starting new match...");
        }
        PhotonNetwork.LoadLevel("Lobby");
    }



    void StartPlayerTurn()
    {
        isPlayerTurn = true;
        playerDice.SetInteractable(true);
        opponentDice.SetInteractable(false);
    }

    void StartOpponentTurn()
    {
        isPlayerTurn = false;
        playerDice.SetInteractable(false);
        opponentDice.SetInteractable(true);

        // Auto-roll if AI
        if (opponentDice.isAiControlled)
        {
            opponentDice.StartRoll();
        }
    }

    void OnPlayerRolled(int value)
    {
        playerDice.SetInteractable(false);
        Invoke(nameof(StartOpponentTurn), turnDelay);
    }

    void OnOpponentRolled(int value)
    {
        opponentDice.SetInteractable(false);
        Invoke(nameof(StartPlayerTurn), turnDelay);
    }
}
