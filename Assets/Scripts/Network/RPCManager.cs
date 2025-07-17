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

    #region Track Sync

    /// <summary>
    /// Sends serialized track data of both players to the opponent.
    /// </summary>
    public void SendTrackData(List<TileData> p1, List<TileData> p2)
    {
        string jsonP1 = JsonUtility.ToJson(new TrackWrapper(p1));
        string jsonP2 = JsonUtility.ToJson(new TrackWrapper(p2));
        photonView.RPC("ReceiveTrackData", RpcTarget.Others, jsonP1, jsonP2);
    }

    /// <summary>
    /// Receives track data and calls GameManager to handle it.
    /// </summary>
    [PunRPC]
    void ReceiveTrackData(string jsonP1, string jsonP2)
    {
        List<TileData> p1 = JsonUtility.FromJson<TrackWrapper>(jsonP1).tiles;
        List<TileData> p2 = JsonUtility.FromJson<TrackWrapper>(jsonP2).tiles;
        GameManager.Instance.OnTrackDataReceived(p1, p2);
    }

    #endregion

    #region Turn Management

    /// <summary>
    /// Sends current turn actor number to all players.
    /// </summary>
    public void SetTurnRPC(int actorNumber)
    {
        photonView.RPC("RPC_SetTurn", RpcTarget.AllBuffered, actorNumber);
    }

    /// <summary>
    /// Receives turn change and notifies GameManager.
    /// </summary>
    [PunRPC]
    void RPC_SetTurn(int actorNumber)
    {
        Debug.Log($"Turn Changed To Actor: {actorNumber}");
        GameManager.Instance.OnTurnChanged(actorNumber);
    }

    #endregion

    #region Dice Rolling

    /// <summary>
    /// Sends dice roll value to all clients to sync dice roll.
    /// </summary>
    public void SendDiceRoll(int diceID, int value)
    {
        photonView.RPC(nameof(RPC_StartRoll), RpcTarget.All, diceID, value);
    }

    /// <summary>
    /// Starts dice roll animation for the specified dice.
    /// </summary>
    [PunRPC]
    public void RPC_StartRoll(int diceID, int value)
    {
        DiceRoller targetDice = FindTargetDice(diceID);
        if (targetDice != null)
            targetDice.StartCoroutine(targetDice.RollRoutine(value));
    }

    /// <summary>
    /// Syncs final dice face after roll is complete.
    /// </summary>
    [PunRPC]
    public void RPC_ShowDiceResult(int diceID, int value)
    {
        DiceRoller targetDice = FindTargetDice(diceID);
        if (targetDice != null)
        {
            targetDice.SetFinalSprite(value);
        }
    }

    /// <summary>
    /// Syncs dice face during roll to show rolling effect.
    /// </summary>
    public void SendDiceSpriteSync(int diceID, int faceValue, bool isRolling = false)
    {
        photonView.RPC("RPC_SyncDiceSprite", RpcTarget.Others, diceID, faceValue, isRolling);
    }

    [PunRPC]
    public void RPC_SyncDiceSprite(int diceID, int faceValue, bool isRolling)
    {
        DiceRoller dice = UIManager.Instance.GetDiceByID(diceID);
        if (dice == null) return;

        dice.SetDiceSprite(faceValue - 1);

        if (isRolling)
            dice.PlayPunchAnimationDuringRoll();
        else
            dice.PlayFinalPunchEffect();
    }

    /// <summary>
    /// Finds a dice by its ID from the scene.
    /// </summary>
    private DiceRoller FindTargetDice(int id)
    {
        foreach (var dice in FindObjectsOfType<DiceRoller>())
        {
            if (dice.DiceID == id)
                return dice;
        }
        return null;
    }

    /// <summary>
    /// Syncs dice face index between clients.
    /// </summary>
    public void SyncDiceFace(int diceId, int faceIndex)
    {
        photonView.RPC("RPC_SyncDiceFace", RpcTarget.Others, diceId, faceIndex);
    }

    #endregion

    #region Countdown

    /// <summary>
    /// Sends start countdown request to all players.
    /// </summary>
    public void SendStartCountdown()
    {
        photonView.RPC(nameof(RPC_StartCountdown), RpcTarget.All);
    }

    /// <summary>
    /// Starts countdown animation on all clients.
    /// </summary>
    [PunRPC]
    public void RPC_StartCountdown()
    {
        GameManager.Instance.StartCoroutine(UIManager.Instance.StartCountdown());
    }

    #endregion

    #region Game Result

    /// <summary>
    /// Show game result based on winning dice ID.
    /// </summary>
    public void ShowGameResult(int winnerDiceID)
    {
        photonView.RPC(nameof(RPC_ShowGameResult), RpcTarget.All, winnerDiceID);
    }

    [PunRPC]
    public void RPC_ShowGameResult(int winnerDiceID)
    {
        bool isWinner = (winnerDiceID == 1 && PhotonNetwork.LocalPlayer.IsMasterClient)
                     || (winnerDiceID == 2 && !PhotonNetwork.LocalPlayer.IsMasterClient);

        if (isWinner)
            UIManager.Instance.ShowVictory();
        else
            UIManager.Instance.ShowDefeat();
    }

    #endregion

    #region Progress Sync

    /// <summary>
    /// Sends player progress update to opponent.
    /// </summary>
    public void UpdateSlider(int diceID, int currentTileIndex, int totalTiles)
    {
        photonView.RPC("RPC_UpdatePlayerProgress", RpcTarget.Others, diceID, currentTileIndex, totalTiles);
    }

    /// <summary>
    /// Updates UI slider for opponent's progress.
    /// </summary>
    [PunRPC]
    public void RPC_UpdatePlayerProgress(int diceID, int currentTileIndex, int totalTiles)
    {
        UIManager.Instance.UpdatePlayerProgress(diceID, currentTileIndex, totalTiles);
    }

    #endregion
}
