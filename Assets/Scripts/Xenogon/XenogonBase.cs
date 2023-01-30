using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Xenogon", menuName = "Xenogon/Create new Xenogon")]

public class XenogonBase : ScriptableObject
{
    [SerializeField] public string Name;

    [TextArea] //adds space between name for text
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] XenogonStatus status;

    [SerializeField] XenogonType type1;
    [SerializeField] XenogonType type2;

    //base stats


   // [SerializeField] int currentMaxHP;
    [SerializeField] int maxHP;
    [SerializeField] int maxMP;
    [SerializeField] int Attack;
    [SerializeField] int Defense;
    [SerializeField] int Dexterity;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;

    [SerializeField] List<NoMpMove> noMpMove;

    public static int MaxNumOfMoves { get; set; } = 9;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }

        return -1;
    }

    public string GetName()
    {
        return Name;
    }

    public string XName
    {
        get { return Name; }
    }

   

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public XenogonStatus Status
    {
        get { return status; }
    }

    public XenogonType Type1
    {
        get { return type1; }
    }
    public XenogonType Type2
    {
        get { return type2; }


    }

    public int MaxHP
    {
        get { return maxHP; }
    }


    public int MaxMP
    {
        get { return maxMP; }
    }

    public int Atk
    {
        get { return Attack; }
    }

    public int Def
    {
        get { return Defense; }
    }

    public int Dex
    {
        get { return Dexterity; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

    public List<NoMpMove> NoMpMove
    {
        get { return noMpMove; }
    }

    public int CatchRate => catchRate;
    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;
}

[System.Serializable]
public class NoMpMove
{
    [SerializeField] MoveBase moveBase;

    public MoveBase Base
    {
        get { return moveBase; }
    }
}

[System.Serializable] //makes list appear in inspector
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;
    [SerializeField] int mpCost;


    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }

    public int MPCost
    {
        get { return mpCost; }
    }
}

public enum XenogonType
    {
        None,     /**/
        Plant,    /**/
        Primal,   /**/
        Water,    /**/
        Ice,      /**/
        Bluster,  /**/
        Fire,     /**/
        Electric, /**/
        Stone,    /**/
        Spirit,   /**/
        Glass     /**/
}

public enum GrowthRate
{
    Fast, MediumFast
}


public enum Stats
{
    Attack, 
    Defense,
    Dexterity,

    // Not an actual stat, just used to boost the moveAccuracy
    Accuracy

}


public class TypeChart
{
    // Setup effectiveness chart; order must be same as the order specified in the xenogon type enum
    static float[][] chart =
    {
        /*Attackers||Defenders: Pla   Pri   Wat   Ice   Blu   Fir   Ele   Sto   Spi  Gla*/
        /*Pla*/   new float[] { 1f,   1f,   1.5f, 1f,   1f,   1f,   1f,   1.5f, 1f },
        /*Pri*/   new float[] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f },
        /*Wat*/   new float[] { 1f,   1f,   1f,   1f,   1f,   1.5f, 1f,   1.5f, 1f },
        /*Ice*/   new float[] { 1.5f, 1f,   1f,   1f,   1f,   1f,   1.5f, 1f,   1f },
        /*Blu*/   new float[] { 1f,   1f,   1f,   1.5f, 1.5f, 1f,   1f,   1f,   1f },
        /*Fir*/   new float[] { 1.5f, 1f,   1f,   1.5f, 1f,   1f,   1f,   1f,   1f },
        /*Ele*/   new float[] { 1f,   1f,   1.5f, 1f,   1.5f, 1f,   1f,   1f,   1f },
        /*Sto*/   new float[] { 1f,   1f,   1f,   1f,   1f,   1.5f, 1.5f, 1f,   1f },
        /*Spi*/   new float[] { 1f,   0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f },

        /*Gla     new float[] { 1f,  0f,  1f,  1f,  1f,  1f,  1f,  1f,  1f }, */
    };

    public static float GetEffectiveness(XenogonType atkType, XenogonType defType) //returns the proper float for effectiveness bonuses
    {
        if (atkType == XenogonType.None || defType == XenogonType.None)
            return 1;

        int row = (int)atkType - 1;
        int col = (int)defType - 1;

        return chart[row][col];
    }
}



public enum XenogonStatus
{
    None,
    Paralyzed,
    Frozen,
    Poisoned,
    Burned
}

    

