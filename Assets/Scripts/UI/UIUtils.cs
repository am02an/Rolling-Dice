using UnityEngine;
using System.Collections;

public class UIUtils : MonoBehaviour
{
    public static IEnumerator FadeCanvasGroup(string gameObjectName, float targetAlpha, float duration, bool setInteractable)
    {
        Debug.Log($"[FadeCanvasGroup] Attempting to fade GameObject: {gameObjectName}");

        GameObject obj = GameObject.Find(gameObjectName);
        if (obj == null)
        {
            Debug.LogWarning($"[FadeCanvasGroup] GameObject '{gameObjectName}' not found.");
            yield break;
        }

        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning($"[FadeCanvasGroup] CanvasGroup not found on '{gameObjectName}'.");
            yield break;
        }

        Debug.Log($"[FadeCanvasGroup] Starting fade: {gameObjectName} from {canvasGroup.alpha} to {targetAlpha} over {duration}s");

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            canvasGroup.alpha = newAlpha;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = setInteractable;
        canvasGroup.blocksRaycasts = setInteractable;

        Debug.Log($"[FadeCanvasGroup] Finished fading {gameObjectName} to {targetAlpha}. Interactable: {setInteractable}");
    }

}
