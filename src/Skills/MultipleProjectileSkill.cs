using Godot;

public partial class MultipleProjectileSkill : Skill
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

        // 查找火球技能并启用多重投射
        if (source is SkillSlot skillSlot)
        {
            foreach (var skill in skillSlot.GetSkills())
            {
                if (skill is FireballSkill fireballSkill)
                {
                    fireballSkill.EnableMultishot();
                    
                    // 创建定时器在持续时间后关闭多重投射
                    var timer = new Timer();
                    timer.WaitTime = 3.0f; // 持续3秒
                    timer.OneShot = true;
                    timer.Timeout += () => {
                        fireballSkill.DisableMultishot();
                        timer.QueueFree();
                    };
                    source.AddChild(timer);
                    timer.Start();
                    
                    break;
                }
            }
        }
    }
} 