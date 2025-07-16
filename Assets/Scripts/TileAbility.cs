using UnityEngine;
using TMPro;
using DG.Tweening;

public class TileAbility : MonoBehaviour
{
    public int moveOffset = 0;

    // UI Elements
    public TextMeshProUGUI offsetText;
    public GameObject greenAbility;
    public GameObject redAbility;
    public GameObject finish;

    private void Start()
    {
        offsetText?.gameObject.SetActive(false);
        greenAbility?.SetActive(false);
        redAbility?.SetActive(false);

        if (moveOffset == 0) return;

        if (offsetText != null)
        {
            offsetText.gameObject.SetActive(true);
            offsetText.text = moveOffset > 0 ? $"+{moveOffset}" : $"{moveOffset}";

            // ✅ Continuous smooth Y-axis rotation
            offsetText.transform.localRotation = Quaternion.identity;
            offsetText.transform.DOLocalRotate(new Vector3(360, 0, 0), 2f, RotateMode.FastBeyond360)
                               .SetEase(Ease.Linear)
                               .SetLoops(-1, LoopType.Restart);
        }

        if (moveOffset > 0)
        {
            greenAbility?.SetActive(true);
        }
        else
        {
            redAbility?.SetActive(true);
        }
    }
}
