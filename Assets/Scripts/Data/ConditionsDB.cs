using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
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
        {ConditionID.psn, new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon) => 
                    {
                        pokemon.UpdateHP(pokemon.MaxHp / 8);
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by poison!");
                    }
            } 
        },

        {ConditionID.brn, new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                    {
                        pokemon.UpdateHP(pokemon.MaxHp / 16);
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by its burn!");
                    }
            }
        },

        {ConditionID.par, new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Pokemon pokemon) =>
                    {
                        if (Random.Range(1, 5) == 1)
                        {
                            pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is paralzyed and can't move!");
                            return false;
                        }
                        return true;
                    }
            }
        },

        {ConditionID.frz, new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Pokemon pokemon) =>
                    {
                        if (Random.Range(1, 5) == 1)
                        {
                            pokemon.CureStatus();
                            pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is not frozen anymore.");
                            return true;
                        }
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is frozen and can't move!");
                        return false;
                    }
            }
        },

        {ConditionID.slp, new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Pokemon pokemon) =>
                    {
                        // Sleep for 1-3 turns
                        pokemon.StatusTime = Random.Range(1, 4);
                        Debug.Log($"Will be asleep for {pokemon.StatusTime} moves");
                    },

                OnBeforeMove = (Pokemon pokemon) =>
                    {        
                        if (pokemon.StatusTime <= 0)
                        {
                            pokemon.CureStatus();
                            pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                            return true;
                        }

                        pokemon.StatusTime--;
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is asleep...");
                        return false;
                    }
            }
        },

        {ConditionID.confusion, new Condition()
            {
                Name = "Confusion",
                StartMessage = "became confused",
                OnStart = (Pokemon pokemon) =>
                    {
                        // Confused for 1-4 turns
                        pokemon.VolatileStatusTime = Random.Range(1, 5);
                        Debug.Log($"Will be confused for {pokemon.VolatileStatusTime} moves");
                    },

                OnBeforeMove = (Pokemon pokemon) =>
                    {
                        if (pokemon.VolatileStatusTime <= 0)
                        {
                            pokemon.CureVolatileStatus();
                            pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is no longer confused!");
                            return true;
                        }

                        pokemon.VolatileStatusTime--;
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused...");

                        // 50% chance to do a move
                        if (Random.Range(1,3) == 1)
                        {
                            return true;
                        }

                        // Hurt by confusion
                        pokemon.UpdateHP(pokemon.MaxHp / 8);
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself in its confusion!");
                        return false;
                    }
            }
        },

        {ConditionID.flinch, new Condition()
            {
                Name = "Flinch",
                StartMessage = "flinched",
                OnStart = (Pokemon pokemon) =>
                    {
                        Debug.Log($"Pokemon might flinch");
                    },

                OnBeforeMove = (Pokemon pokemon) =>
                    {
                        Debug.Log($"Pokemon might have been flinched...");
                        return true;
                    },

                OnAfterTurn = (Pokemon pokemon) =>
                    {
                        pokemon.CureVolatileStatus();
                    }
            }
        },
    };

    public static float GetStatusBonus(Condition condition) 
    {
        if (condition == null)
        {
            return 1f;
        }
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
        {
            return 2f;
        }
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
        {
            return 1.5f;
        }

        return 1f;
    }
}

public enum ConditionID
{
    none,
    psn,
    brn,
    slp,
    par,
    frz,
    confusion,
    flinch,
}