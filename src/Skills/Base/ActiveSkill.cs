using Godot;
using System.Collections.Generic;

namespace Game.Skills.Base
{
    public abstract class ActiveSkill : Skill
    {
        public override SkillTriggerType TriggerType => SkillTriggerType.Active;
        public virtual bool HasReservation { get; protected set; } = false;
        public virtual bool IsChanneling { get; protected set; } = false;
        
        protected List<SupportSkill> SupportSkills { get; } = new();
        
        public virtual void AddSupport(SupportSkill support)
        {
            if (!SupportSkills.Contains(support))
            {
                SupportSkills.Add(support);
                support.LinkSkill(this);
                GD.Print($"技能 {Name} 添加辅助: {support.Name}");
            }
        }
    }
} 