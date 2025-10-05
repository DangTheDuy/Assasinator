using UnityEngine;

public class WaterTile : Tile
{
    public override bool IsObstacle
    {
        get
        {
            // Không thể đi qua trừ khi có buff đi trên nước
            var selected = HeroUnit.SelectedHero;
            if (selected != null && selected.canWalkOnWater)
                return false;
            return true;
        }
    }

    public override void OnUnitEnter(Unit unit)
    {
        base.OnUnitEnter(unit);

        // Nếu hero có thể đi trên nước (do skill) thì không sao
        if (unit is HeroUnit hero && hero.canWalkOnWater)
            return;

        // Nếu hero không có buff -> đáng lẽ không được vào
        // nhưng vẫn check phòng trường hợp buff vừa hết khi đang ở trên nước
        if (unit is HeroUnit drowningHero && !drowningHero.canWalkOnWater)
        {
            drowningHero.StartDrowning();
        }
    }
}
