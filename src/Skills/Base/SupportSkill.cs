using Godot;

namespace Game.Skills.Base
{
    public abstract class SupportSkill : Skill
    {
        public override SkillTriggerType TriggerType => SkillTriggerType.Support;
        protected Skill[] LinkedSkills { get; set; } = new Skill[0];
        protected Node Source { get; set; }

        public override void Trigger(Node source)
        {
            Source = source;
        }
        
        public virtual void LinkSkill(Skill skill)
        {
            if (CanLinkSkill(skill))
            {
                var newLinkedSkills = new Skill[LinkedSkills.Length + 1];
                LinkedSkills.CopyTo(newLinkedSkills, 0);
                newLinkedSkills[LinkedSkills.Length] = skill;
                LinkedSkills = newLinkedSkills;
                GD.Print($"技能 {skill.Name} 已被 {Name} 辅助");
            }
        }
        
        protected virtual bool CanLinkSkill(Skill skill)
        {
            return skill.TriggerType == SkillTriggerType.Active;
        }
    }
} 