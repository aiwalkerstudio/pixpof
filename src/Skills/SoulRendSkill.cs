using Godot;
using Game.Skills;

namespace Game.Skills
{
    public class SoulRendSkill : ProjectileSkill
    {
        public override string Name { get; protected set; } = "裂魂术";
        public override SkillTriggerType TriggerType { get; protected set; } = SkillTriggerType.Active;
        
        private const float PROJECTILE_SPEED = 300f;
        private const float DAMAGE = 20f;
        private const float DOT_DAMAGE = 10f; // 持续伤害
        private const float LIFE_LEECH_PERCENT = 0.2f; // 20%吸血
        private const float TRACKING_RANGE = 100f; // 追踪范围
        private const float AREA_DAMAGE_RADIUS = 50f; // 范围伤害半径
        
        private bool _isMultishot = false;
        private const int MULTISHOT_COUNT = 3;
        private const float MULTISHOT_ANGLE = 15f;

        public override void Initialize()
        {
            base.Initialize();
            Cooldown = 1.0f; // 1秒冷却
        }

        protected override void CreateProjectile(Node2D source)
        {
            if (_isMultishot)
            {
                CreateMultipleProjectiles(source);
            }
            else
            {
                CreateSingleProjectile(source);
            }
        }

        private void CreateSingleProjectile(Node2D source)
        {
            var projectile = CreateSoulRendProjectile(source);
            projectile.Direction = GetAimDirection(source);
            source.GetTree().Root.AddChild(projectile);
        }

        private void CreateMultipleProjectiles(Node2D source)
        {
            float startAngle = -MULTISHOT_ANGLE * (MULTISHOT_COUNT - 1) / 2;
            Vector2 baseDirection = GetAimDirection(source);

            for (int i = 0; i < MULTISHOT_COUNT; i++)
            {
                var projectile = CreateSoulRendProjectile(source);
                float angle = startAngle + MULTISHOT_ANGLE * i;
                projectile.Direction = baseDirection.Rotated(Mathf.DegToRad(angle));
                source.GetTree().Root.AddChild(projectile);
            }
        }

        private SoulRendProjectile CreateSoulRendProjectile(Node2D source)
        {
            var projectile = new SoulRendProjectile
            {
                Speed = PROJECTILE_SPEED,
                Damage = DAMAGE,
                DotDamage = DOT_DAMAGE,
                LifeLeechPercent = LIFE_LEECH_PERCENT,
                TrackingRange = TRACKING_RANGE,
                AreaDamageRadius = AREA_DAMAGE_RADIUS,
                Source = source
            };
            
            projectile.GlobalPosition = source.GlobalPosition;
            return projectile;
        }

        public void EnableMultishot()
        {
            _isMultishot = true;
            GD.Print("裂魂术: 启用多重投射模式");
        }

        public void DisableMultishot()
        {
            _isMultishot = false;
            GD.Print("裂魂术: 关闭多重投射模式");
        }
    }
} 