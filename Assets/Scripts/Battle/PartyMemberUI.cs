using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] MPBar mpBar;
    [SerializeField] GameObject selectedMember;

    Xenogon _xenogon;

    public void Init(Xenogon xenogon)
    {
        _xenogon = xenogon;
        UpdateData();

        _xenogon.OnHpChanged += UpdateData;
        _xenogon.OnMpChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _xenogon.Base.Name;
        levelText.text = "Lvl" + _xenogon.Level;
        hpBar.SetHP((float)_xenogon.HP / _xenogon.MaxHP);
        mpBar.SetMP((float)_xenogon.MP / _xenogon.MaxMP);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = GlobalSettings.i.HighlightedColor;
        else
            nameText.color = Color.black;
    }

    public void ShowSelectedMember(bool enabled)
    {
        selectedMember.SetActive(enabled);
    }
}
