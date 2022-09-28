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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
