using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UserControl))]
public class AIWaypointFollower : MonoBehaviour
{
    public WaypointCircuit waypointCircuit;
    public float followDistance = 1f;
    public float turnSpeed = 5f;
    public float moveSpeed = 5f;

    private UserControl userControl;
    private List<Vector3> pathPoints;
    private int currentTargetIndex = 0;

    private void Start()
    {
        userControl = GetComponent<UserControl>();
        pathPoints = waypointCircuit.GetCurvePoints();
    }

    private void Update()
    {
        if (pathPoints == null || pathPoints.Count == 0)
            return;

        Vector3 targetPoint = pathPoints[currentTargetIndex];
        Vector3 dirToTarget = (targetPoint - transform.position);
        float distance = dirToTarget.magnitude;

        // Move forward
        Vector3 moveDir = dirToTarget.normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // Smooth rotate toward path direction
        Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

        // If close enough to point, advance
        if (distance < followDistance)
        {
            currentTargetIndex = (currentTargetIndex + 1) % pathPoints.Count;
        }

        // Optional: Keep userControl for throttle/steering
        userControl.aiThrottle = 1f;
        userControl.aiSteering = 0f; // not needed since we rotate directly
        userControl.aiBrake = false;
    }
}
