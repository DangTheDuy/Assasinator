using UnityEngine;

public enum ItemType
{
    Heal,        
    RestoreAP,
    Shuriken,   
    BuffAttack,  
    BuffDefense  
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Properties")]
    public ItemType type;
    public int value;
    public bool consumable = true;
    public SkillData linkedSkill;
    public bool requireTarget;
}
