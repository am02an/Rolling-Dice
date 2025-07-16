using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player1;              // Assign Player 1's transform
    public Transform player2;              // Assign Player 2's transform

    [Header("Offset Settings")]
    public Vector3 offset = new Vector3(2.53f, 3f, -5f);  // Offset from the target

    [Header("Follow Settings")]
    public float followSpeed = 2f;

    private bool shouldFollow = false;

    void LateUpdate()
    {
        if (!shouldFollow) return;

        Vector3 targetPosition;

        // Determine who to follow
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
            return; // No player to follow
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Call this when it's time to follow the player(s).
    /// </summary>
    public void MoveCameraAfterTurn()
    {
        shouldFollow = true;
    }

    /// <summary>
    /// Stop camera from following.
    /// </summary>
    public void StopCameraFollow()
    {
        shouldFollow = false;
    }
}
