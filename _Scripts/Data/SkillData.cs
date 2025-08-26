using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    public void Execute(Unit caster)
    {
        Debug.Log($"{caster.name} dùng skill {skillName}");
        // TODO: logic skill (tấn công, buff, ám sát, ...)
    }
}
