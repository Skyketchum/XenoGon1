using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    Image image;
    Image image2;


    private void Awake()
    {
        image = GetComponent<Image>();
        image2 = GetComponentInChildren<Image>();
    }

    public IEnumerator FadeIn(float time)
    {
       yield return image.DOFade(1f, time).WaitForCompletion();
    }

    public IEnumerator Banner(float time)
    {
        yield return image2.DOFillAmount(1f, time).WaitForCompletion();
    }

    public IEnumerator BannerOff(float time)
    {
        yield return image2.DOFillAmount(0f, time).WaitForCompletion();
    }



    public IEnumerator FadeOut(float time)
    {
        yield return image.DOFade(0f, time).WaitForCompletion();
    }
}
