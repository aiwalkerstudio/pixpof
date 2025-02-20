using Godot;
using System.Collections.Generic;
using Game.Skills.Base;
using Game.Enemies;

namespace Game.Skills.Active
{
	public partial class Fireball : ProjectileSkill
	{
		private bool _isMultishot = false;
		private const float PROJECTILE_SPEED = 300f;
		private const float DAMAGE = 25f;
		private const int MULTISHOT_COUNT = 3;
		private const float MULTISHOT_ANGLE = 15f;
		private float _damage = 30.0f;  // 基础伤害
		private float _radius = 100.0f;  // 爆炸范围
		
		public override string Name { get; protected set; } = "火球术";

		private partial class FireballProjectile : Area2D
		{
			[Export]
			public float Speed { get; set; } = 300.0f;
			
			[Export]
			public float Damage { get; set; } = 25.0f;  // 确保这个值不是0
			
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
				
				// 添加视觉效果
				var sprite = new ColorRect();
				sprite.Color = new Color(1f, 0.2f, 0f);
				sprite.Size = new Vector2(16, 16);
				sprite.Position = new Vector2(-8, -8);
				AddChild(sprite);
				
				var glow = new ColorRect();
				glow.Color = new Color(1f, 0.5f, 0f, 0.3f);
				glow.Size = new Vector2(24, 24);
				glow.Position = new Vector2(-12, -12);
				glow.ZIndex = -1;
				AddChild(glow);
				
				BodyEntered += OnBodyEntered;
				AreaEntered += OnAreaEntered;
			}
			
			private void OnBodyEntered(Node2D body)
			{
				if (body is Monster monster)
				{
					GD.Print($"FireballProjectile hit monster at {monster.GlobalPosition}");
					monster.TakeDamage(Damage);
					CreateExplosionEffect();
					QueueFree();
				}
			}
			
			private void OnAreaEntered(Area2D area)
			{
				GD.Print($"FireballProjectile entered area: {area.Name} of type {area.GetType().Name}");
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
			
			private void CreateExplosionEffect()
			{
				var explosion = new Node2D();
				var circle = new ColorRect();
				circle.Color = new Color(1f, 0.5f, 0f, 0.5f);
				circle.Size = new Vector2(32, 32);
				circle.Position = -circle.Size / 2;
				
				explosion.AddChild(circle);
				explosion.GlobalPosition = GlobalPosition;
				GetTree().CurrentScene.AddChild(explosion);
				
				var tween = explosion.CreateTween();
				tween.TweenProperty(circle, "size", circle.Size * 2, 0.2f);
				tween.Parallel().TweenProperty(circle, "modulate:a", 0.0f, 0.2f);
				tween.TweenCallback(Callable.From(() => explosion.QueueFree()));
			}
		}

		public override void Initialize()
		{
			base.Initialize();
			Cooldown = 1.0f;
			ManaCost = 20f;
			Description = "发射一颗火球，对敌人造成火焰伤害";
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
			var projectile = CreateFireballProjectile(source);
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
				var projectile = CreateFireballProjectile(source);
				float angle = startAngle + MULTISHOT_ANGLE * i;
				projectile.Direction = baseDirection.Rotated(Mathf.DegToRad(angle));
				source.GetTree().CurrentScene.AddChild(projectile);
				projectile.GlobalPosition = source.GlobalPosition;
			}
		}

		private FireballProjectile CreateFireballProjectile(Node2D source)
		{
			var projectile = new FireballProjectile
			{
				Speed = PROJECTILE_SPEED,
				Damage = DAMAGE,  // 确保这里的DAMAGE值正确
				Source = source
			};
			GD.Print($"Created FireballProjectile with damage: {projectile.Damage}"); // 添加这行调试信息
			return projectile;
		}

		public override void EnableMultiProjectiles()
		{
			_isMultishot = true;
			GD.Print("火球术: 启用多重投射模式");
		}

		public override void DisableMultiProjectiles()
		{
			_isMultishot = false;
			GD.Print("火球术: 关闭多重投射模式");
		}

		protected override void OnTrigger(Node source)
		{
			base.OnTrigger(source);  // 调用基类方法
			
			if (source is Game.Player player)
			{
				// 获取鼠标位置作为目标点
				var mousePos = player.GetGlobalMousePosition();
				GD.Print($"Casting Fireball at {mousePos}");

				// 检测范围内的敌人
				var spaceState = player.GetWorld2D().DirectSpaceState;
				var query = new PhysicsShapeQueryParameters2D();
				var shape = new CircleShape2D();
				shape.Radius = _radius;
				query.Shape = shape;
				query.Transform = new Transform2D(0, mousePos);
				query.CollisionMask = 4;  // 敌人的碰撞层

				var results = spaceState.IntersectShape(query);
				foreach (var result in results)
				{
					var collider = result["collider"].As<Node2D>();
					if (collider is Enemy enemy)
					{
						GD.Print($"Fireball hits enemy at {enemy.GlobalPosition}");
						enemy.TakeDamage(_damage);
					}
				}

				// 播放特效
				PlayFireballEffect(player.GlobalPosition, mousePos);
			}
		}

		private void PlayFireballEffect(Vector2 start, Vector2 end)
		{
			// TODO: 添加火球飞行和爆炸特效
			var tween = Source.CreateTween();
			var effect = new ColorRect();
			effect.Color = new Color(1, 0.5f, 0);  // 橙色
			effect.Size = new Vector2(20, 20);
			effect.Position = start - effect.Size / 2;
			Source.GetTree().CurrentScene.AddChild(effect);

			tween.TweenProperty(effect, "position", end - effect.Size / 2, 0.3f);
			tween.TweenCallback(Callable.From(() => {
				effect.QueueFree();
				PlayExplosionEffect(end);
			}));
		}

		private void PlayExplosionEffect(Vector2 position)
		{
			var explosion = new ColorRect();
			explosion.Color = new Color(1, 0.3f, 0, 0.5f);
			explosion.Size = new Vector2(_radius * 2, _radius * 2);
			explosion.Position = position - explosion.Size / 2;
			Source.GetTree().CurrentScene.AddChild(explosion);

			var tween = Source.CreateTween();
			tween.TweenProperty(explosion, "modulate:a", 0.0f, 0.3f);
			tween.TweenCallback(Callable.From(() => explosion.QueueFree()));
		}
	}
} 
