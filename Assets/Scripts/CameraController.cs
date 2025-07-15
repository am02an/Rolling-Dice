using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player1;              // Assign Player 1's transform
    public Vector3 offset = new Vector3(0f, 10f, -10f);  // Adjust as needed

    [Header("Follow Settings")]
    public float followSpeed = 2f;

    private bool shouldFollow = false;

    void LateUpdate()
    {
        if (!shouldFollow || player1 == null) return;

        // Smoothly move the camera toward the player1 position + offset
        Vector3 targetPosition = player1.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Call this when both players finish their turns.
    /// </summary>
    public void MoveCameraAfterTurn()
    {
        shouldFollow = true;
    }

    /// <summary>
    /// Optional: Stop following if needed later.
    /// </summary>
    public void StopCameraFollow()
    {
        shouldFollow = false;
    }
}
