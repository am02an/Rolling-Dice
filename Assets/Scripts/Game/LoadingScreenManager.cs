using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;
    [Header("UI Elements")]
    public Canvas loadingCanvas;
    public Slider loadingSlider;
    public TextMeshProUGUI percentageText;

    [Header("Settings")]
    public float loadingDuration = 2.5f; // between 2-3 seconds

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        //ShowLoadingScreen();
    }

    public void ShowLoadingScreen()
    {
        // Ensure canvas is enabled and on top
        loadingCanvas.enabled = true;
        loadingCanvas.sortingOrder = 999;

        StartCoroutine(PlayLoadingBar());
    }

    private IEnumerator PlayLoadingBar()
    {
        float elapsed = 0f;
        float value = 0f;

        while (elapsed < loadingDuration)
        {
            elapsed += Time.deltaTime;
            value = Mathf.Clamp01(elapsed / loadingDuration);
            loadingSlider.value = value;
            percentageText.text = Mathf.RoundToInt(value * 100f) + "%";
            yield return null;
        }

        loadingSlider.value = 1f;
        percentageText.text = "100%";

        yield return new WaitForSeconds(0.5f); // Optional delay before hiding

        loadingCanvas.enabled = false;
        // You can trigger your scene load or UI switch here
    }
}
