using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WaypointCircuit : MonoBehaviour
{
    [Range(0.01f, 1f)] public float resolution = 0.05f;
    public Color lineColor = Color.cyan;

    public List<Transform> waypoints = new List<Transform>();
    private List<Vector3> curvePoints = new List<Vector3>();

    public int WaypointCount => curvePoints.Count;

    private void OnDrawGizmos()
    {
        RefreshWaypoints();
        GenerateCurvePoints();

        Gizmos.color = lineColor;
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            Gizmos.DrawLine(curvePoints[i], curvePoints[i + 1]);
        }

        foreach (var wp in waypoints)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(wp.position, 0.2f);
        }
    }

    public Vector3 GetCurvePoint(int index)
    {
        if (curvePoints.Count == 0) return Vector3.zero;
        return curvePoints[Mathf.Clamp(index, 0, curvePoints.Count - 1)];
    }

    public List<Vector3> GetCurvePoints() => curvePoints;

    private void RefreshWaypoints()
    {
        waypoints.Clear();
        foreach (Transform child in transform)
        {
            waypoints.Add(child);
        }
    }

    private void GenerateCurvePoints()
    {
        curvePoints.Clear();
        if (waypoints.Count < 4) return;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 p0 = waypoints[(i - 1 + waypoints.Count) % waypoints.Count].position;
            Vector3 p1 = waypoints[i % waypoints.Count].position;
            Vector3 p2 = waypoints[(i + 1) % waypoints.Count].position;
            Vector3 p3 = waypoints[(i + 2) % waypoints.Count].position;

            for (float t = 0; t < 1; t += resolution)
            {
                Vector3 point = CatmullRom(t, p0, p1, p2, p3);
                curvePoints.Add(point);
            }
        }
    }

    private Vector3 CatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}
