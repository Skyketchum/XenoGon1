using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] MPBar mpBar;
    [SerializeField] Slider expSlider;
    [SerializeField] List<GameObject> hudStatuses;
    

    Xenogon _xenogon;

    public void SetData(Xenogon xenogon)
    {
        if (_xenogon != null)
        {
            _xenogon.OnStatusChanged -= ShowStatus;
            _xenogon.OnHpChanged -= UpdateHP;
            _xenogon.OnMpChanged -= UpdateMP;
        }

        _xenogon = xenogon;
        nameText.text = xenogon.Base.Name;
        SetLevel();
        hpBar.SetHP((float) xenogon.HP / xenogon.MaxHP);
        mpBar.SetMP((float) xenogon.MP / xenogon.MaxMP);
        SetExp();

        ShowStatus();
        _xenogon.OnStatusChanged += ShowStatus;
        _xenogon.OnHpChanged += UpdateHP;
        _xenogon.OnMpChanged += UpdateMP;

        //Debug.Log($"{xenogon.Base.name} HP: {xenogon.HP} / {xenogon.MaxHP} || MP: {xenogon.MP} / {xenogon.MaxHP}");
    }

    
    public void SetExp()
    {
        if (expSlider == null) return;

        expSlider.value = GetNormalizedExp();
    }

    float GetNormalizedExp()
    {
        int currentLevelExp = _xenogon.Base.GetExpForLevel(_xenogon.Level);
        int nextLevelExp = _xenogon.Base.GetExpForLevel(_xenogon.Level + 1);

        float normalizedExp = (float)(_xenogon.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void ShowStatus()
    {
        string _statusName = string.Empty;
        int _statusNum = -1;

        if (_xenogon.Status != null)
        {
            _statusName = _xenogon.Status.Id.ToString();

            // brn=0 : psn=1 : shk=2 : frz=3 : cfd=4 : dsy=5 : crs=6 : sap=7 : stg=8 : drh=9
            switch (_statusName)
            {
                case "brn":
                    _statusNum = 0;
                    break;
                case "psn":
                    _statusNum = 1;
                    break;
                case "shk":
                    _statusNum = 2;
                    break;
                case "frz":
                    _statusNum = 3;
                    break;
                case "dsy":
                    _statusNum = 5;
                    break;
                case "crs":
                    _statusNum = 6;
                    break;
                case "sap":
                    _statusNum = 7;
                    break;
                case "stg":
                    _statusNum = 8;
                    break;
            }

            for (int i = 0; i < hudStatuses.Count; ++i)
            {
                if (i == _statusNum)
                    hudStatuses[i].SetActive(true);
                else
                    hudStatuses[i].SetActive(false);
            }
        }
        else if (_xenogon.VolatileStatus != null)
        {
            _statusName = _xenogon.VolatileStatus.Id.ToString();

            // brn=0 : psn=1 : shk=2 : frz=3 : cfd=4 : dsy=5 : crs=6 : sap=7 : stg=8
            switch (_statusName)
            {
                case "cfd":
                    _statusNum = 4;
                    break;
                case "drh":
                    _statusNum = 9;
                    break;
            }

            for (int i = 0; i < hudStatuses.Count; ++i)
            {
                if (i == _statusNum)
                    hudStatuses[i].SetActive(true);
                else
                    hudStatuses[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < hudStatuses.Count; ++i)
            {
                hudStatuses[i].SetActive(false);
            }
        }
    }

    public void HideStatus()
    {
        for (int i = 0; i < hudStatuses.Count; ++i)
        {
            hudStatuses[i].SetActive(false);
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl: " + _xenogon.Level;
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_xenogon.HP / _xenogon.MaxHP);
    }
    public IEnumerator UpdateMPAsync()
    {
        yield return mpBar.SetMPSmooth((float)_xenogon.MP / _xenogon.MaxMP);
    }

    public void UpdateMP()
    {
        StartCoroutine(UpdateMPAsync());
    }

    public IEnumerator UpdateExp(bool reset=false)
    {
        if (expSlider == null) yield break;

        if (reset)
            expSlider.value = 0;

        float newExp = GetNormalizedExp();
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

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    public IEnumerator WaitForMPUpdate()
    {
        yield return new WaitUntil(() => mpBar.IsUpdatingMP == false);
    }

    public void ClearData()
    {
        if (_xenogon != null)
        {
            _xenogon.OnStatusChanged -= ShowStatus;
            _xenogon.OnHpChanged -= UpdateHP;
            _xenogon.OnMpChanged -= UpdateMP;
        }
    }
    //Smooth mp decrease for visual effect --> not in use

    /*public IEnumerator UpdateMPSmooth(int moveCost)
    {
        _xenogon.MP -= moveCost;

        if (_xenogon.MP <= 0)
            _xenogon.MP = 0;

        yield return mpBar.SetMPSmooth((float)_xenogon.MP / _xenogon.MaxMP);
    }*/

    //handled the above feature- Sky


}
