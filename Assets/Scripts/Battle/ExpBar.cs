using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpBar : MonoBehaviour
{
    [SerializeField] GameObject exp;
    [SerializeField] Slider expSlider;

    public void SetExp(float expNormalized)
    {
        expSlider.value = expNormalized;
    }

    public IEnumerator SetExpSmooth(float newExp)
    {
        float curExp = expSlider.value;
        float changeAmount;

        changeAmount = newExp - curExp;
        
        while (newExp - curExp > Mathf.Epsilon)
        {
            curExp += changeAmount * Time.deltaTime;
            expSlider.value = curExp;
            yield return null;
        }

        expSlider.value = newExp;
    }
}
