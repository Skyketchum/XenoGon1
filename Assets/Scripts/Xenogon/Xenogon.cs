using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Xenogon
{
    [SerializeField] XenogonBase _base;
    [SerializeField] int level;
    

    public Xenogon(XenogonBase xBase, int xLevel)
    {
        _base = xBase;
        level = xLevel;

        Init();
    }

    public XenogonBase Base {
        get {
            return _base;
        }
    }
    public int Level {
        get {
            return level;
        }
    }

    private XenogonStatus status;

    private XenogonStatus GetStatus()
    {
        return status;
    }

    private void SetStatus(XenogonStatus value)
    {
        status = value;
    }

    public int Exp { get; set; }

    public int HP { get; set; }

    public int MP { get; set; }




    public List<Moves> Moves { get; set; }
    public List<Moves> noMpMove { get; set; }
    public Moves CurrentMove { get; set; }
    public Dictionary<Stats, int> XStats { get; private set; }
    public Dictionary<Stats, int> XStatBoost { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public bool Drenched { get; set; }
    public bool Staggered { get; set; }
    public bool cannotCrit { get; set; }

    public Queue<string> StatusChanges { get; private set; } //Queue is used to store a list of elements like a 'List', but you can take out elements and it will use elements in the order added; basically used to simplify code more so than if using a List.

    public event System.Action OnStatusChanged;
    public event System.Action OnHpChanged;
    public event System.Action OnMpChanged;
    // public bool HpDecreased { get; set; } // controls which way the hp goes in HUD

    // public Dictionary<Stats, int> XStatDebuff{ get; private set; }

    //Xenogon constructor for Xenogon reference and level assoc
    public void Init()
    {
        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHP;
        MP = MaxMP;

        MakeMoveList(true);

        StatusChanges = new Queue<string>();

        ResetStatBoosts();
        Status = null;
        VolatileStatus = null;
    }

    public void MakeMoveList(bool init = false)
    {
        bool struggle = false;

        
        if (init)
        {
            Moves = new List<Moves>();
            foreach (var moves in Base.LearnableMoves)
            {
                if (moves.Level <= Level)
                    Moves.Add(new Moves(moves.Base));

                if (Moves.Count >= XenogonBase.MaxNumOfMoves)
                    break;
            }
        }
        
        /*
        Moves = new List<Moves>();
        foreach (var moves in Base.LearnableMoves)
        {
            if (moves.Level <= Level)
                Moves.Add(new Moves(moves.Base));

            if (Moves.Count >= XenogonBase.MaxNumOfMoves)
                break;
        }
        */

        foreach (var moves in Moves)
        {
            if (MP >= moves.MPCOST)
            {
                struggle = false;
                break;
            }
            else
                struggle = true;
        }

        if (struggle)
        {
            Debug.Log("not enough MP");
            Moves.Clear();

            noMpMove = new List<Moves>();
            foreach (var moves in Base.NoMpMove)
            {
                Moves.Add(new Moves(moves.Base));
            }
        }
    }

    public Xenogon(XenogonSaveData saveData)
    {
        _base = Xenogon_DataBase.GetXenogonByName(saveData.name);
        HP = saveData.hp;
        MP = saveData.mp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
            Status = Conditions_DataBase.Conditions[saveData.statusId.Value];
        else
            Status = null;

        MakeMoveList(true);

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoosts();
        VolatileStatus = null;
    }

    public XenogonSaveData GetSaveData()
    {
        var saveData = new XenogonSaveData()
        {
            name = Base.name,
            hp = HP,
            mp = MP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id
        };

        return saveData;
    }

    void CalculateStats()
    {
        XStats = new Dictionary<Stats, int>();
        XStats.Add(Stats.Attack, Mathf.FloorToInt((Base.Atk * Level) / 100f) + 5);
        XStats.Add(Stats.Defense, Mathf.FloorToInt((Base.Def * Level) / 100f) + 5);
        XStats.Add(Stats.Dexterity, Mathf.FloorToInt((Base.Dex * Level) / 100f) + 5);
        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + level;
        MaxMP = 100;
    }

    void ResetStatBoosts()
    {
        XStatBoost = new Dictionary<Stats, int>()
        {
            {Stats.Attack, 0},
            {Stats.Defense, 0},
            {Stats.Dexterity, 0},
        };
    }

    int GetStat(Stats stat)
    {
        int statVal = XStats[stat];

        // Apply stat boost

        int boost = XStatBoost[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            XStatBoost[stat] = Mathf.Clamp(XStatBoost[stat] + boost, -6, 6);

            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            else
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");

            Debug.Log($"{stat} has been boosted tp {XStatBoost[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }

        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > XenogonBase.MaxNumOfMoves)
            return;

        Moves.Add(new Moves(moveToLearn));
    }
   public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;
    }

    public int Atk
    {
        get { return GetStat(Stats.Attack); }
    }

    public int Def
    {
        get { return GetStat(Stats.Defense); }
    }

    public int Dex
    {
        get { return GetStat(Stats.Dexterity); }
    }

    public int MaxHP
    {
        get; private set;
    }

    public int MaxMP
    {
        get; private set;
    }

    public DamageDetails TakeDamage(Moves move, Xenogon attacker) //calculate damage for the move used on the specific xenogon
    {
        float critical = 1f;
        float critChance = 5f;

        if (Staggered)
            critChance = 15;

        if (cannotCrit)
            critChance = 0;

        if (Random.value * 100f <= critChance) //5% is default crit chance
            critical = 1.25f; //25% is default crit damage


        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false,
        };

        if (Drenched)
        {
            if (type != 1)
            {
                type = 1.75f;
            }
        }

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.GetPower * ((float)attacker.Atk / Def) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);

        //Debug.Log($"Attacker Crit Chance: {critChance}");
        //Debug.Log($"Elemental Weakness: {type}");

        Staggered = false;
        cannotCrit = false;
        Drenched = false;

        return damageDetails;
    }

    public void FullyRestoreXenogon() // Restore a Xenogon back to full for everything
    {
        HP = MaxHP;
        MP = MaxMP;
        CureStatus();
        CureVolatileStatus();
        OnHpChanged?.Invoke();
        OnMpChanged?.Invoke();
        OnStatusChanged?.Invoke();
    }

    public void IncreaseHP(int amount) // Heal Xenogon
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHP);
        OnHpChanged?.Invoke();
    }

    public void DecreaseHP(int damage) // Damage Xenogon
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        OnHpChanged?.Invoke();
    }

    public void IncreaseMP(int amount)
    {
        MP = Mathf.Clamp(MP + amount, 0, MaxMP);
        OnMpChanged?.Invoke();
    }
    
    public void DecreaseMP(int amount)
    {
        MP = Mathf.Clamp(MP - amount, 0, MaxMP);
        OnMpChanged?.Invoke();
    }

    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null) return;

        Status = Conditions_DataBase.Conditions[conditionId];
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        Status?.OnStart?.Invoke(this);
        OnStatusChanged?.Invoke();
    } 

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = Conditions_DataBase.Conditions[conditionId];
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
        VolatileStatus?.OnStart?.Invoke(this);
        OnStatusChanged?.Invoke();
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
        OnStatusChanged?.Invoke();
    }

    public void Heal()
    {
        HP = MaxHP;
        OnHpChanged?.Invoke();
    }
    public Moves GetRandomMove() //gets a random move for wild xenogon ai
    {
        var movesAvailable = Moves.Where(x => x.MPCOST <= MP).ToList(); //List of moves for the xenogon that is 

        if (movesAvailable.Count > 0)
        {
            int r = Random.Range(0, movesAvailable.Count);
            return movesAvailable[r];
        }
        else
        {
            return noMpMove[0];
        }
        
    }

    public void OnAfterTurn()
    {
        VolatileStatus?.OnAfterTurn?.Invoke(this);
        Status?.OnAfterTurn?.Invoke(this); //the '?' is a nonconditional operator that is called only if 'Status' and 'OnAfterTurn' is not null
    }

    public void OnAfterAttack()
    {
        VolatileStatus?.OnAfterAttack?.Invoke(this);
        Status?.OnAfterAttack?.Invoke(this);
    }

    public bool OnBeforeMove()
    {

        if(Status?.OnBeforeMove != null)
        {
            return Status.OnBeforeMove(this);
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            return VolatileStatus.OnBeforeMove(this);
        }

        return true;
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoosts();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class XenogonSaveData
{
    public string name;
    public int hp;
    public int mp;
    public int level;
    public int exp;
    public ConditionID? statusId;
}
