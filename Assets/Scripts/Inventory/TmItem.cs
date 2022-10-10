using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Pokemon pokemon)
    {
        // Learning move is handled from Inventory UI. If it was learned, return true;
        return pokemon.HasMove(move);
    }
    
    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.TeachableMoves.Any(tm => tm.move == move);
    }

    // In this version, TMs can be reusable and no HMs exist.
    public override bool IsReusable => true;
    public override bool CanUseInBattle => false;

    public MoveBase Move => move;
}
