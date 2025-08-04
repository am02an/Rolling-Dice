using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

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
    public void HideLoadingScreen()
    {
        loadingCanvas.enabled = false;
    }

    public void ShowLoadingScreen(bool isPhotonScene, string sceneName)
    {
        loadingCanvas.enabled = true;
        loadingCanvas.sortingOrder = 999;

        if (isPhotonScene)
        {
            StartCoroutine(PlayPhotonLoadingBar(sceneName));
        }
        else
        {
            StartCoroutine(PlayUnityLoadingBar(sceneName));
        }
    }
    private IEnumerator PlayPhotonLoadingBar(string sceneName)
    {
        float progress = 0f;

        PhotonNetwork.LoadLevel(sceneName); // Photon scene load
        while (progress < 1f)
        {
            progress += Time.deltaTime * 0.3f; // Simulated loading speed
            loadingSlider.value = Mathf.Clamp01(progress);
            percentageText.text = Mathf.RoundToInt(progress * 100f) + "%";
            yield return null;
        }

        loadingSlider.value = 1f;
        percentageText.text = "100%";

        yield return new WaitForSeconds(0.3f);
        loadingCanvas.enabled = false;
    }

    private IEnumerator PlayUnityLoadingBar(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;
            percentageText.text = Mathf.RoundToInt(progress * 100f) + "%";

            if (operation.progress >= 0.9f)
            {
                loadingSlider.value = 1f;
                percentageText.text = "100%";
                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        loadingCanvas.enabled = false;
    }

    public void UpdateLoadingUI(float progress)
    {
        loadingSlider.value = progress;
        percentageText.text = Mathf.RoundToInt(progress * 100f) + "%";
    }

}
