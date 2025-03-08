using Godot;
using Game.Enemies;

namespace Game.Enemies.Boss
{
	public partial class EaterOfWorlds : Enemy
	{
		private string _bossEmoji = "👾"; // 外星怪物形象
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
		private float moveSpeed = 100f;
		private float health = 1000f;
		private bool isChanneling = false;

		// 技能冷却
		private float lightningBeamCooldown = 3.0f;
		private float sphereSlamCooldown = 5.0f;
		private float lastBeamTime = 0f;
		private float lastSlamTime = 0f;

		private Node2D player;
		private Timer castTimer;

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

			// 尝试使用闪电光束
			if(currentTime - lastBeamTime >= lightningBeamCooldown)
			{
				CastLightningBeam();
				lastBeamTime = currentTime;
			}
			// 尝试使用球体猛击
			else if(currentTime - lastSlamTime >= sphereSlamCooldown)
			{
				CastSphereSlam();
				lastSlamTime = currentTime;
			}
		}

		private void CastLightningBeam()
		{
			if(player == null) return;

			EmitSignal(SignalName.BossVoiceLine, "Perish!");
			isChanneling = true;
			castTimer.Start(1.0); // 1秒施法时间

			// 创建闪电光束效果
			var beam = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/LightningBeam.tscn").Instantiate<Node2D>();
			AddChild(beam);
			beam.GlobalPosition = GlobalPosition;
			beam.LookAt(player.GlobalPosition);
		}

		private void CastSphereSlam()
		{
			if(player == null) return;

			EmitSignal(SignalName.BossVoiceLine, "Hunger!");
			isChanneling = true;
			castTimer.Start(0.5); // 0.5秒施法时间

			// 创建球体猛击效果
			var sphere = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/SphereSlam.tscn").Instantiate<Node2D>();
			AddChild(sphere);
			sphere.GlobalPosition = GlobalPosition;
		}

		private void OnCastComplete()
		{
			isChanneling = false;
			currentState = State.Chase;
		}

		public override void TakeDamage(float damage)
		{
			health -= damage;
			if(health <= 0)
			{
				QueueFree();
			}
		}
	}
}
