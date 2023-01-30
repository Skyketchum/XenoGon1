using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moves
{
    public MoveBase Base { get; set; }

    public int MPCOST{ get; set; }



    public Moves(MoveBase xBase/*int mpCOST*/)
    {
        Base = xBase;
        MPCOST = Base.MPCost;
    }
}
