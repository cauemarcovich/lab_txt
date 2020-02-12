using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    public IEnumerator FadeIn(float duration, float max = 1f)
    {
        yield return StartCoroutine(Fade(FadeDirection.In, duration, max));
    }

    public IEnumerator FadeOut(float duration, float max = 1f)
    {
        yield return StartCoroutine(Fade(FadeDirection.Out, duration, max));
    }

    /* FADE */
    IEnumerator Fade(FadeDirection dir, float duration, float max)
    {
        var blackScreen = GetComponent<SpriteRenderer>();
        if (dir == FadeDirection.In)
        {
            for (float i = 1; i >= 0; i -= Time.deltaTime / duration)
            {
                var newColor = new Color(0, 0, 0, Mathf.Lerp(0, max, i));
                blackScreen.color = newColor;
                yield return null;
            }
        }
        else
        {
            for (float i = 0; i <= 1; i += Time.deltaTime / duration)
            {
                var newColor = new Color(0, 0, 0, Mathf.Lerp(0, max, i));
                blackScreen.color = newColor;
                yield return null;
            }
        }
    }

    enum FadeDirection { In, Out }
}