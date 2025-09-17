using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestTile : Tile
{
    public override float DetectionModifier => 0.5f; // giảm 50% khả năng bị phát hiện
    public override bool CanHide => true;
}

