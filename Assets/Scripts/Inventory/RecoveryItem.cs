using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("Max Restore")]
    [SerializeField] bool maxRestore;

    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("MP")]
    [SerializeField] int mpAmount;
    [SerializeField] bool restoreMaxMP;

    [Header("Status")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;



    [SerializeField] PartyMemberUI partymemberUI;


    public override bool Use(Xenogon xenogon)
    {
       

        var x = xenogon; // Shortening the xenogon variable to "x"

       
        if (revive || maxRevive)
        {
            if (x.HP > 0)
                return false;

            if (revive)
                x.IncreaseHP(x.MaxHP / 2); //revives fainted to half hp
            else if (maxRevive)
                x.IncreaseHP(x.MaxHP); //revies fainted to max hp

            x.CureStatus(); // cures status from previous instance before the xenogon fainted

            return true; //exuecutes up to this point
        }
        if (x.HP == 0)
            return false;

        if (maxRestore)
        {
            if (x.HP == x.MaxHP && x.MP == x.MaxMP && x.Status == null && x.VolatileStatus == null)
                return false;

            x.FullyRestoreXenogon();
        }
        else if (restoreMaxHP || hpAmount > 0) // for hp recovery items
        {
            if (x.HP == x.MaxHP)
                return false;
            if (restoreMaxHP)
                x.IncreaseHP(x.MaxHP);
            else
            x.IncreaseHP(hpAmount);
        }
        if (restoreMaxMP || mpAmount > 0)   // for mp recovery items
        {
            if (x.MP == x.MaxMP)
                return false;
            if (restoreMaxMP)
                x.IncreaseMP(x.MaxMP);
            else
                x.IncreaseMP(mpAmount);

        }

        if (recoverAllStatus || status != ConditionID.none)
        {
            if (x.Status == null && x.VolatileStatus == null)
                return false;

            if (recoverAllStatus)
            {
                x.CureStatus();
                  x.CureVolatileStatus();
            }
            else
            {
                if (x.Status.Id == status)
                    x.CureStatus();
                else if (x.VolatileStatus.Id == status)
                    x.CureVolatileStatus();
                else
                    return false;
            }
        }

        return true;
    }
}
