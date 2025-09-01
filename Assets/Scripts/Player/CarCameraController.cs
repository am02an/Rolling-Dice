using UnityEngine;

public class CarCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);

    [Header("Smooth Settings")]
    public float followSpeed = 5f;
    public float rotationSpeed = 5f;

    private void LateUpdate()
    {
        if (!target) return;

        // Desired position relative to car
        Vector3 desiredPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Match the car's forward rotation smoothly
        Quaternion desiredRotation = Quaternion.LookRotation(target.forward, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }
}
