using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Move",  menuName = "Xenogon/Create new move")]

public class MoveBase : ScriptableObject
{
    [SerializeField] string moveName;

    [TextArea] //adds space between name for text
    [SerializeField] string moveDescription;

    //   [SerializeField] Sprite moveFrontSprite;
    //   [SerializeField] Sprite moveBackSprite;

    [SerializeField] XenogonType type;

    [SerializeField] int Power;

    // added move accuracy and bool for always hit option | currently not incorporated
    [SerializeField] bool alwaysHits = false;

    /*[SerializeField] int BuffAttack;
    [SerializeField] int DebuffAttack;
    [SerializeField] int BuffDefense;
    [SerializeField] int DebuffDefense;
    */
    [SerializeField] int mpCost;
    [SerializeField] int priority;
    [SerializeField] MoveCategory category;

    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] MoveTarget target;
    [SerializeField] MoveTarget secondTarget;


    public string GetName()
    {
        return moveName;
    }

    public string XMoveName
    {
        get { return moveName; }
    }

    public string XMoveDescription
    {
        get { return moveDescription; }
    }

    /*
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return BackSprite; }
    }
    */

    public XenogonType Type
    {
        get { return type; }
    }

    public int GetPower
    {
        get { return Power; }
    }

    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }

    /*
    public int GetBuffAttack
    {
        get { return BuffAttack; }
    }


    public int GetDebuffAttack
    {
        get { return DebuffAttack; }
    }

    public int GetBuffDefense
    {
        get { return BuffDefense; }
    }


    public int GetDebuffDefense
    {
        get { return DebuffDefense; }
    }

  */
    public int MPCost
    {
        get { return mpCost; }
    }

    public int Priority
    {
        get { return priority; }
    }

    public MoveCategory Category
    {
        get { return category; }
    }

  

    public MoveEffects Effects
    {
        get { return effects; }
    }

    public List<SecondaryEffects> Secondaries
    {
        get { return secondaries; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }

    public MoveTarget SecondTarget
    {
        get { return secondTarget; }
    }
    

}

  [System.Serializable]

public class MoveEffects
{

    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }

    public ConditionID Status
    {
        get { return status; }
    }

    public ConditionID VolatileStatus
    {
        get { return volatileStatus; }
    }

}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance; //percent chance to cause the secondary effect
    [SerializeField] MoveTarget target; //target for the secondary effect
    [SerializeField] MoveTarget secondTarget;

    public int Chance
    {
        get { return chance; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }

   public MoveTarget SecondTarget
    {
        get { return secondTarget; }
    }
}

[System.Serializable]

public class StatBoost
{
    public Stats stat;
    public int boost;


}

public enum MoveCategory
{
    Physical, Special, Status, Multi, Double, Null}


public enum MoveTarget
{
    Foe, Self, Partner, Player, EnemyPartner, Null
}


//back up move type enum
/*
  public enum MoveType
 {
    None,
    Plant,
    Primal,
    Water,
    Ice,
    Bluster,
    Fire,
    Electric,
    Stone,
    Spirit,
    Glass
  }
  */

