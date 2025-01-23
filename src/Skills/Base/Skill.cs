using Godot;
using System;

namespace Game.Skills
{
    public abstract class Skill
    {
        public abstract string Name { get; protected set; }
        public virtual string Description { get; protected set; } = "";
        public virtual float Cooldown { get; protected set; } = 0f;
        public float CurrentCooldown { get; protected set; } = 0f;
        public virtual bool IsPassive { get; protected set; } = false;
        public abstract SkillTriggerType TriggerType { get; }
        public virtual bool HasReservation { get; protected set; } = false;
        public virtual bool IsChanneling { get; protected set; } = false;
        protected SceneTree SceneTree { get; private set; }

        public virtual void Initialize()
        {
            SceneTree = Engine.GetMainLoop() as SceneTree;
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