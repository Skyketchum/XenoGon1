using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conditions_DataBase : MonoBehaviour
{

    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        //Regular Status Conditions
        {
            ConditionID.brn, //1/8 max hp per turn
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Xenogon xenogon) =>
                {
                    xenogon.DecreaseHP(xenogon.MaxHP / 8);
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name} was hurt due to burn");
                }
            }
        },
        {
            ConditionID.psn, //1/8 max hp per turn
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Xenogon xenogon) =>
                {
                    xenogon.DecreaseHP(xenogon.MaxHP / 8);
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name} was hurt due to poison");
                }
            }
        },
        {
            ConditionID.shk, //50% chance to damage self with clash damage (45) instead of hitting opponent
            new Condition()
            {
                Name = "Shock",
                StartMessage = "is shocked",
                OnBeforeMove = (Xenogon xenogon) =>
                {
                    //50% chance to do a move
                    if (Random.Range(1, 3) == 1)
                        return true;

                    //Hurt by confusion if move didn't go through
                    xenogon.DecreaseHP(45);
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name} hurt itself due to shock.");
                    return false;
                }
            }
        },
        {
            ConditionID.frz, //takes 1/8 max hp every time you land an attack
            new Condition()
            {
                Name = "Frost",
                StartMessage = "is frozen",
                OnAfterAttack = (Xenogon xenogon) =>
                {
                    xenogon.DecreaseHP(xenogon.MaxHP / 8);
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name} was hurt due to frost.");
                }
            }
        },
        {
            ConditionID.dsy, //5% miss chance goes to 25% miss chance
            new Condition()
            {
                Name = "Drowsy",
                StartMessage = "is drowsy",
                OnStart = (Xenogon xenogon) =>
                {
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name}'s accuracy was greatly reduced!");
                }
            }
        },
        {
            ConditionID.crs, //doubles mp costs
            new Condition()
            {
                Name = "Curse",
                StartMessage = "has been cursed",
                OnStart = (Xenogon xenogon) =>
                {
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name}'s mana costs are greatly increased!");
                }
            }
        },
        {
            ConditionID.sap, //1/16 max hp goes to the opponent per turn (1/16 taken is the amount healed)
            new Condition()
            {
                Name = "Sapped",
                StartMessage = "has been sapped",
                OnAfterTurn = (Xenogon xenogon) =>
                {
                    xenogon.DecreaseHP(xenogon.MaxHP / 16);
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name} was sapped.");
                }
            }
        },
        {
            ConditionID.stg, //5% crit chance goes to 15% crit chance against staggered xenogon | staggered xenogon can't crit
            new Condition()
            {
                Name = "Stagger",
                StartMessage = "has been staggered",
                OnStart = (Xenogon xenogon) =>
                {
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name} can't crit.");
                }
            }
        },

        //Volatile Status Conditions
        {
            ConditionID.cfd, //1-5 turns random | 50% chance to damage self with 45 damage instead of hitting opponent
            new Condition()
            {
                Name = "Confuse",
                StartMessage = "is confused",
                OnStart = (Xenogon xenogon) =>
                {
                    // Confuse for 1-5 turns
                    xenogon.StatusTime = Random.Range(1, 6);
                    Debug.Log($"{xenogon.StatusTime} moves");
                },
                OnBeforeMove = (Xenogon xenogon) =>
                {
                    if (xenogon.VolatileStatusTime <= 0)
                    {
                        xenogon.CureVolatileStatus();
                        xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name} snapped out of confusion!");
                        return true;
                    }
                    xenogon.StatusTime--;

                    //50% chance to do a move
                    if (Random.Range(1, 3) == 1)
                        return true;

                    //Hurt by confusion if move didn't go through
                    xenogon.DecreaseHP(45);
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name} is confused and hurt itself.");
                    return false;
                }
            }
        },
        {
            ConditionID.drh, //swap xenogon to remove | amplifies elemental weakness (25% more damage on top)
            new Condition()
            {
                Name = "Drench",
                StartMessage = "is drenched",
                OnStart = (Xenogon xenogon) =>
                {
                    xenogon.StatusChanges.Enqueue($"{xenogon.Base.Name} takes extra damage from weaknesses!");
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.dsy || condition.Id == ConditionID.frz)
            return 2f;
        else if (condition.Id == ConditionID.shk || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionID
{
    none, brn, psn, shk, frz, cfd, dsy, crs, sap, stg, drh
    /* none
     * burn
     * poison
     * shock
     * frost
     * confuse
     * drowsy
     * curse
     * sapped
     * stagger
     * drench
     */
}
