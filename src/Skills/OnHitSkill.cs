using Godot;

public class OnHitSkill : Skill
{
    public OnHitSkill()
    {
        Name = "受伤反击";
        Description = "受到伤害时,对周围敌人造成伤害";
        Cooldown = 3.0f;
        IsPassive = true;
        TriggerType = SkillTriggerType.OnHit;
    }

    public override void Trigger(Node source)
    {
        // 实现受伤反击效果
        GD.Print($"触发{Name}!");
        CurrentCooldown = Cooldown;
        
        // TODO: 对周围敌人造成伤害
    }
} 