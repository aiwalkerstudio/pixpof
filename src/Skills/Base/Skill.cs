using Godot;
using System;

namespace Game.Skills
{
    public abstract class Skill
    {
        public abstract string Name { get; protected set; }
        public virtual string Description { get; protected set; }
        public float Cooldown { get; protected set; }
        public float CurrentCooldown { get; protected set; }
        public bool IsPassive { get; protected set; }
        public abstract SkillTriggerType TriggerType { get; }
        public virtual bool HasReservation { get; protected set; } = false;
        public virtual bool IsChanneling { get; protected set; } = false;

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

        protected void StartCooldown()
        {
            CurrentCooldown = Cooldown;
        }

        public virtual void OnDamageTaken(float damage) { }
    }
} 