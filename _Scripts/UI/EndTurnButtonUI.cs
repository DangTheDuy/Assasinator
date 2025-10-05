using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EndTurnButtonUI : MonoBehaviour
{
    public void OnClick()
    {
        HeroUnit.SelectedHero?.OnDeselect();
        TurnManager.Instance.EndHeroTurn();
    }

}