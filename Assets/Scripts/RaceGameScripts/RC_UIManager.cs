using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class RC_UIManager : MonoBehaviour
{
    public static RC_UIManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI driftText;
    public TextMeshProUGUI totalDriftText;
    public TextMeshProUGUI driftPopupText;

    private float totalDriftPoints = 0f;

    private void Awake()
    {
        Instance = this;
        driftText.text = "";
        driftText.transform.localScale = Vector3.zero;

        totalDriftText.text = "Total Drift: 0";
        totalDriftText.transform.localScale = Vector3.one;

        driftPopupText.gameObject.SetActive(false);
    }

    public void ShowDrift(float currentDrift)
    {
        driftText.text = $"+{Mathf.FloorToInt(currentDrift)}";

        // Change color based on score
        if (currentDrift >= 2500)
            driftText.color = Color.red;
        else if (currentDrift >= 1000)
            driftText.color = Color.yellow;
        else
            driftText.color = Color.white;

        // Animate drift text
        driftText.transform.DOKill();
        driftText.transform.localScale = Vector3.one * 1.4f;
        driftText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }
    public void ShowTotalDrift(float total)
    {
        totalDriftPoints = Mathf.FloorToInt(total);
        totalDriftText.text = $"Total Drift: {totalDriftPoints}";
    }

    public void HideDrift()
    {
        driftText.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
    }

    public void EndDrift(float score)
    {
        // Add to total only once per drift end
        totalDriftPoints += score;

        Debug.Log($"[Drift Ended] Score: {score}, Total: {totalDriftPoints}");

        totalDriftText.text = $"Total Drift: {Mathf.FloorToInt(totalDriftPoints)}";

        HideDrift();

        // Optional effects
        if (score >= 3000)
            ShowPerfectDrift();
    }

    public void ShowPerfectDrift()
    {
        StartCoroutine(ShowPopup("Perfect Drift!", Color.red));
    }

    public void ShowDriftChain(int chainCount)
    {
        StartCoroutine(ShowPopup($"Drift Chain x{chainCount}!", Color.yellow));
    }

    private IEnumerator ShowPopup(string message, Color color)
    {
        driftPopupText.text = message;
        driftPopupText.color = color;
        driftPopupText.gameObject.SetActive(true);
        driftPopupText.transform.localScale = Vector3.zero;

        driftPopupText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(1.2f);

        driftPopupText.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.3f);
        driftPopupText.gameObject.SetActive(false);
    }
}
