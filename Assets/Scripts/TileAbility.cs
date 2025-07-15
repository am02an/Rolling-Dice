using UnityEngine;
using TMPro;

public class TileAbility : MonoBehaviour
{
    public int moveOffset = 0; // Example: +3, -2

    // UI Elements
    public TextMeshProUGUI offsetText;
    public GameObject greenAbility; // For +ve offset (forward)
    public GameObject redAbility;   // For -ve offset (backward)
    public GameObject finish;   // For -ve offset (backward)

    private void Start()
    {
        // Hide all visuals by default
        if (offsetText != null)
            offsetText.gameObject.SetActive(false);

        greenAbility?.SetActive(false);
        redAbility?.SetActive(false);
       
        if (moveOffset == 0)
        {
            // No ability, nothing to show
            return;
        }

        // Set text and activate proper ability visual
        if (offsetText != null)
        {
            offsetText.gameObject.SetActive(true);
            offsetText.text = moveOffset > 0 ? $"+{moveOffset}" : $"{moveOffset}";
        }

        if (moveOffset > 0)
        {
            greenAbility?.SetActive(true);
        }
        else if (moveOffset < 0)
        {
            redAbility?.SetActive(true);
        }
    }
}
