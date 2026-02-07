using System.Collections;
using UnityEngine;

public class ShowWithFade : MonoBehaviour
{
    public GameObject componentToShow;  // The UI GameObject
    private float delayTime = 17f;      // Time before showing (17 seconds)
    private float fadeDuration = 2f;    // Duration of fade-in

    void Start()
    {
        // Ensure the object starts hidden
        CanvasGroup canvasGroup = componentToShow.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = componentToShow.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;
        componentToShow.SetActive(true);

        StartCoroutine(ShowAfterDelay(canvasGroup));
    }

    IEnumerator ShowAfterDelay(CanvasGroup canvasGroup)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delayTime);

        // Fade-in effect
        float timer = 0f;
        while (timer <= fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }
}
