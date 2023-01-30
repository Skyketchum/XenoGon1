using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MPBar : MonoBehaviour
{
    [SerializeField] GameObject mana;
    [SerializeField] Slider manaSlider;


    public bool IsUpdatingMP { get; private set; }

    public void SetMP(float mpNormalized)
    {
        manaSlider.value = mpNormalized;
        //mana.transform.localScale = new Vector3(mpNormalized, 1f);
       
    }

    public IEnumerator SetMPSmooth(float newMP)
    {
        IsUpdatingMP = true;

        float curMP = manaSlider.value;
        float changeAmount;
        bool decreased;

        if (newMP < curMP)
            decreased = true;
        else
            decreased = false;

        if (decreased)
        {
            changeAmount = curMP - newMP;

            while (curMP - newMP > Mathf.Epsilon)
            {
                curMP -= changeAmount * Time.deltaTime;
                manaSlider.value = curMP;
                yield return null;
            }
        }
        else
        {
            changeAmount = newMP - curMP;

            while (newMP - curMP > Mathf.Epsilon)
            {
                curMP += changeAmount * Time.deltaTime;
                manaSlider.value = curMP;
                yield return null;
            }
        }

        manaSlider.value = newMP;

        IsUpdatingMP = false;
    }

   /* public IEnumerator SetMPSmooth(float newMP) //slowly lower mp for visual effect
    {


        IsUpdatingMP = true;

        float curMP = manaSlider.value;
        float changeAmount = curMP - newMP;

        while (curMP - newMP > Mathf.Epsilon)
        {
            curMP -= changeAmount * Time.deltaTime;
            manaSlider.value = curMP;
            yield return null;
        }
        manaSlider.value = newMP;

        IsUpdatingMP = false;
    }
    */



}
