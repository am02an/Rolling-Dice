using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MapNavigator : MonoBehaviour
{
    public RectTransform playerIcon;   // UI arrow icon
    public Transform player;           // Player in world
    public float mapWidth = 100f;      // World size (X or Z axis)
    public RectTransform mapBar;       // UI map bar
    public TextMeshProUGUI compassText;
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
        HandleDirection();
    }
    public RawImage compassImage; // Your navigation bar image

    [Header("Compass Settings")]
    public float scrollSpeed = 0.002f; // Adjust to fit the image size


    private string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

    public void HandleDirection()
    {
        if (player == null || compassText == null) return;

        // Get player's Y rotation (heading)
        float playerRotation = player.eulerAngles.y;

        // Divide 360 degrees into 8 slices (45° each)
        int index = Mathf.RoundToInt(playerRotation / 45f) % 8;

        // Update text
        compassText.text = directions[index];
    }
}
