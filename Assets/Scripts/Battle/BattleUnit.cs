using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    //[SerializeField] XenogonBase _base;
    //[SerializeField] int level;
    [SerializeField] int CurrMaxMP;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }
    public bool IsPlayerUnit { 
        get { return isPlayerUnit; }
    }

    public BattleHud Hud
    {
        get { return hud; }
    }

    public Xenogon Xenogon { get; set; }

    public void Setup(Xenogon xenogon)
    {
        //Xenogon = new Xenogon(_base, level, CurrMaxMP);
        Xenogon = xenogon;
        if (isPlayerUnit)
            GetComponent<Image>().sprite = Xenogon.Base.BackSprite;
        else
            GetComponent<Image>().sprite = Xenogon.Base.FrontSprite;

        transform.localScale = new Vector3(1, 1, 1);
        hud.gameObject.SetActive(true);
        image.color = originalColor;
        hud.SetData(xenogon);
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    /* public void PlayEnterAnimation()
     {
         if(isPlayerUnit)

     } */

    // /*

    public IEnumerator PlayTakeBaitAnimation(GameObject bait)
    {
        var sequence = DOTween.Sequence();
        sequence.Join(image.transform.DOLocalMoveX(originalPos.x + 15f, 1f));
        sequence.Join(image.transform.DOLocalMoveY(originalPos.y - 10f, 1f));
        yield return sequence.WaitForCompletion();
        bait.SetActive(false);
        sequence.Join(image.transform.DOLocalMoveX(originalPos.x, 1f));
        sequence.Join(image.transform.DOLocalMoveY(originalPos.y, 1f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }
    // */

}
