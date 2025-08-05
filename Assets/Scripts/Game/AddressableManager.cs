using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressableManager : MonoBehaviour
{
    [Header("Game Settings")]
    public string gameLabel;

    [Header("UI Elements")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI downloadText;
    public Slider progressSlider;
    public Button downloadButton;

    private float lastDownloadBytes = 0f;
    private float downloadSpeed = 0f;

    private void Start()
    {
        downloadButton.onClick.AddListener(() => StartDownload());

        if (IsGameAlreadyDownloaded())
        {
            statusText.text = $"{gameLabel} already downloaded.";
            downloadText.text = "";
            progressSlider.value = 1;
        }
        else
        {
            statusText.text = $"{gameLabel} not downloaded.";
            progressSlider.value = 0;
        }
    }

    public void StartDownload()
    {
        if (IsGameAlreadyDownloaded())
        {
            statusText.text = $"{gameLabel} already downloaded.";
            return;
        }

        StartCoroutine(DownloadCoroutine());
    }

    private IEnumerator DownloadCoroutine()
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(gameLabel);
        yield return locationsHandle;

        if (locationsHandle.Status != AsyncOperationStatus.Succeeded)
        {
            statusText.text = "Failed to load locations!";
            yield break;
        }

        IList<IResourceLocation> locations = locationsHandle.Result;

        var sizeHandle = Addressables.GetDownloadSizeAsync(locations);
        yield return sizeHandle;

        long totalBytes = sizeHandle.Result;
        float totalMB = totalBytes / (1024f * 1024f);

        if (totalBytes == 0)
        {
            statusText.text = "Already cached.";
            MarkGameAsDownloaded();
            yield break;
        }

        statusText.text = "Downloading...";
        var downloadHandle = Addressables.DownloadDependenciesAsync(locations, true);
        float startTime = Time.time;

        while (!downloadHandle.IsDone)
        {
            float percent = downloadHandle.PercentComplete;
            float downloadedBytes = totalBytes * percent;
            float downloadedMB = downloadedBytes / (1024f * 1024f);

            float deltaTime = Time.time - startTime;
            if (deltaTime > 0.1f)
            {
                downloadSpeed = (downloadedBytes - lastDownloadBytes) / deltaTime;
                lastDownloadBytes = downloadedBytes;
                startTime = Time.time;
            }

            downloadText.text = $"Downloaded: {downloadedMB:F2} / {totalMB:F2} MB\nSpeed: {downloadSpeed / (1024f * 1024f):F2} MB/s";
            progressSlider.value = percent;
            yield return null;
        }

        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            MarkGameAsDownloaded();
            statusText.text = $"{gameLabel} Download Complete!";
        }
        else
        {
            statusText.text = "Download Failed!";
        }

        Addressables.Release(downloadHandle);
    }

    private bool IsGameAlreadyDownloaded()
    {
        return PlayerPrefs.GetInt($"Downloaded_{gameLabel}", 0) == 1;
    }

    private void MarkGameAsDownloaded()
    {
        PlayerPrefs.SetInt($"Downloaded_{gameLabel}", 1);
        PlayerPrefs.Save();
    }
}
