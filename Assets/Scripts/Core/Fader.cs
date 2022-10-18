using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public static Fader i { get; private set; }
    Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        i = this;
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
