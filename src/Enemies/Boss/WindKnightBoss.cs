using Godot;
using System;
using System.Collections.Generic;
using Game.Enemies;

namespace Game.Enemies.Boss
{
	public partial class WindKnightBoss : Enemy
	{
		[Signal]
		public delegate void BossDefeatedEventHandler();

		// 添加Unicode表情显示
		private Label _emojiLabel;
		private string _bossEmoji = "🌪️"; // 风之涡苏表情
		private float _animationTime = 0f;
		
		// 风骑士特有的状态
		private enum BossState
		{
			Idle,
			Chase,           // 追击玩家
			Slashing,        // 向前劈砍
			Invisible,       // 隐身状态
			ShockwaveAttack, // 霸体冲击波
			Dead             // 死亡状态
		}
		
		// 基本属性
		[Export]
		public float NormalMoveSpeed { get; set; } = 180.0f; // 常态移动速度
		
		[Export]
		public float LowHealthMoveSpeed { get; set; } = 250.0f; // 低血量时的移动速度
		
		[Export]
		public float AttackRange { get; set; } = 80.0f; // 攻击范围
		
		[Export]
		public float DetectionRange { get; set; } = 300.0f; // 检测范围
		
		[Export]
		public float InvisibilityCooldown { get; set; } = 8.0f; // 隐身冷却时间
		
		[Export]
		public float InvisibilityDuration { get; set; } = 5.0f; // 隐身持续时间
		
		[Export]
		public float AttackCooldown { get; set; } = 1.5f; // 攻击冷却
		
		[Export]
		public float ShockwaveDamage { get; set; } = 20.0f; // 冲击波伤害
		
		[Export]
		public float ShockwaveRadius { get; set; } = 120.0f; // 冲击波半径
		
		[Export]
		public float LowHealthThreshold { get; set; } = 0.3f; // 低血量阈值（最大生命值的百分比）
		
		[Export]
		public float ForceVisibleDuration { get; set; } = 2.0f; // 被迫现身持续时间
		
		// 内部状态变量
		private BossState _currentState = BossState.Idle;
		private Game.Player _target;
		private float _stateTimer = 0f;
		private float _invisibilityTimer = 0f;
		private float _attackTimer = 0f;
		private float _lastHitTime = 0f;
		private float _continuousAttackTimer = 0f;
		private Vector2 _slashDirection = Vector2.Zero;
		private bool _isLowHealth = false;
		private CPUParticles2D _dustParticles;
		
		// 用于跟踪玩家攻击的计时器
		private float _timeSinceLastPlayerAttack = 0f;
		private const float FORCE_VISIBLE_ATTACK_INTERVAL = 5.0f; // 如果5秒内没有受到攻击，可以重新隐身
		
		public override void _Ready()
		{
			base._Ready();
			MaxHealth = 500.0f;
			AttackDamage = 25.0f;
			CurrentHealth = MaxHealth;
			
			// 设置碰撞
			CollisionLayer = 4;  // 敌人层
			CollisionMask = 3;   // 与玩家(1)和墙(2)碰撞
			
			// 创建显示Unicode表情的Label
			SetupEmojiDisplay();
			
			// 创建尘土粒子效果（用于隐身时的视觉提示）
			SetupDustParticles();
		}
		
		private void SetupEmojiDisplay()
		{
			_emojiLabel = new Label();
			_emojiLabel.Text = _bossEmoji;
			_emojiLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_emojiLabel.VerticalAlignment = VerticalAlignment.Center;
			
			// 设置字体大小和颜色
			_emojiLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 1.0f)); // 淡蓝色风元素
			_emojiLabel.AddThemeFontSizeOverride("font_size", 48);
			
			// 调整位置，使其与碰撞形状居中对齐
			_emojiLabel.Position = new Vector2(-24, -24);
			
			AddChild(_emojiLabel);
		}
		
		private void SetupDustParticles()
		{
			_dustParticles = new CPUParticles2D();
			_dustParticles.Emitting = false;
			_dustParticles.Amount = 20;
			_dustParticles.Lifetime = 0.8f;
			_dustParticles.OneShot = false;
			_dustParticles.Explosiveness = 0.0f;
			_dustParticles.Direction = Vector2.Up;
			_dustParticles.Spread = 180.0f;
			_dustParticles.Gravity = new Vector2(0, -20);
			_dustParticles.InitialVelocity = 30.0f;
			_dustParticles.Scale = 2.0f;
			
			// 设置粒子颜色
			var gradient = new Gradient();
			gradient.Colors = new Color[] { new Color(0.8f, 0.8f, 0.8f, 0.8f), new Color(0.9f, 0.9f, 0.9f, 0.0f) };
			_dustParticles.ColorRamp = gradient;
			
			AddChild(_dustParticles);
		}
		
		public override void _PhysicsProcess(double delta)
		{
			float deltaF = (float)delta;
			
			// 更新计时器
			_stateTimer += deltaF;
			_invisibilityTimer += deltaF;
			_attackTimer += deltaF;
			_timeSinceLastPlayerAttack += deltaF;
			
			// 检查是否处于低血量状态
			_isLowHealth = CurrentHealth < MaxHealth * LowHealthThreshold;
			
			// 获取目标（玩家）
			if (_target == null)
			{
				_target = GetTree().GetFirstNodeInGroup("Player") as Game.Player;
				if (_target == null) return;
			}
			
			// 更新状态
			UpdateState(deltaF);
			
			// 根据状态执行行为
			UpdateBehavior(deltaF);
			
			// 更新表情动画
			UpdateEmojiAnimation(deltaF);
		}
		
		private void UpdateState(float delta)
		{
			// 如果已经死亡，不更新状态
			if (_currentState == BossState.Dead) return;
			
			// 检查是否应该被迫现身
			if (_currentState == BossState.Invisible && _timeSinceLastPlayerAttack < FORCE_VISIBLE_ATTACK_INTERVAL)
			{
				_currentState = BossState.Chase;
				_stateTimer = 0f;
				SetVisibility(true);
			}
			
			// 根据当前状态和计时器决定下一个状态
			switch (_currentState)
			{
				case BossState.Idle:
					// 从待机状态转为追击状态
					if (_target != null)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
					}
					break;
					
				case BossState.Chase:
					// 检查是否可以进入隐身状态
					if (_invisibilityTimer >= InvisibilityCooldown && _timeSinceLastPlayerAttack >= FORCE_VISIBLE_ATTACK_INTERVAL)
					{
						_currentState = BossState.Invisible;
						_stateTimer = 0f;
						_invisibilityTimer = 0f;
						SetVisibility(false);
						GD.Print("风骑士进入隐身状态");
					}
					// 检查是否可以攻击
					else if (_target != null && GlobalPosition.DistanceTo(_target.GlobalPosition) <= AttackRange && _attackTimer >= AttackCooldown)
					{
						_currentState = BossState.Slashing;
						_stateTimer = 0f;
						_attackTimer = 0f;
						_slashDirection = (_target.GlobalPosition - GlobalPosition).Normalized();
						SetVisibility(true); // 攻击时解除隐身
						GD.Print("风骑士发动劈砍攻击");
					}
					break;
					
				case BossState.Slashing:
					// 劈砍攻击结束后释放冲击波
					if (_stateTimer >= 0.5f)
					{
						_currentState = BossState.ShockwaveAttack;
						_stateTimer = 0f;
						GD.Print("风骑士释放冲击波");
					}
					break;
					
				case BossState.ShockwaveAttack:
					// 冲击波攻击结束后回到追击状态
					if (_stateTimer >= 0.8f)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
					}
					break;
					
				case BossState.Invisible:
					// 隐身状态结束后回到追击状态
					if (_stateTimer >= InvisibilityDuration)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
						SetVisibility(true);
						GD.Print("风骑士隐身状态结束");
					}
					// 在隐身状态下如果接近玩家，有几率发动突袭
					else if (_target != null && GlobalPosition.DistanceTo(_target.GlobalPosition) <= AttackRange && _attackTimer >= AttackCooldown && GD.Randf() < 0.3f)
					{
						_currentState = BossState.Slashing;
						_stateTimer = 0f;
						_attackTimer = 0f;
						_slashDirection = (_target.GlobalPosition - GlobalPosition).Normalized();
						SetVisibility(true); // 攻击时解除隐身
						GD.Print("风骑士从隐身状态发动突袭");
					}
					break;
			}
		}
		
		private void UpdateBehavior(float delta)
		{
			// 根据当前状态执行相应行为
			switch (_currentState)
			{
				case BossState.Idle:
					// 待机状态下不移动
					Velocity = Vector2.Zero;
					break;
					
				case BossState.Chase:
					// 追击玩家
					ChaseTarget(delta);
					break;
					
				case BossState.Slashing:
					// 向前劈砍
					PerformSlashAttack(delta);
					break;
					
				case BossState.ShockwaveAttack:
					// 释放冲击波
					PerformShockwaveAttack(delta);
					break;
					
				case BossState.Invisible:
					// 隐身状态下继续追击，但玩家看不到
					ChaseTarget(delta);
					// 更新尘土粒子效果
					UpdateDustParticles();
					break;
					
				case BossState.Dead:
					// 死亡状态下不执行任何行为
					Velocity = Vector2.Zero;
					break;
			}
			
			// 应用移动
			MoveAndSlide();
		}
		
		private void ChaseTarget(float delta)
		{
			if (_target == null) return;
			
			// 计算到目标的方向
			Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
			
			// 根据血量状态决定移动速度
			float currentSpeed = _isLowHealth ? LowHealthMoveSpeed : NormalMoveSpeed;
			
			// 设置速度
			Velocity = direction * currentSpeed;
		}
		
		private void PerformSlashAttack(float delta)
		{
			// 向前冲刺劈砍
			float slashSpeed = _isLowHealth ? LowHealthMoveSpeed * 1.5f : NormalMoveSpeed * 1.5f;
			Velocity = _slashDirection * slashSpeed;
			
			// 检测是否击中玩家
			var spaceState = GetWorld2D().DirectSpaceState;
			var query = PhysicsRayQueryParameters2D.Create(
				GlobalPosition,
				GlobalPosition + _slashDirection * AttackRange,
				1  // 只检测玩家层
			);
			var result = spaceState.IntersectRay(query);
			
			if (result.Count > 0 && result["collider"].As<Node>() is Game.Player player)
			{
				// 对玩家造成伤害
				player.TakeDamage(AttackDamage);
				GD.Print($"风骑士劈砍攻击命中玩家，造成 {AttackDamage} 点伤害");
			}
		}
		
		private void PerformShockwaveAttack(float delta)
		{
			// 释放环形冲击波
			if (_stateTimer < 0.1f)  // 只在冲击波开始时执行一次
			{
				// 创建冲击波视觉效果
				CreateShockwaveEffect();
				
				// 检测范围内的玩家
				var playersInRange = GetPlayersInRadius(ShockwaveRadius);
				foreach (var player in playersInRange)
				{
					// 对玩家造成伤害
					player.TakeDamage(ShockwaveDamage);
					GD.Print($"风骑士冲击波命中玩家，造成 {ShockwaveDamage} 点伤害");
				}
			}
			
			// 冲击波释放期间不移动
			Velocity = Vector2.Zero;
		}
		
		private void CreateShockwaveEffect()
		{
			// 创建一个简单的冲击波视觉效果
			var shockwave = new CPUParticles2D();
			shockwave.Emitting = true;
			shockwave.OneShot = true;
			shockwave.Explosiveness = 1.0f;
			shockwave.Amount = 24;
			shockwave.Lifetime = 0.8f;
			shockwave.Direction = Vector2.Right;
			shockwave.Spread = 180.0f;
			shockwave.InitialVelocity = ShockwaveRadius / shockwave.Lifetime;
			shockwave.Scale = 3.0f;
			
			// 设置粒子颜色
			var gradient = new Gradient();
			gradient.Colors = new Color[] { new Color(0.7f, 0.7f, 1.0f, 0.8f), new Color(0.8f, 0.8f, 1.0f, 0.0f) };
			shockwave.ColorRamp = gradient;
			
			AddChild(shockwave);
			
			// 设置自动销毁
			var timer = new Timer();
			timer.WaitTime = 2.0f;
			timer.OneShot = true;
			timer.Connect("timeout", new Callable(shockwave, "queue_free"));
			AddChild(timer);
			timer.Start();
		}
		
		private List<Game.Player> GetPlayersInRadius(float radius)
		{
			var result = new List<Game.Player>();
			
			if (_target != null && GlobalPosition.DistanceTo(_target.GlobalPosition) <= radius)
			{
				result.Add(_target);
			}
			
			return result;
		}
		
		private void UpdateDustParticles()
		{
			// 在隐身状态下显示尘土粒子
			if (_currentState == BossState.Invisible && Velocity.Length() > 0.1f)
			{
				_dustParticles.Emitting = true;
			}
			else
			{
				_dustParticles.Emitting = false;
			}
		}
		
		private void SetVisibility(bool visible)
		{
			// 设置Boss的可见性
			if (visible)
			{
				Modulate = new Color(1, 1, 1, 1);
				_emojiLabel.Modulate = new Color(_emojiLabel.Modulate.R, _emojiLabel.Modulate.G, _emojiLabel.Modulate.B, 1);
			}
			else
			{
				Modulate = new Color(1, 1, 1, 0.2f);
				_emojiLabel.Modulate = new Color(_emojiLabel.Modulate.R, _emojiLabel.Modulate.G, _emojiLabel.Modulate.B, 0.2f);
			}
		}
		
		private void UpdateEmojiAnimation(float delta)
		{
			_animationTime += delta;
			
			// 根据Boss状态调整表情显示效果
			switch (_currentState)
			{
				case BossState.Idle:
					// 待机状态轻微摇晃
					_emojiLabel.Rotation = Mathf.Sin(_animationTime * 1.5f) * 0.1f;
					break;
					
				case BossState.Chase:
					// 追击状态快速摇晃
					_emojiLabel.Rotation = Mathf.Sin(_animationTime * 8) * 0.15f;
					break;
					
				case BossState.Slashing:
					// 劈砍状态放大
					_emojiLabel.Scale = new Vector2(
						Mathf.Sign(_emojiLabel.Scale.X) * (1.0f + 0.3f * Mathf.Sin(_animationTime * 10)),
						1.0f + 0.3f * Mathf.Sin(_animationTime * 10)
					);
					break;
					
				case BossState.ShockwaveAttack:
					// 冲击波状态旋转
					_emojiLabel.Rotation = _animationTime * 10;
					break;
					
				case BossState.Invisible:
					// 隐身状态闪烁
					float alpha = 0.1f + 0.1f * Mathf.Sin(_animationTime * 5);
					_emojiLabel.Modulate = new Color(_emojiLabel.Modulate.R, _emojiLabel.Modulate.G, _emojiLabel.Modulate.B, alpha);
					break;
					
				case BossState.Dead:
					// 死亡状态
					_emojiLabel.Rotation = Mathf.Pi/2; // 横躺
					_emojiLabel.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 变灰
					break;
			}
			
			// 低血量效果
			if (_isLowHealth && _currentState != BossState.Dead && _currentState != BossState.Invisible)
			{
				// 低血量时闪烁红色
				float redPulse = 0.5f + 0.5f * Mathf.Sin(_animationTime * 5);
				_emojiLabel.Modulate = new Color(1, redPulse, redPulse, _emojiLabel.Modulate.A);
			}
		}
		
		public override void TakeDamage(float damage)
		{
			base.TakeDamage(damage);
			
			// 记录最后一次受到攻击的时间
			_timeSinceLastPlayerAttack = 0f;
			
			// 如果在隐身状态下受到攻击，有几率被迫现身
			if (_currentState == BossState.Invisible && GD.Randf() < 0.3f)
			{
				_currentState = BossState.Chase;
				_stateTimer = 0f;
				SetVisibility(true);
				GD.Print("风骑士被攻击，被迫现身");
			}
			
			// 受伤时表情闪烁
			var tween = CreateTween();
			tween.TweenProperty(_emojiLabel, "modulate", new Color(1, 0, 0, _emojiLabel.Modulate.A), 0.1f);
			tween.TweenProperty(_emojiLabel, "modulate", new Color(_emojiLabel.Modulate.R, _emojiLabel.Modulate.G, _emojiLabel.Modulate.B, _emojiLabel.Modulate.A), 0.1f);
		}
		
		public override void Die()
		{
			// 设置死亡状态
			_currentState = BossState.Dead;
			
			// 确保可见
			SetVisibility(true);
			
			// 调用基类的Die方法
			base.Die();
			
			// 发出Boss被击败的信号
			EmitSignal(SignalName.BossDefeated);
		}
	}
}