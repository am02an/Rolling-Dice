using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("AI Match Toggle")]
    public Toggle aiMatchToggle;

    void Start()
    {
        // Set toggle to match current PhotonManager setting
        if (PhotonManager.Instance != null)
            aiMatchToggle.isOn = PhotonManager.Instance.allowAIMatch;

        // Subscribe to toggle change
        aiMatchToggle.onValueChanged.AddListener(OnAIMatchToggleChanged);
    }

    private void OnAIMatchToggleChanged(bool isOn)
    {
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.allowAIMatch = isOn;
            Debug.Log("AI Matches Allowed: " + isOn);
        }
    }
}
