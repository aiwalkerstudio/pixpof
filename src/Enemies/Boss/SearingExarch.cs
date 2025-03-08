using Godot;
using Game.Enemies;

namespace Game.Enemies.Boss
{
	public partial class SearingExarch : Enemy
	{
		private string _bossEmoji = "👹"; // 恶魔形象
		private Label _label;
		
		[Signal]
		public delegate void BossVoiceLineEventHandler(string line);

		private enum State
		{
			Idle,
			Chase,
			Attack,
			Cast
		}

		private State currentState = State.Idle;
		private float moveSpeed = 80f;
		private float health = 1200f;
		private bool isChanneling = false;

		// 技能冷却
		private float fireballCooldown = 2.0f;
		private float novaCooldown = 5.0f;
		private float shieldCooldown = 10.0f;
		private float lastFireballTime = 0f;
		private float lastNovaTime = 0f;
		private float lastShieldTime = 0f;

		private Node2D player;
		private Timer castTimer;
		private Node2D fireShieldEffect;

		public override void _Ready()
		{
			// 设置emoji显示
			_label = new Label();
			_label.Text = _bossEmoji;
			_label.HorizontalAlignment = HorizontalAlignment.Center;
			_label.VerticalAlignment = VerticalAlignment.Center;
			_label.Scale = new Vector2(5, 5); // 放大emoji
			AddChild(_label);

			player = GetTree().GetFirstNodeInGroup("player") as Node2D;
			fireShieldEffect = GetNode<Node2D>("FireShieldEffect");
			fireShieldEffect.Hide();

			// 初始化施法计时器
			castTimer = new Timer();
			AddChild(castTimer);
			castTimer.OneShot = true;
			castTimer.Connect("timeout", new Callable(this, nameof(OnCastComplete)));
		}

		public override void _PhysicsProcess(double delta)
		{
			if(isChanneling) return;

			switch(currentState)
			{
				case State.Idle:
					if(player != null)
					{
						currentState = State.Chase;
					}
					break;

				case State.Chase:
					ChasePlayer(delta);
					TryUseSkills();
					break;

				case State.Attack:
					// 基础攻击逻辑
					break;

				case State.Cast:
					// 施法状态
					break;
			}
		}

		private void ChasePlayer(double delta)
		{
			if(player == null) return;

			Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
			Velocity = direction * moveSpeed;
			MoveAndSlide();
		}

		private void TryUseSkills()
		{
			float currentTime = Time.GetTicksMsec() / 1000.0f;

			// 尝试使用火球
			if(currentTime - lastFireballTime >= fireballCooldown)
			{
				CastFireball();
				lastFireballTime = currentTime;
			}
			// 尝试使用火焰新星
			else if(currentTime - lastNovaTime >= novaCooldown)
			{
				CastFireNova();
				lastNovaTime = currentTime;
			}
			// 尝试使用火焰护盾
			else if(currentTime - lastShieldTime >= shieldCooldown)
			{
				ActivateShield();
				lastShieldTime = currentTime;
			}
		}

		private void CastFireball()
		{
			if(player == null) return;

			EmitSignal(SignalName.BossVoiceLine, "Burn!");
			isChanneling = true;
			castTimer.Start(0.5); // 0.5秒施法时间

			// 创建火球
			var fireball = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/Fireball.tscn").Instantiate<Node2D>();
			AddChild(fireball);
			fireball.GlobalPosition = GlobalPosition;
			fireball.LookAt(player.GlobalPosition);
		}

		private void CastFireNova()
		{
			EmitSignal(SignalName.BossVoiceLine, "Feel my wrath!");
			isChanneling = true;
			castTimer.Start(0.8); // 0.8秒施法时间

			// 创建火焰新星效果
			var novaParticles = GetNode<GpuParticles2D>("NovaParticles");
			novaParticles.Emitting = true;

			// 对范围内的目标造成伤害
			var spaceState = GetWorld2D().DirectSpaceState;
			var query = new CircleShape2D();
			query.Radius = 200;
			
			var parameters = new PhysicsShapeQueryParameters2D();
			parameters.Shape = query;
			parameters.CollisionMask = 1; // 玩家层
			parameters.Transform = new Transform2D(0, GlobalPosition);
			
			var results = spaceState.IntersectShape(parameters);
			foreach(var result in results)
			{
				var collider = result["collider"].As<Node2D>();
				if(collider != null && collider.HasMethod("TakeDamage"))
				{
					collider.Call("TakeDamage", 50f);
				}
			}
		}

		private void ActivateShield()
		{
			EmitSignal(SignalName.BossVoiceLine, "You cannot harm me!");
			isChanneling = true;
			castTimer.Start(0.5); // 0.5秒施法时间

			// 激活护盾效果
			fireShieldEffect.Show();
			var particles = fireShieldEffect.GetNode<GpuParticles2D>("FireParticles");
			particles.Emitting = true;

			// 设置护盾持续时间
			var shieldTimer = new Timer();
			AddChild(shieldTimer);
			shieldTimer.WaitTime = 3.0f;
			shieldTimer.OneShot = true;
			shieldTimer.Connect("timeout", new Callable(this, nameof(OnShieldExpired)));
			shieldTimer.Start();
		}

		private void OnCastComplete()
		{
			isChanneling = false;
			currentState = State.Chase;
		}

		private void OnShieldExpired()
		{
			fireShieldEffect.Hide();
		}

		public override void TakeDamage(float damage)
		{
			if(fireShieldEffect.Visible) return; // 护盾激活时免疫伤害

			health -= damage;
			if(health <= 0)
			{
				QueueFree();
			}
		}
	}
}
