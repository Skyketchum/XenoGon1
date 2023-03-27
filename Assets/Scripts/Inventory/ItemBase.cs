 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class that all items are to be derived from.
//Virtual key word setsup overriding
public class ItemBase : ScriptableObject
{
    [SerializeField] string itemName;
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] string usedDialog;

    public virtual string Name => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public string UsedDialog => usedDialog;

    public virtual bool Use(Xenogon xenogon)
    {
        return false;
    }

    public virtual bool IsReusable => false;

    public virtual bool CanUseInBattle => true;   /// <summary>
    /// 
    /// theses two virtual bools help control inside and outside item usage
    /// </summary>

    public virtual bool CanUseOutsideBattle => true;
}
