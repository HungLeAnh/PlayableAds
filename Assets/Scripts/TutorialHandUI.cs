using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TutorialHandUI : MonoBehaviour
{
    public RectTransform handRect;
    public float moveDuration = 1.5f;
    private Coroutine animationCoroutine;

    public void Show(GridCell startCell, GridCell endCell)
    {
        gameObject.SetActive(true);
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(PingPongAnimation(startCell, endCell));
    }

    public void Hide()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        gameObject.SetActive(false);
    }

    IEnumerator PingPongAnimation(GridCell start, GridCell end)
    {
        Vector3 startPos = start.transform.position;
        Vector3 endPos = end.transform.position;

        while (true)
        {
            float elapsed = 0;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                handRect.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            
            elapsed = 0;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                handRect.position = Vector3.Lerp(endPos, startPos, elapsed / moveDuration);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
