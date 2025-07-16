using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RPCManager : MonoBehaviourPun
{
    public static RPCManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Send Track Data to Other Player
    public void SendTrackData(List<TileData> p1, List<TileData> p2)
    {
        string jsonP1 = JsonUtility.ToJson(new TrackWrapper(p1));
        string jsonP2 = JsonUtility.ToJson(new TrackWrapper(p2));
        photonView.RPC("ReceiveTrackData", RpcTarget.Others, jsonP1, jsonP2);
    }
  


    [PunRPC]
    void ReceiveTrackData(string jsonP1, string jsonP2)
    {
        List<TileData> p1 = JsonUtility.FromJson<TrackWrapper>(jsonP1).tiles;
        List<TileData> p2 = JsonUtility.FromJson<TrackWrapper>(jsonP2).tiles;
        GameManager.Instance.OnTrackDataReceived(p1, p2);
    }

    // Turn Management
    public void SetTurnRPC(int actorNumber)
    {
        photonView.RPC("RPC_SetTurn", RpcTarget.AllBuffered, actorNumber);
    }
    [PunRPC]
    public void RPC_StartRoll(int diceID, int value)
    {
        DiceRoller targetDice = FindTargetDice(diceID);
        if (targetDice != null)
            targetDice.StartCoroutine(targetDice.RollRoutine(value));
    }

    public void SendDiceRoll(int diceID, int value)
    {
        photonView.RPC(nameof(RPC_StartRoll), RpcTarget.All, diceID, value);
    }

    private DiceRoller FindTargetDice(int id)
    {
        foreach (var dice in FindObjectsOfType<DiceRoller>())
        {
            if (dice.DiceID == id)
                return dice;
        }
        return null;
    }
    [PunRPC]
    public void RPC_ShowDiceResult(int diceID, int value)
    {
        DiceRoller targetDice = FindTargetDice(diceID);
        if (targetDice != null)
        {
            targetDice.SetFinalSprite(value);
        }
    }
    public void SendDiceSpriteSync(int diceID, int value)
    {
        photonView.RPC(nameof(RPC_ShowDiceResult), RpcTarget.Others, diceID, value);
    }

    [PunRPC]
    void RPC_SetTurn(int actorNumber)
    {
        Debug.Log($"Turn Changed To Actor: {actorNumber}");
        GameManager.Instance.OnTurnChanged(actorNumber);
    }

    // Optional: sync dice roll sprite (new addition)
    public void SyncDiceFace(int diceId, int faceIndex)
    {
        photonView.RPC("RPC_SyncDiceFace", RpcTarget.Others, diceId, faceIndex);
    }

    [PunRPC]
    void RPC_SyncDiceFace(int diceId, int faceIndex)
    {
        UIManager.Instance.UpdateDiceFace(diceId, faceIndex);
    }
    // Inside RPCManager.cs
    [PunRPC]
    public void RPC_StartCountdown()
    {
        GameManager.Instance.StartCoroutine(UIManager.Instance.StartCountdown());
    }

    public void SendStartCountdown()
    {
        photonView.RPC(nameof(RPC_StartCountdown), RpcTarget.All);
    }
    [PunRPC]
    public void RPC_ShowGameResult(int winnerDiceID)
    {
        if (winnerDiceID == 1)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
                UIManager.Instance.ShowVictory();
            else
                UIManager.Instance.ShowDefeat();
        }
        else if (winnerDiceID == 2)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
                UIManager.Instance.ShowDefeat();
            else
                UIManager.Instance.ShowVictory();
        }
    }

}
