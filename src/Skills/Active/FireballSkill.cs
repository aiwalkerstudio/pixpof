using Godot;
using System.Collections.Generic;
using Game.Skills.Base;
using Game.Enemies;

namespace Game.Skills.Active
{
	public partial class FireballSkill : ProjectileSkill
	{
		private bool _isMultishot = false;
		private const float PROJECTILE_SPEED = 300f;
		private const float DAMAGE = 25f;
		private const int MULTISHOT_COUNT = 3;
		private const float MULTISHOT_ANGLE = 15f;
		
		public override string Name { get; protected set; } = "火球术";

		private partial class Fireball : Area2D
		{
			[Export]
			public float Speed { get; set; } = 300.0f;
			
			[Export]
			public float Damage { get; set; } = 25.0f;
			
			public Vector2 Direction { get; set; } = Vector2.Right;
			public Node Source { get; set; }
			
			private float _lifetime = 5.0f;
			
			public override void _Ready()
			{
				CollisionLayer = 4;
				CollisionMask = 2;
				
				var shape = new CircleShape2D();
				shape.Radius = 8f;
				var collision = new CollisionShape2D();
				collision.Shape = shape;
				AddChild(collision);
				
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
					monster.TakeDamage(Damage);
					CreateExplosionEffect();
					QueueFree();
				}
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

		private Fireball CreateFireballProjectile(Node2D source)
		{
			var projectile = new Fireball
			{
				Speed = PROJECTILE_SPEED,
				Damage = DAMAGE,
				Source = source
			};
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
	}
} 
