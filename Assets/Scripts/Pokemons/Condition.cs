using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }

    public Action<Pokemon> OnStart { get; set; }
    public Func<Pokemon, bool> OnBeforeMove { get; set; }
    public Func<Pokemon, ConditionResponse> OnAfterTurn { get; set; }
}

/// <summary>
///  CUSTOM: Included class to accommodate responses to source based on target condition, such as leech seed
/// </summary>
public class ConditionResponse
{
    public int LeechSeedGain { get; set; } = 0;
}