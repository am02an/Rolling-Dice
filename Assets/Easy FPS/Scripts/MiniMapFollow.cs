using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 50, 0);

    void LateUpdate()
    {
        // Follow player
        transform.position = player.position + offset;

        // Keep top-down view
       // transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
