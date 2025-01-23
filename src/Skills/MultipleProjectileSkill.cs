using Godot;

public class MultipleProjectileSkill : Skill
{
    public MultipleProjectileSkill()
    {
        Name = "低阶多重投射";
        Description = "使投射物分裂成多个";
        Cooldown = 5.0f;
        IsPassive = true;
        TriggerType = SkillTriggerType.Passive;
    }

    public override void Trigger(Node source)
    {
        GD.Print($"触发{Name}!");
        CurrentCooldown = Cooldown;

        // TODO: 分裂投射物逻辑
    }
} 