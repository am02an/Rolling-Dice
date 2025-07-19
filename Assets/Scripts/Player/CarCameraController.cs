using UnityEngine;

public class CarCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);

    [Header("Smooth Settings")]
    public float followSpeed = 10f;
    public float rotationSpeed = 5f;

    private void LateUpdate()
    {
        if (!target) return;

        // Smooth Position
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Smooth Rotation (look at target)
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
