using Godot;
using Game.Skills.Base;

namespace Game.Skills.Support
{
    public class CastOnDamageTakenSupport : SupportSkill
    {
        public override string Name { get; protected set; } = "受伤时施放";
        private float _damageAccumulated = 0;
        private const float DAMAGE_THRESHOLD = 100f;

        public override void Initialize()
        {
            base.Initialize();
            Cooldown = 0.5f; // 触发冷却
        }

        public override void OnDamageTaken(float damage)
        {
            _damageAccumulated += damage;
            GD.Print($"{Name}: 累积伤害 {_damageAccumulated}/{DAMAGE_THRESHOLD}");
            
            if (_damageAccumulated >= DAMAGE_THRESHOLD && CanTrigger())
            {
                foreach (var skill in LinkedSkills)
                {
                    if (skill.CanTrigger())
                    {
                        GD.Print($"{Name} 触发技能: {skill.Name}");
                        skill.Trigger(Source);
                    }
                }
                _damageAccumulated = 0;
                StartCooldown();
            }
        }

        protected override bool CanLinkSkill(Skill skill)
        {
            // 只能辅助主动技能，不能辅助图腾、陷阱等
            return skill.TriggerType == SkillTriggerType.Active 
                && !skill.HasReservation 
                && !skill.IsChanneling;
        }
    }
} 