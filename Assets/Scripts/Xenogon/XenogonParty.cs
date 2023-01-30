using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class XenogonParty : MonoBehaviour
{
    [SerializeField]List<Xenogon> xenogon;

    public event Action OnUpdated;
    public List<Xenogon> XenogonList
    {
        get
        {
            return xenogon;
        }
        set
        {
            xenogon = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake() //sets the players xenogon before party screen is called from Start
    {
        foreach (var _xenogon in xenogon)
        {
            _xenogon.Init();
        }
    }

    private void Start()
    {
       
    }

    public Xenogon GetHealthyXenogon()
    {
        return xenogon.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddXenogon(Xenogon newXenogon)
    {
        Debug.Log($"{newXenogon.Base.Name} was added to the pc.");
        
        if (XenogonList.Count < 6)
        {
            XenogonList.Add(newXenogon);
            OnUpdated?.Invoke();
        }
        else
        {
            //add to pc
        }
        
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }

    public static XenogonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<XenogonParty>();
    }
}
