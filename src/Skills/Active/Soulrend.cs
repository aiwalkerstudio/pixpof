using Godot;
using System.Collections.Generic;
using Game.Skills.Base;
using Game.Enemies;
using Game;

namespace Game.Skills.Active
{
	public partial class Soulrend : ProjectileSkill
	{
		private bool _isMultishot = false;
		private const float PROJECTILE_SPEED = 300f;
		private const float DAMAGE = 20f;
		private const float DOT_DAMAGE = 10f;
		private const float LIFE_LEECH_PERCENT = 0.2f;
		private const float TRACKING_RANGE = 100f;
		private const float AREA_DAMAGE_RADIUS = 50f;
		private const int MULTISHOT_COUNT = 3;
		private const float MULTISHOT_ANGLE = 15f;
		private float _damage = 25.0f;  // 基础伤害
		private float _dotDuration = 3.0f;  // 持续时间
		private float _range = 150.0f;  // 攻击范围

		public override string Name { get; protected set; } = "裂魂术";

		private partial class SoulRendProjectile : Area2D
		{
			[Export]
			public float Speed { get; set; } = 300.0f;
			
			[Export]
			public float Damage { get; set; } = 20.0f;
			
			[Export]
			public float DotDamage { get; set; } = 10.0f;
			
			[Export]
			public float LifeLeechPercent { get; set; } = 0.2f;
			
			[Export]
			public float TrackingRange { get; set; } = 100.0f;
			
			[Export]
			public float AreaDamageRadius { get; set; } = 50.0f;
			
			public Vector2 Direction { get; set; } = Vector2.Right;
			public Node Source { get; set; }
			
			private float _lifetime = 5.0f;
			private HashSet<Node2D> _hitTargets = new();
			
			public override void _Ready()
			{
				CollisionLayer = 4;
				CollisionMask = 2;
				
				var shape = new CircleShape2D();
				shape.Radius = 12f;
				var collision = new CollisionShape2D();
				collision.Shape = shape;
				AddChild(collision);
				
				// 添加外发光效果
				var outerGlow = new ColorRect();
				outerGlow.Color = new Color(1f, 0.4f, 1f, 0.2f);
				outerGlow.Size = new Vector2(48, 48);
				outerGlow.Position = new Vector2(-24, -24);
				AddChild(outerGlow);
				
				// 添加内发光效果
				var innerGlow = new ColorRect();
				innerGlow.Color = new Color(0.9f, 0.3f, 0.9f, 0.4f);
				innerGlow.Size = new Vector2(36, 36);
				innerGlow.Position = new Vector2(-18, -18);
				AddChild(innerGlow);
				
				// 添加核心
				var core = new ColorRect();
				core.Color = new Color(1f, 0.8f, 1f, 1.0f);
				core.Size = new Vector2(24, 24);
				core.Position = new Vector2(-12, -12);
				AddChild(core);
				
				// 添加中心点
				var center = new ColorRect();
				center.Color = new Color(1f, 0.2f, 1f, 1.0f);
				center.Size = new Vector2(12, 12);
				center.Position = new Vector2(-6, -6);
				AddChild(center);
				
				BodyEntered += OnBodyEntered;
			}
			
			public override void _Process(double delta)
			{
				_lifetime -= (float)delta;
				if (_lifetime <= 0)
				{
					QueueFree();
					return;
				}
				
				// 追踪逻辑
				var nearestEnemy = FindNearestEnemy();
				if (nearestEnemy != null)
				{
					var targetDir = (nearestEnemy.GlobalPosition - GlobalPosition).Normalized();
					Direction = Direction.Lerp(targetDir, 0.1f);
				}
				
				// 移动
				Position += Direction * Speed * (float)delta;
				
				// 范围伤害
				ApplyAreaDamage();
			}
			
			private void OnBodyEntered(Node2D body)
			{
				if (body is Monster monster && !_hitTargets.Contains(body))
				{
					_hitTargets.Add(body);
					
					// 直接伤害
					monster.TakeDamage(Damage);
					
					// 持续伤害
					monster.ApplyDotDamage(DotDamage, 3.0f);
					
					// 吸血效果
					if (Source is Game.Player player)
					{
						player.AddEnergyShield(Damage * LifeLeechPercent);
					}
				}
			}
			
			private Node2D FindNearestEnemy()
			{
				Node2D nearest = null;
				float nearestDist = TrackingRange;
				
				foreach (var body in GetOverlappingBodies())
				{
					if (body is Monster monster && !_hitTargets.Contains(monster))
					{
						float dist = GlobalPosition.DistanceTo(monster.GlobalPosition);
						if (dist < nearestDist)
						{
							nearest = monster;
							nearestDist = dist;
						}
					}
				}
				
				return nearest;
			}
			
			private void ApplyAreaDamage()
			{
				foreach (var body in GetOverlappingBodies())
				{
					if (body is Monster monster && !_hitTargets.Contains(monster))
					{
						float dist = GlobalPosition.DistanceTo(monster.GlobalPosition);
						if (dist <= AreaDamageRadius)
						{
							monster.TakeDamage(DotDamage * (float)GetProcessDeltaTime());
						}
					}
				}
			}
		}

		public override void Initialize()
		{
			base.Initialize();
			Cooldown = 2.0f;  // 冷却时间
			Description = "发射追踪灵魂投射物，造成伤害和持续伤害";
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
			source.GetTree().CurrentScene.AddChild(projectile);
			projectile.GlobalPosition = source.GlobalPosition;
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
				source.GetTree().CurrentScene.AddChild(projectile);
				projectile.GlobalPosition = source.GlobalPosition;
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
			return projectile;
		}

		public override void EnableMultiProjectiles()
		{
			_isMultishot = true;
			GD.Print("裂魂术: 启用多重投射模式");
		}

		public override void DisableMultiProjectiles()
		{
			_isMultishot = false;
			GD.Print("裂魂术: 关闭多重投射模式");
		}

		protected override void OnTrigger(Node source)
		{
			base.OnTrigger(source);  // 调用基类方法
			
			if (source is Game.Player player)
			{
				// 获取鼠标位置作为目标点
				var mousePos = player.GetGlobalMousePosition();
				GD.Print($"Casting Soulrend at {mousePos}");

				// 检测范围内的敌人
				var spaceState = player.GetWorld2D().DirectSpaceState;
				var query = new PhysicsShapeQueryParameters2D();
				var shape = new CircleShape2D();
				shape.Radius = _range;
				query.Shape = shape;
				query.Transform = new Transform2D(0, mousePos);
				query.CollisionMask = 4;  // 敌人的碰撞层

				var results = spaceState.IntersectShape(query);
				foreach (var result in results)
				{
					var collider = result["collider"].As<Node2D>();
					if (collider is Monster monster)  // 注意这里用Monster而不是Enemy
					{
						GD.Print($"Soulrend hits monster at {monster.GlobalPosition}");
						monster.TakeDamage(_damage);  // 直接伤害
						monster.ApplyDotDamage(DOT_DAMAGE, _dotDuration);  // 持续伤害
					}
				}

				// 播放特效
				PlaySoulrendEffect(mousePos);
			}
		}

		private void PlaySoulrendEffect(Vector2 position)
		{
			var effect = new ColorRect();
			effect.Color = new Color(0.5f, 0, 1, 0.7f);  // 紫色
			effect.Size = new Vector2(_range * 2, _range * 2);
			effect.Position = position - effect.Size / 2;
			Source.GetTree().CurrentScene.AddChild(effect);

			var tween = Source.CreateTween();
			tween.TweenProperty(effect, "modulate:a", 0.0f, 0.5f);
			tween.TweenCallback(Callable.From(() => effect.QueueFree()));
		}
	}
} 
