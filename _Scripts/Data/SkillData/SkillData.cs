using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;
    public bool requireTarget; 
    public SkillType type;

    public virtual void Execute(Unit caster, Unit target)
    {
        // TODO: logic skill (tấn công, buff, ám sát, ...)
        Debug.Log("Cast Skill: " + skillName);
    }
}
