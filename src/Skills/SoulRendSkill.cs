using Godot;

public class SoulRendSkill : ProjectileSkill 
{
    public override string Name => "裂魂术";
    public override SkillTriggerType TriggerType => SkillTriggerType.Active;
    
    private const float PROJECTILE_SPEED = 300f;
    private const float DAMAGE = 20f;
    private const float DOT_DAMAGE = 10f; // 持续伤害
    private const float LIFE_LEECH_PERCENT = 0.2f; // 20%吸血
    private const float TRACKING_RANGE = 100f; // 追踪范围
    private const float AREA_DAMAGE_RADIUS = 50f; // 范围伤害半径
    
    public override void Initialize()
    {
        base.Initialize();
        Cooldown = 1.0f; // 1秒冷却
    }

    protected override void CreateProjectile(Node source)
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
        
        // 设置初始位置和方向
        projectile.GlobalPosition = source.GlobalPosition;
        projectile.Direction = GetAimDirection(source);
        
        // 添加到场景
        source.GetTree().Root.AddChild(projectile);
    }
} 