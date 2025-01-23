using Godot;
using System;

public abstract class Skill
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public float Cooldown { get; protected set; }
    public float CurrentCooldown { get; protected set; }
    public bool IsPassive { get; protected set; }
    public SkillTriggerType TriggerType { get; protected set; }

    public virtual void Initialize()
    {
        CurrentCooldown = 0;
    }

    public virtual bool CanTrigger()
    {
        return CurrentCooldown <= 0;
    }

    public abstract void Trigger(Node source);

    public virtual void Update(float delta)
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= delta;
        }
    }
}

public enum SkillTriggerType
{
    OnHit,
    Manual,
    Passive
} 