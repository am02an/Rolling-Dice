using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Controls the visual and gameplay behavior of a tile's special ability,
/// including offset movement and associated UI effects.
/// </summary>
public class TileAbility : MonoBehaviour
{
    #region Config
    public int moveOffset = 0;
    #endregion

    #region UI References
    public TextMeshProUGUI offsetText;
    public GameObject greenAbility;
    public GameObject redAbility;
    public GameObject finish;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        offsetText?.gameObject.SetActive(false);
        greenAbility?.SetActive(false);
        redAbility?.SetActive(false);

        if (moveOffset == 0) return;

        SetupOffsetText();
        ToggleAbilityColor();
    }
    #endregion

    #region Helper Methods
    private void SetupOffsetText()
    {
        if (offsetText == null) return;

        offsetText.gameObject.SetActive(true);
        offsetText.text = moveOffset > 0 ? $"+{moveOffset}" : $"{moveOffset}";

        offsetText.transform.localRotation = Quaternion.identity;
        offsetText.transform
            .DOLocalRotate(new Vector3(360, 0, 0), 2f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void ToggleAbilityColor()
    {
        if (moveOffset > 0)
        {
            greenAbility?.SetActive(true);
        }
        else
        {
            redAbility?.SetActive(true);
        }
    }
    #endregion
}
