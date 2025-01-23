using Godot;

public class FireballSkill : Skill
{
    public FireballSkill()
    {
        Name = "火球术";
        Description = "发射一颗火球,对敌人造成火焰伤害";
        Cooldown = 1.0f;
        IsPassive = false;
        TriggerType = SkillTriggerType.Manual;
    }

    public override void Trigger(Node source)
    {
        GD.Print($"释放{Name}!");
        CurrentCooldown = Cooldown;

        // TODO: 生成火球预制体
        // TODO: 设置火球飞行方向
        // TODO: 添加火球伤害逻辑
    }
} 