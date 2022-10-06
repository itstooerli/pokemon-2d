using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public IEnumerator FadeIn(float time)
    {
        yield return image.DOFade(1.0f, time).WaitForCompletion();
    }

    public IEnumerator FadeOut(float time)
    {
        yield return image.DOFade(0.0f, time).WaitForCompletion();
    }
}
