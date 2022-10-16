using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new evolution item")]
public class EvolutionItem : ItemBase
{
    [SerializeField] Color itemColor; // Color of the sprite if using generic sprite

    public override bool Use(Pokemon pokemon)
    {
        return true;
    }

    public Color ItemColor => itemColor;
}
