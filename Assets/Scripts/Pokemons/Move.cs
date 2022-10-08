using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    
    public MoveBase Base { get; set; }
    public int Pp { get; set; }

    public Move(MoveBase pBase)
    {
        Base = pBase;
        Pp = pBase.Pp;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetMoveByName(saveData.name);
        Pp = saveData.pp;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData()
        {
            name = Base.Name,
            pp = Pp
        };
        return saveData;
    }

    public void IncreasePP(int amount)
    {
        Pp = Mathf.Clamp(Pp + amount, 0, Base.Pp);
    }

}

[System.Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
}
