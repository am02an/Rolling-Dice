using UnityEngine;

/// <summary>
/// Controls the camera behavior to follow one or two player targets smoothly.
/// Can be toggled on or off during gameplay (e.g., during turns).
/// </summary>
public class Dice_CameraController : MonoBehaviour
{
    #region Target Settings
    [Header("Target Settings")]
    public Transform player1; // Assign Player 1's transform
    public Transform player2; // Assign Player 2's transform
    #endregion

    #region Camera Settings
    [Header("Offset Settings")]
    public Vector3 offset = new Vector3(2.53f, 3f, -5f);

    [Header("Follow Settings")]
    public float followSpeed = 2f;
    private bool shouldFollow = false;
    #endregion

    #region Unity Callbacks
    private void LateUpdate()
    {
        if (!shouldFollow) return;

        Vector3 targetPosition;

        if (player1 != null && player2 != null)
        {
            Vector3 midpoint = (player1.position + player2.position) / 2f;
            targetPosition = midpoint + offset;
        }
        else if (player1 != null)
        {
            targetPosition = player1.position + offset;
        }
        else if (player2 != null)
        {
            targetPosition = player2.position + offset;
        }
        else
        {
            return;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Enables camera following behavior.
    /// </summary>
    public void MoveCameraAfterTurn()
    {
        shouldFollow = true;
    }

    /// <summary>
    /// Disables camera following behavior.
    /// </summary>
    public void StopCameraFollow()
    {
        shouldFollow = false;
    }
    #endregion
}
