using UnityEngine;
using UnityEngine.UI;

public class MapNavigator : MonoBehaviour
{
    public RectTransform playerIcon;   // UI arrow icon
    public Transform player;           // Player in world
    public float mapWidth = 100f;      // World size (X or Z axis)
    public RectTransform mapBar;       // UI map bar

    void Update()
    {
        // --- POSITION ---
        float normalizedX = Mathf.InverseLerp(-mapWidth, mapWidth, player.position.x);

        float barWidth = mapBar.rect.width;
        float newX = normalizedX * barWidth;

        playerIcon.anchoredPosition = new Vector2(newX, playerIcon.anchoredPosition.y);

        // --- ROTATION ---
        // Get the player's forward direction (XZ plane only)
        Vector3 forward = player.forward;
        forward.y = 0; // Ignore vertical tilt

        // Calculate angle in degrees
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

        // Apply rotation to UI arrow
        playerIcon.localRotation = Quaternion.Euler(0, 0, -angle);
    }
    public RawImage compassImage; // Your navigation bar image

    [Header("Compass Settings")]
    public float scrollSpeed = 0.002f; // Adjust to fit the image size

   
    public void handleDirection()
    {
        if (player == null || compassImage == null) return;

        // Get player’s Y rotation (heading)
        float playerRotation = player.eulerAngles.y;

        // Convert rotation into UV offset
        float offsetX = playerRotation * scrollSpeed;

        // Scroll only X axis of the RawImage
        compassImage.uvRect = new Rect(offsetX, 0, 1, 1);

    }
}
