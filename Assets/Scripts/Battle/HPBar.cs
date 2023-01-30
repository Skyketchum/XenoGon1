using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;
    [SerializeField] Slider healthSlider;

    public bool IsUpdating { get; private set; }

    public void SetHP(float hpNormalized)
    {
        healthSlider.value = hpNormalized;
        //health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    public IEnumerator SetHPSmooth(float newHP) //slowly lower hp for visual effect
    {
        IsUpdating = true;

        float curHP = healthSlider.value;
        float changeAmount;
        bool decreased;

        if (newHP < curHP)
            decreased = true;
        else
            decreased = false;

        if (decreased)
        {
            changeAmount = curHP - newHP;

            while (curHP - newHP > Mathf.Epsilon)
            {
                curHP -= changeAmount * Time.deltaTime;
                healthSlider.value = curHP;
                yield return null;
            }
        }
        else
        {
            changeAmount = newHP - curHP;

            while (newHP - curHP > Mathf.Epsilon)
            {
                curHP += changeAmount * Time.deltaTime;
                healthSlider.value = curHP;
                yield return null;
            }
        }

        healthSlider.value = newHP;

        IsUpdating = false;
    }

    private void Update()
    {
        if(healthSlider.value <= 0.25)
            health.GetComponent<Image>().color = Color.red;
        else if(healthSlider.value <= 0.5)
            health.GetComponent<Image>().color = Color.yellow;
        else
            health.GetComponent<Image>().color = Color.green;
        
        //Color.Lerp(Color.green, Color.yellow, healthSlider.value); --> trying to gradually change color
    }
}
