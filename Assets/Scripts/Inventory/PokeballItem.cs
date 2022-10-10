using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new pokeball item")]
public class PokeballItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;

    public override bool Use(Pokemon pokemon)
    {
        return true;
    }
    
    public override bool CanUseInTrainerBattle => false;
    public override bool CanUseOutsideBattle => false;
    public float CatchRateModifier => catchRateModifier;
}
