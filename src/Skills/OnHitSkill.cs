using Godot;

public partial class OnHitSkill : Skill
{
	[Export]
	public float DamageRadius { get; set; } = 100.0f;
	
	[Export]
	public float Damage { get; set; } = 15.0f;

	public OnHitSkill()
	{
		Name = "受伤反击";
		Description = "受到伤害时,对周围敌人造成伤害";
		Cooldown = 3.0f;
		IsPassive = true;
		TriggerType = SkillTriggerType.OnHit;
	}

	public override void Trigger(Node source)
	{
		if (source is Player player)
		{
			GD.Print($"触发{Name}!");
			CurrentCooldown = Cooldown;

			// 创建伤害区域
			var space = player.GetWorld2D().DirectSpaceState;
			var shape = new CircleShape2D();
			shape.Radius = DamageRadius;

			var query = new PhysicsShapeQueryParameters2D
			{
				Shape = shape,
				Transform = new Transform2D(0, player.GlobalPosition),
				CollisionMask = 4 // 怪物层
			};
			
			var results = space.IntersectShape(query);
			
			foreach (var result in results)
			{
				if (result["collider"].As<Node2D>() is Monster monster)
				{
					monster.TakeDamage(Damage);
				}
			}

			// 播放技能特效
			PlaySkillEffect(player);
		}
	}

	private void PlaySkillEffect(Node2D source)
	{
		// 创建简单的视觉效果
		var effect = new Node2D();
		var circle = new ColorRect();
		circle.Color = new Color(1, 1, 0, 0.3f);
		circle.Size = new Vector2(DamageRadius * 2, DamageRadius * 2);
		circle.Position = -circle.Size / 2;
		
		effect.AddChild(circle);
		effect.GlobalPosition = source.GlobalPosition;
		
		// 添加到场景
		source.GetTree().Root.AddChild(effect);
		
		// 创建消失动画
		var tween = effect.CreateTween();
		tween.TweenProperty(circle, "modulate:a", 0.0f, 0.5f);
		tween.TweenCallback(Callable.From(() => effect.QueueFree()));
	}
} 
