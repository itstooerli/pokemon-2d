using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public PokemonBase Base
    {
        get
        {
            return _base;
        }
    }

    public int Level
    {
        get
        {
            return level;
        }
    }

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }

    public Queue<string> StatusChanges { get; private set; }
    public bool HpChanged { get; set; }
    public event System.Action OnStatusChanged;

    public void Init()
    {
        // Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
            {
                break;
            }
        }

        // Generate Stats
        CalculateStats();
        HP = MaxHp;
        
        Exp = Base.GetExpForLevel(Level);
        StatusChanges = new Queue<string>();
        ResetStatBoosts();
        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetPokemonByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;
        if (saveData.statusId != null)
        {
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        }
        else
        {
            Status = null;
        }

        // Generate Moves
        Moves = saveData.moves.Select(s => new Move(s)).ToList();

        // Generate additional data
        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoosts();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.Name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList(),
        };

        return saveData;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f + 5));
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f + 5));
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f + 5));

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f + 10 + Level);
    }

    void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 },
            { Stat.Accuracy, 0 },
            { Stat.Evasion, 0 },
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // Apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6); // -6 and 6 are min and max boost values
            
            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > _base.GetExpForLevel(Level + 1))
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

    public void LearnMove(LearnableMove moveToLearn)
    {
        // Failsafe
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;
        
        // Add move
        Moves.Add(new Move(moveToLearn.Base));
    }

    public void BoostStatsAfterLevelUp()
    {
        var oldMaxHp = MaxHp;
        CalculateStats(); // Increase stats
        var diff = MaxHp - oldMaxHp;
        UpdateHP(-diff); // Increase HP based on gain
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHp
    {
        get; private set;
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;
        float stab = (move.Base.Type == attacker.Base.Type1 || move.Base.Type == attacker.Base.Type2) ? 1.5f : 1f;

        float modifiers = Random.Range(0.85f, 1f) * type * critical * stab;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null) return;
        
        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}!");
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

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}!");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        var movesWithPp = Moves.Where(x => x.Pp > 0).ToList();
        int r = Random.Range(0, movesWithPp.Count);
        return movesWithPp[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;

        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {   
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        return canPerformMove;
    }

    public List<ConditionResponse> OnAfterTurn()
    {
        var conditionResponse = new List<ConditionResponse>();

        var statusResponse = Status?.OnAfterTurn?.Invoke(this);

        if (statusResponse != null)
        {
            conditionResponse.Add(statusResponse);
        }
        
        var volatileStatusResponse = VolatileStatus?.OnAfterTurn?.Invoke(this);

        if (volatileStatusResponse != null)
        {
            conditionResponse.Add(volatileStatusResponse);
        }
        return conditionResponse;
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoosts();
    }
}

public class DamageDetails
{
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}