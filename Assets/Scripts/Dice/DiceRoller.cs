using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using Photon.Pun;

/// <summary>
/// Handles dice roll logic for both player and AI, including animations, UI updates,
/// player movement, tile effects, and multiplayer synchronization.
/// </summary>
public class DiceRoller : MonoBehaviour
{
    #region Fields & Configurations

    public int DiceID;

    [Header("Dice Visuals")]
    public Sprite[] diceFaces;
    public Image diceImage;
    public Button rollButton;

    [Header("Highlight UI")]
    public GameObject highlightObject;
    public Transform arrowObject;

    [Header("Settings")]
    public float rollDuration = 1.2f;
    public bool isAiControlled = false;

    public System.Action<int> OnRollComplete;
    private PhotonView pv;
    public int actorNumber;
    public bool isMine;

    private bool isRolling = false;
    private Tween arrowTween;

    [Header("Player Movement")]
    public Transform playerObject;
    public float tileDistance = 1f;
    public List<Transform> tilePoints;
    private int currentTileIndex = 0;
    private static int turnCompletedCount = 0;
    private Dice_CameraController cameraController;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        isAiControlled = (DiceID == 2 && PhotonManager.Instance.isAIMatch);
        highlightObject?.SetActive(false);
        arrowObject?.gameObject.SetActive(false);
        cameraController = Camera.main.GetComponent<Dice_CameraController>();
        pv = GetComponent<PhotonView>();
    }

    #endregion

    #region UI Interaction

    public void SetInteractable(bool interactable)
    {
        rollButton.interactable = interactable && !isAiControlled;
        diceImage.color = interactable ? Color.white : new Color(1, 1, 1, 0.4f);
        highlightObject?.SetActive(interactable);
        arrowObject?.gameObject.SetActive(interactable);

        if (interactable) StartArrowTween();
        else StopArrowTween();
    }

    #endregion

    #region Dice Rolling Logic

    public void StartRoll()
    {
        if (isRolling) return;

        if (isAiControlled) StartCoroutine(AIRollRoutine());
        else RollDiceButton();
    }

    public void RollDiceButton()
    {
        if (isRolling) return;

        int value = Random.Range(1, 7);

        if (!PhotonManager.Instance.isAIMatch && PhotonNetwork.InRoom)
            RPCManager.Instance.RPC_StartRoll(DiceID, value);
        else
            StartCoroutine(RollRoutine(value));
    }

    private IEnumerator AIRollRoutine()
    {
        yield return new WaitForSeconds(Random.Range(1f, 1.8f));
        int aiValue = Random.Range(1, 7);

        if (PhotonNetwork.InRoom)
            RPCManager.Instance.SendDiceRoll(DiceID, aiValue);
        else
            yield return RollRoutine(aiValue);
    }

    public IEnumerator RollRoutine(int value)
    {
        isRolling = true;
        float timer = 0f;

        // Reset scale before animation
        diceImage.transform.DOKill(); // Kill any running tweens
        diceImage.transform.localScale = Vector3.one;

        while (timer < rollDuration)
        {
            int randomIndex = Random.Range(0, diceFaces.Length);
            diceImage.sprite = diceFaces[randomIndex];

            // Scale animation for dice shake
            diceImage.transform.DOKill(); // Prevent overlapping animations
            diceImage.transform.localScale = Vector3.one; // Reset before punch
            diceImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 10, 1f);

            // Sync visual dice rolling if needed
            if (PhotonNetwork.InRoom && !PhotonManager.Instance.isAIMatch && PhotonNetwork.LocalPlayer.ActorNumber == GameManager.Instance.currentTurnActorNumber)
                RPCManager.Instance.SendDiceSpriteSync(DiceID, randomIndex + 1, true);

            timer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // Final result
        diceImage.transform.DOKill(); // Kill before setting result
        diceImage.transform.localScale = Vector3.one;
        diceImage.sprite = diceFaces[value - 1];
        RPCManager.Instance.SendDiceSpriteSync(DiceID, value, false);

        // Final bounce effect
        diceImage.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 8, 0.7f);
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MovePlayerRoutine(value));

        isRolling = false;
        OnRollComplete?.Invoke(value);
        turnCompletedCount++;

        if (turnCompletedCount >= 2 && cameraController != null)
        {
            cameraController.MoveCameraAfterTurn();
            turnCompletedCount = 0;
        }
    }


    #endregion

    #region Player Movement Logic

    private IEnumerator MovePlayerRoutine(int diceValue)
    {
        if (playerObject == null || tilePoints == null || tilePoints.Count == 0) yield break;

        int maxIndex = tilePoints.Count - 1;
        int attemptedTargetIndex = currentTileIndex + diceValue;

        if (attemptedTargetIndex > maxIndex)
        {
            UIManager.Instance.UpdatePlayerProgress(DiceID, currentTileIndex, tilePoints.Count);
            RPCManager.Instance.UpdateSlider(DiceID, currentTileIndex, tilePoints.Count);
            yield break;
        }

        int targetIndex = attemptedTargetIndex;
        Sequence moveSequence = DOTween.Sequence();

        for (int i = currentTileIndex + 1; i <= targetIndex; i++)
        {
            moveSequence.Append(playerObject.DOMove(tilePoints[i].position + new Vector3(0, 0.5f, 0), 0.3f).SetEase(Ease.Linear));
        }

        currentTileIndex = targetIndex;

        bool movementComplete = false;
        moveSequence.OnComplete(() => movementComplete = true);
        yield return new WaitUntil(() => movementComplete);

        if (currentTileIndex == maxIndex)
        {
            TileAbility ability = tilePoints[currentTileIndex].GetComponent<TileAbility>();
            if (ability != null && ability.finish != null)
                ability.finish.SetActive(true);

            UIManager.Instance.UpdatePlayerProgress(DiceID, currentTileIndex, tilePoints.Count);
            RPCManager.Instance.UpdateSlider(DiceID, currentTileIndex, tilePoints.Count);

            if (PhotonNetwork.InRoom)
                RPCManager.Instance.photonView.RPC("RPC_ShowGameResult", RpcTarget.All, DiceID);

            yield break;
        }

        yield return StartCoroutine(HandleAbilityMovement(DiceID,  tilePoints.Count));
    }

    private IEnumerator HandleAbilityMovement(int diceID, int totalTiles)
    {
        while (true)
        {
            TileAbility ability = tilePoints[currentTileIndex].GetComponent<TileAbility>();
            if (ability == null || ability.moveOffset == 0)
                break;

            yield return new WaitForSeconds(1f);

            int newIndex = Mathf.Clamp(currentTileIndex + ability.moveOffset, 0, tilePoints.Count - 1);
            if (newIndex == currentTileIndex) break;

            int stepDirection = newIndex > currentTileIndex ? 1 : -1;

            Sequence abilitySequence = DOTween.Sequence();
            for (int i = currentTileIndex + stepDirection; i != newIndex + stepDirection; i += stepDirection)
            {
                abilitySequence.Append(playerObject.DOMove(tilePoints[i].position + new Vector3(0, 0.5f, 0), 0.3f).SetEase(Ease.Linear));
            }

            bool abilityMoveDone = false;
            abilitySequence.OnComplete(() => abilityMoveDone = true);
            yield return new WaitUntil(() => abilityMoveDone);

            // ✅ Now we are updating the actual field, not a local copy
            currentTileIndex = newIndex;

            UIManager.Instance.UpdatePlayerProgress(diceID, currentTileIndex, tilePoints.Count);
            RPCManager.Instance.UpdateSlider(diceID, currentTileIndex, totalTiles);
        }

        // Optional final update (in case of no ability)
        UIManager.Instance.UpdatePlayerProgress(diceID, currentTileIndex, tilePoints.Count);
        RPCManager.Instance.UpdateSlider(diceID, currentTileIndex, totalTiles);
    }

    #endregion

    #region Dice Visual Sync (Remote)
    public void SetDiceSprite(int index)
    {
        diceImage.sprite = diceFaces[index];
    }
    public void PlayPunchAnimationDuringRoll()
    {
        diceImage.transform.DOKill();
        diceImage.transform.localScale = Vector3.one;
        diceImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 10, 1f);
    }
    public void PlayFinalPunchEffect()
    {
        diceImage.transform.DOKill();
        diceImage.transform.localScale = Vector3.one;
        diceImage.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 8, 0.7f);
    }
    public void SetFinalSprite(int value)
    {
        diceImage.sprite = diceFaces[value - 1];
    }
    #endregion

    #region Arrow Highlighting

    private void StartArrowTween()
    {
        if (arrowObject == null) return;

        arrowTween?.Kill();

        float arrowOffset = 590f;
        float moveDistance = 15f;

        Vector3 startPos = DiceID == 1 ? new Vector3(arrowOffset, 0, 0) : new Vector3(-arrowOffset, 0, 0);
        float targetX = DiceID == 1 ? startPos.x + moveDistance : startPos.x - moveDistance;

        arrowObject.localScale = Vector3.one;
        arrowObject.localRotation = Quaternion.Euler(0f, DiceID == 1 ? 0f : 180f, 0f);
        arrowObject.localPosition = startPos;

        arrowTween = arrowObject.DOLocalMoveX(targetX, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    private void StopArrowTween()
    {
        arrowTween?.Kill();
        arrowTween = null;

        if (arrowObject != null)
            arrowObject.localPosition = Vector3.zero;
    }

    #endregion
}
