using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new SecretMove item")]
public class SecretMoveItem : ItemBase
{

    [SerializeField] MoveBase move;
    [SerializeField] bool isHiddenMove;

    public override string Name => base.Name + $": {move.name}";


    public override bool Use(Xenogon xenogon)
    {

        //Learning move is handle from Inventory UI, if it was learned then return true

        return xenogon.HasMove(move);
    }

    public override bool IsReusable => isHiddenMove;

    public override bool CanUseInBattle => false;


    public MoveBase Move => move;

    public bool IsHiddenMove => isHiddenMove;
  
}
