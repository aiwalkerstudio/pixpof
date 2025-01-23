using Godot;
using Game.Skills.Base;

namespace Game.Skills.Support
{
    public class CastOnDamageTakenSupport : SupportSkill
    {
        public override string Name { get; protected set; } = "受伤时施放";
        private float _damageAccumulated = 0;
        private const float DAMAGE_THRESHOLD = 10f;

        public override void Initialize()
        {
            base.Initialize();
            Cooldown = 0.001f; // 触发冷却
        }

        public override void OnDamageTaken(float damage)
        {
            _damageAccumulated += damage;
            GD.Print($"{Name}: 累积伤害 {_damageAccumulated}/{DAMAGE_THRESHOLD}  {_damageAccumulated >= DAMAGE_THRESHOLD} {CanTrigger()}");
            

            if (_damageAccumulated >= DAMAGE_THRESHOLD && CanTrigger())
            {
              GD.Print($" LinkedSkills: {LinkedSkills}");
                // 触发所有链接的技能
                foreach (var skill in LinkedSkills)
                {
                    GD.Print($"{Name} 准备触发技能: {skill.Name}  {skill.CanTrigger()}");
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