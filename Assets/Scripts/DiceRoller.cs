using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class DiceRoller : MonoBehaviour
{
    public int DiceID;

    [Header("Dice Visuals")]
    public Sprite[] diceFaces;      // 6 faces, index 0 = face 1
    public Image diceImage;
    public Button rollButton;
    public TMP_Text resultText;

    [Header("Highlight UI")]
    public GameObject highlightObject; // Outline or glow
    public Transform arrowObject;      // Left-right animated arrow

    [Header("Settings")]
    public float rollDuration = 1.2f;
    public bool isAiControlled = false;

    public System.Action<int> OnRollComplete;

    private bool isRolling = false;
    private Tween arrowTween;
    [Header("PlayerMovement")]
    public Transform playerObject; // Assign your player prefab instance
    public float tileDistance = 1f; // Distance per dice number
    public List<Transform> tilePoints;
    private int currentTileIndex = 0;
    private static int turnCompletedCount = 0;
    private CameraController cameraController;
    private void Start()
    {
        // Existing setup...
        isAiControlled = (DiceID == 2 && PhotonManager.Instance.isAIMatch);
        highlightObject?.SetActive(false);
        arrowObject?.gameObject.SetActive(false);

        cameraController = Camera.main.GetComponent<CameraController>();
    }


    /// <summary>
    /// Call this when it's this dice's turn.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        rollButton.interactable = interactable && !isAiControlled;
        diceImage.color = interactable ? Color.white : new Color(1, 1, 1, 0.4f);

        // Highlight and arrow visuals
        highlightObject?.SetActive(interactable);
        arrowObject?.gameObject.SetActive(interactable);

        if (interactable)
            StartArrowTween();
        else
            StopArrowTween();
    }

    /// <summary>
    /// Call to start the dice roll logic.
    /// AI will auto-roll, player will wait for button.
    /// </summary>
    public void StartRoll()
    {
        if (isRolling) return;

        if (isAiControlled)
        {
            StartCoroutine(AIRollRoutine());
        }
        else
        {
            RollDiceButton(); // Manual roll
        }
    }

    /// <summary>
    /// Called by UI button click.
    /// </summary>
    public void RollDiceButton()
    {
        if (isRolling) return;

        int value = Random.Range(1, 7);
        StartCoroutine(RollRoutine(value));
    }

    private IEnumerator AIRollRoutine()
    {
        yield return new WaitForSeconds(Random.Range(1f, 1.8f));
        int aiValue = Random.Range(1, 7);
        yield return RollRoutine(aiValue);
    }

    private IEnumerator RollRoutine(int value)
    {
        isRolling = true;
        float timer = 0f;
        diceImage.transform.localScale = Vector3.one;

        while (timer < rollDuration)
        {
            int randomIndex = Random.Range(0, diceFaces.Length);
            diceImage.sprite = diceFaces[randomIndex];
            diceImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 10, 1f);

            timer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        diceImage.sprite = diceFaces[value - 1];
        diceImage.transform.DOKill();
        diceImage.transform.localScale = Vector3.one;
        diceImage.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 8, 0.7f);
        yield return new WaitForSeconds(1f);

        if (resultText != null)
        {
            resultText.text = $"Result: {value}";
        }

        Debug.Log($"Dice {DiceID} rolled {value}");

        // ✅ Wait for full player movement including tile abilities
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

    private IEnumerator MovePlayerRoutine(int diceValue)
    {
        if (playerObject == null || tilePoints == null || tilePoints.Count == 0) yield break;

        int maxIndex = tilePoints.Count - 1;
        int attemptedTargetIndex = currentTileIndex + diceValue;

        // 👇 Exact roll required to win
        if (attemptedTargetIndex > maxIndex)
        {
            Debug.Log($"Dice roll {diceValue} is too high to finish. Must roll exactly {maxIndex - currentTileIndex}.");
            UIManager.Instance.UpdatePlayerProgress(DiceID, currentTileIndex, tilePoints.Count);
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

        // ✅ Final Tile Reached Logic
        if (currentTileIndex == maxIndex)
        {
            Debug.Log("Player reached the final tile!");

            // Show Finish UI
            TileAbility ability = tilePoints[currentTileIndex].GetComponent<TileAbility>();
            if (ability != null && ability.finish != null)
                ability.finish.SetActive(true);

            UIManager.Instance.UpdatePlayerProgress(DiceID, currentTileIndex, tilePoints.Count);

            // Show Result UI
            if (DiceID == 1)
                UIManager.Instance.ShowVictory(); // Replace with your method
            else if (DiceID == 2)
                UIManager.Instance.ShowDefeat(); // Replace with your method

            yield break;
        }

        // 🔁 Ability Movement (if not on final tile)
        yield return StartCoroutine(HandleAbilityMovement());
    }


    private IEnumerator HandleAbilityMovement()
    {
        while (true)
        {
            TileAbility ability = tilePoints[currentTileIndex].GetComponent<TileAbility>();
            if (ability == null || ability.moveOffset == 0)
                break;

            int newIndex = Mathf.Clamp(currentTileIndex + ability.moveOffset, 0, tilePoints.Count - 1);

            if (newIndex == currentTileIndex) break;

            int stepDirection = newIndex > currentTileIndex ? 1 : -1;

            Sequence abilitySequence = DOTween.Sequence();

            for (int i = currentTileIndex + stepDirection; i != newIndex + stepDirection; i += stepDirection)
            {
                abilitySequence.Append(playerObject.DOMove(tilePoints[i].position + new Vector3(0, 0.5f, 0), 0.3f).SetEase(Ease.Linear));
            }

            currentTileIndex = newIndex;

            bool abilityMoveDone = false;
            abilitySequence.OnComplete(() => abilityMoveDone = true);

            yield return new WaitUntil(() => abilityMoveDone);
        }

        UIManager.Instance.UpdatePlayerProgress(DiceID, currentTileIndex, tilePoints.Count);
    }


    //private void MovePlayer(int diceValue)
    //{
    //    if (playerObject == null || tilePoints == null || tilePoints.Count == 0) return;

    //    Sequence fullSequence = DOTween.Sequence();

    //    int targetIndex = Mathf.Clamp(currentTileIndex + diceValue, 0, tilePoints.Count - 1);

    //    // Step 1: Move to the rolled tile
    //    for (int i = currentTileIndex + 1; i <= targetIndex; i++)
    //    {
    //        fullSequence.Append(playerObject.DOMove(tilePoints[i].position + new Vector3(0, 0.5f, 0), 0.3f).SetEase(Ease.Linear));
    //    }

    //    currentTileIndex = targetIndex;

    //    // Step 2: Recursively check and move based on tile abilities
    //    fullSequence.AppendCallback(() =>
    //    {
    //        TryHandleAbilityRecursive();
    //    });

    //    // Step 3: Update UI when all movement is complete
    //    fullSequence.OnComplete(() =>
    //    {
    //        UIManager.Instance.UpdatePlayerProgress(DiceID, currentTileIndex, tilePoints.Count);
    //    });
    //}
    //private void TryHandleAbilityRecursive()
    //{
    //    TileAbility ability = tilePoints[currentTileIndex].GetComponent<TileAbility>();

    //    if (ability != null && ability.moveOffset != 0)
    //    {
    //        int nextIndex = Mathf.Clamp(currentTileIndex + ability.moveOffset, 0, tilePoints.Count - 1);

    //        if (nextIndex != currentTileIndex)
    //        {
    //            Sequence recursiveMove = DOTween.Sequence();

    //            int stepDirection = nextIndex > currentTileIndex ? 1 : -1;

    //            for (int i = currentTileIndex + stepDirection; i != nextIndex + stepDirection; i += stepDirection)
    //            {
    //                recursiveMove.Append(playerObject.DOMove(tilePoints[i].position + new Vector3(0, 0.5f, 0), 0.3f).SetEase(Ease.Linear));
    //            }

    //            currentTileIndex = nextIndex;

    //            recursiveMove.OnComplete(() =>
    //            {
    //                TryHandleAbilityRecursive(); // Check again if landed on another ability tile
    //            });
    //        }
    //    }
    //}


    private void StartArrowTween()
    {
        if (arrowObject == null) return;

        arrowTween?.Kill();

        float arrowOffset = 590f;
        float moveDistance = 15f;

        Vector3 startPos;
        float targetX;

        if (DiceID == 1)
        {
            // Right side for player 1
            startPos = new Vector3(arrowOffset, 0, 0);
            targetX = startPos.x + moveDistance;

            arrowObject.localScale = Vector3.one;
            arrowObject.localRotation = Quaternion.Euler(0f, 0f, 0f); // Facing right
        }
        else
        {
            // Left side for player 2/AI
            startPos = new Vector3(-arrowOffset, 0, 0);
            targetX = startPos.x - moveDistance;

            arrowObject.localScale = Vector3.one; // Keep scale normal
            arrowObject.localRotation = Quaternion.Euler(0f, 180f, 0f); // Flip to face left
        }

        arrowObject.localPosition = startPos;

        arrowTween = arrowObject.DOLocalMoveX(targetX, 0.6f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }


    private void StopArrowTween()
    {
        arrowTween?.Kill();
        arrowTween = null;

        if (arrowObject != null)
            arrowObject.localPosition = Vector3.zero;
    }
}
