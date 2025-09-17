using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTile : Tile
{
    public override bool IsObstacle => true; // không thể đi qua
}

