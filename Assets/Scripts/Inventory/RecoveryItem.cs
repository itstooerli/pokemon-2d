using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Pokemon pokemon)
    {
        // Revive
        if (revive || maxRevive)
        {
            if (pokemon.HP > 0) return false;

            if (revive)
                pokemon.IncreaseHP(pokemon.MaxHp / 2);
            else if (maxRevive)
                pokemon.IncreaseHP(pokemon.MaxHp);

            pokemon.CureStatus();
            pokemon.CureAllVolatileStatuses();
            return true;
        }

        if (pokemon.HP == 0) return false;

        // Restore HP
        if (restoreMaxHP || hpAmount > 0)
        {
            if (pokemon.HP == pokemon.MaxHp) return false;

            if (restoreMaxHP)
                pokemon.IncreaseHP(pokemon.MaxHp);
            else
                pokemon.IncreaseHP(hpAmount);
        }

        // Recover Status
        if (recoverAllStatus || status != ConditionID.none)
        {
            if (pokemon.Status == null && pokemon.VolatileStatuses.Count == 0) return false;

            if (recoverAllStatus)
            {
                pokemon.CureStatus();
                pokemon.CureOneVolatileStatus(ConditionID.confusion); // Recover all status only recognizes confusion as a valid volatile status to heal
            }
            else
            {
                if (pokemon.Status.Id == status)
                {
                    pokemon.CureStatus();
                }
                else if (pokemon.VolatileStatuses.ContainsKey(status))
                {
                    pokemon.CureOneVolatileStatus(status);
                }
                else
                {
                    return false;
                }
            }
        }

        // Restore PP
        if (restoreMaxPP)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(m.Base.Pp));
        }
        else if (ppAmount > 0)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }

        return true;
    }
}
