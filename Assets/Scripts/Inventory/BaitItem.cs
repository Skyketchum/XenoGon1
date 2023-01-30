using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new bait item")]
public class BaitItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;

    [Header("Type")]
    [SerializeField] XenogonType type;

    [Header("Level")]
    [Range(0, 3)]
    [SerializeField] int Level;


    public override bool Use(Xenogon xenogon)
    {
       // if(GameController.Instance.State == GameState.Battle || GameController.Instance.State == GameState.DoubleBattle) // item selection handles this now kept for double battle logic
              return true;
                
        
    }

    public override bool CanUseOutsideBattle => false;

    public float CatchRateModifier => catchRateModifier;
}
