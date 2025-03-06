using Godot;
using System.Collections.Generic;
using Game.Skills.Base;
using Game.Enemies;

namespace Game.Skills.Active
{
	public partial class IceBolt : ProjectileSkill
	{
		private bool _isMultishot = false;
		private const float PROJECTILE_SPEED = 350f;
		private const float DAMAGE = 20f;
		private const float SLOW_FACTOR = 0.5f;  // 减速效果
		private const float SLOW_DURATION = 3.0f;  // 减速持续时间
		private const int MULTISHOT_COUNT = 3;
		private const float MULTISHOT_ANGLE = 15f;
		
		public override string Name { get; protected set; } = "寒冰弹";
		public override string Description { get; protected set; } = "发射一枚寒冰弹，对敌人造成伤害并减速";
		public override float Cooldown { get; protected set; } = 2.0f;
		public override float ManaCost { get; protected set; } = 15.0f;

		public override void Initialize()
		{
			base.Initialize();
			GD.Print($"寒冰弹技能初始化完成");
		}

		public override void EnableMultiProjectiles()
		{
			_isMultishot = true;
			GD.Print("寒冰弹: 启用多重投射模式");
		}

		public override void DisableMultiProjectiles()
		{
			_isMultishot = false;
			GD.Print("寒冰弹: 关闭多重投射模式");
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
			var projectile = new IceBoltProjectile();
			projectile.Speed = PROJECTILE_SPEED;
			projectile.Damage = DAMAGE;
			projectile.Direction = GetAimDirection(source);
			projectile.Source = source;
			projectile.GlobalPosition = source.GlobalPosition;
			
			source.GetTree().CurrentScene.AddChild(projectile);
			GD.Print($"寒冰弹发射: 位置={source.GlobalPosition}, 方向={projectile.Direction}");
		}

		private void CreateMultipleProjectiles(Node2D source)
		{
			Vector2 baseDirection = GetAimDirection(source);
			
			for (int i = 0; i < MULTISHOT_COUNT; i++)
			{
				float angle = (i - (MULTISHOT_COUNT - 1) / 2f) * Mathf.DegToRad(MULTISHOT_ANGLE);
				Vector2 direction = baseDirection.Rotated(angle);
				
				var projectile = new IceBoltProjectile();
				projectile.Speed = PROJECTILE_SPEED;
				projectile.Damage = DAMAGE;
				projectile.Direction = direction;
				projectile.Source = source;
				projectile.GlobalPosition = source.GlobalPosition;
				
				source.GetTree().CurrentScene.AddChild(projectile);
			}
			
			GD.Print($"多重寒冰弹发射: 位置={source.GlobalPosition}, 数量={MULTISHOT_COUNT}");
		}

		private partial class IceBoltProjectile : Area2D
		{
			[Export]
			public float Speed { get; set; } = 350.0f;
			
			[Export]
			public float Damage { get; set; } = 20.0f;
			
			public Vector2 Direction { get; set; } = Vector2.Right;
			public Node Source { get; set; }
			
			private float _lifetime = 5.0f;
			
			public override void _Ready()
			{
				CollisionLayer = 8;  // 第4层，用于投射物
				CollisionMask = 4;   // 第3层，用于检测敌人
				
				var shape = new CircleShape2D();
				shape.Radius = 8f;
				var collision = new CollisionShape2D();
				collision.Shape = shape;
				AddChild(collision);
				
				// 添加视觉效果 - 简化为基本形状
				var sprite = new ColorRect();
				sprite.Color = new Color(0.5f, 0.8f, 1.0f);  // 淡蓝色
				sprite.Size = new Vector2(16, 16);
				sprite.Position = new Vector2(-8, -8);
				AddChild(sprite);
				
				// 添加发光效果
				var glow = new ColorRect();
				glow.Color = new Color(0.7f, 0.9f, 1.0f, 0.3f);  // 淡蓝色发光
				glow.Size = new Vector2(24, 24);
				glow.Position = new Vector2(-12, -12);
				glow.ZIndex = -1;
				AddChild(glow);
				
				// 添加冰晶效果 - 使用简单的形状代替粒子
				for (int i = 0; i < 5; i++)
				{
					var iceShard = new ColorRect();
					iceShard.Color = new Color(0.8f, 0.95f, 1.0f, 0.7f);
					iceShard.Size = new Vector2(4, 4);
					float angle = (float)GD.RandRange(0, Mathf.Pi * 2);
					float distance = (float)GD.RandRange(5, 10);
					iceShard.Position = new Vector2(
						-2 + Mathf.Cos(angle) * distance,
						-2 + Mathf.Sin(angle) * distance
					);
					AddChild(iceShard);
					
					// 添加动画
					var tween = CreateTween();
					tween.SetLoops();
					tween.TweenProperty(iceShard, "position", 
						iceShard.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 3, 
						0.5f).SetTrans(Tween.TransitionType.Sine);
					tween.TweenProperty(iceShard, "position", 
						iceShard.Position, 
						0.5f).SetTrans(Tween.TransitionType.Sine);
				}
				
				BodyEntered += OnBodyEntered;
				AreaEntered += OnAreaEntered;
			}
			
			public override void _Process(double delta)
			{
				_lifetime -= (float)delta;
				if (_lifetime <= 0)
				{
					QueueFree();
					return;
				}
				
				Position += Direction * Speed * (float)delta;
			}
			
			private void OnBodyEntered(Node2D body)
			{
				if (body is Monster monster)
				{
					GD.Print($"寒冰弹击中怪物: {monster.Name}");
					monster.TakeDamage(Damage);
					
					// 应用减速效果
					ApplySlowEffect(monster);
					
					// 播放冰冻特效
					PlayFreezeEffect(monster.GlobalPosition);
					
					QueueFree();
				}
				else if (body is Enemy enemy)
				{
					GD.Print($"寒冰弹击中敌人: {enemy.Name}");
					enemy.TakeDamage(Damage);
					
					// 播放冰冻特效
					PlayFreezeEffect(enemy.GlobalPosition);
					
					QueueFree();
				}
			}
			
			private void OnAreaEntered(Area2D area)
			{
				// 处理与其他区域的碰撞
			}
			
			private void ApplySlowEffect(Monster monster)
			{
				// 这里应该调用怪物的减速方法
				// 由于Monster类可能没有直接的减速方法，这里只是示例
				// 实际实现时需要在Monster类中添加相应的方法
				GD.Print($"对怪物 {monster.Name} 施加减速效果: {SLOW_FACTOR * 100}%, 持续 {SLOW_DURATION} 秒");
				
				// 如果Monster有减速方法，可以这样调用:
				// monster.ApplySlowEffect(SLOW_FACTOR, SLOW_DURATION);
			}
			
			private void PlayFreezeEffect(Vector2 position)
			{
				// 创建冰冻特效
				var freezeEffect = new Node2D();
				Source.GetTree().CurrentScene.AddChild(freezeEffect);
				freezeEffect.GlobalPosition = position;
				
				// 添加冰冻光环
				var iceRing = new ColorRect();
				iceRing.Color = new Color(0.7f, 0.9f, 1.0f, 0.5f);
				iceRing.Size = new Vector2(40, 40);
				iceRing.Position = new Vector2(-20, -20);
				freezeEffect.AddChild(iceRing);
				
				// 添加冰晶碎片 - 使用简单的形状
				for (int i = 0; i < 12; i++)
				{
					var iceShard = new ColorRect();
					iceShard.Color = new Color(0.8f, 0.95f, 1.0f, 0.7f);
					iceShard.Size = new Vector2(6, 6);
					float angle = i * (Mathf.Pi * 2 / 12);
					float distance = 15;
					iceShard.Position = new Vector2(
						-3 + Mathf.Cos(angle) * distance,
						-3 + Mathf.Sin(angle) * distance
					);
					freezeEffect.AddChild(iceShard);
					
					// 添加飞散动画
					var tween = freezeEffect.CreateTween();
					tween.Parallel().TweenProperty(iceShard, "position", 
						iceShard.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 15, 
						0.5f).SetTrans(Tween.TransitionType.Cubic);
					tween.Parallel().TweenProperty(iceShard, "modulate:a", 0.0f, 0.5f);
				}
				
				// 创建消失动画
				var ringTween = freezeEffect.CreateTween();
				ringTween.TweenProperty(iceRing, "modulate:a", 0.0f, 0.5f);
				ringTween.TweenCallback(Callable.From(() => freezeEffect.QueueFree()));
			}
		}
	}
} 
