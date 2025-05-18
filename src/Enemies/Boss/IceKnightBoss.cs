using Godot;
using System;
using System.Collections.Generic;
using Game.Enemies;

namespace Game.Enemies.Boss
{
	public partial class IceKnightBoss : Enemy
	{
		[Signal]
		public delegate void BossDefeatedEventHandler();

		// 添加Unicode表情显示
		private Label _emojiLabel;
		private string _bossEmoji = "❄️"; // 冰骑士表情
		private float _animationTime = 0f;
		
		// 冰骑士特有的状态
		private enum BossState
		{
			Idle,
			Chase,           // 追击玩家
			Slashing,        // 普通劈砍
			Thrust,          // 突刺攻击
			IceSpikeAttack,  // 冰刺攻击
			Dead             // 死亡状态
		}
		
		// 基本属性
		[Export]
		public float MoveSpeed { get; set; } = 160.0f; // 移动速度
		
		[Export]
		public float AttackRange { get; set; } = 80.0f; // 攻击范围
		
		[Export]
		public float DetectionRange { get; set; } = 300.0f; // 检测范围
		
		[Export]
		public float SlashCooldown { get; set; } = 2.0f; // 普通劈砍冷却
		
		[Export]
		public float ThrustCooldown { get; set; } = 4.0f; // 突刺冷却
		
		[Export]
		public float IceSpikeCooldown { get; set; } = 6.0f; // 冰刺攻击冷却
		
		[Export]
		public float FreezeChance { get; set; } = 0.7f; // 冰冻几率
		
		[Export]
		public float FreezeDuration { get; set; } = 2.0f; // 冰冻持续时间
		
		[Export]
		public float IceSpikeDamage { get; set; } = 15.0f; // 冰刺伤害
		
		[Export]
		public float IceSpikeDotDamage { get; set; } = 5.0f; // 冰刺持续伤害
		
		[Export]
		public int IceSpikeCount { get; set; } = 3; // 每次生成的冰刺数量
		
		// 内部状态变量
		private BossState _currentState = BossState.Idle;
		private Game.Player _target;
		private float _stateTimer = 0f;
		private float _slashTimer = 0f;
		private float _thrustTimer = 0f;
		private float _iceSpikeTimer = 0f;
		private Vector2 _attackDirection = Vector2.Zero;
		private ColorRect _bodyRect;
		private List<IceSpike> _activeIceSpikes = new List<IceSpike>();
		
		public override void _Ready()
		{
			base._Ready();
			MaxHealth = 450.0f;
			AttackDamage = 25.0f;
			CurrentHealth = MaxHealth;
			
			// 设置碰撞
			CollisionLayer = 4;  // 敌人层
			CollisionMask = 3;   // 与玩家(1)和墙(2)碰撞
			
			// 创建身体视觉效果
			SetupBodyVisual();
			
			// 创建显示Unicode表情的Label
			SetupEmojiDisplay();
		}
		
		private void SetupBodyVisual()
		{
			_bodyRect = new ColorRect();
			_bodyRect.Size = new Vector2(48, 48);
			_bodyRect.Position = new Vector2(-24, -24);
			_bodyRect.Color = new Color(0.7f, 0.9f, 1.0f, 0.7f); // 淡蓝色半透明
			AddChild(_bodyRect);
		}
		
		private void SetupEmojiDisplay()
		{
			_emojiLabel = new Label();
			_emojiLabel.Text = _bossEmoji;
			_emojiLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_emojiLabel.VerticalAlignment = VerticalAlignment.Center;
			
			// 设置字体大小和颜色
			_emojiLabel.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f)); 
			_emojiLabel.AddThemeFontSizeOverride("font_size", 48);
			
			// 调整位置，使其与碰撞形状居中对齐
			_emojiLabel.Position = new Vector2(-24, -24);
			
			// 将表情放在最上层
			_emojiLabel.ZIndex = 1;
			
			AddChild(_emojiLabel);
		}
		
		public override void _PhysicsProcess(double delta)
		{
			float deltaF = (float)delta;
			
			// 更新计时器
			_stateTimer += deltaF;
			_slashTimer += deltaF;
			_thrustTimer += deltaF;
			_iceSpikeTimer += deltaF;
			
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
			
			// 更新健康条
			UpdateHealthBar();
			
			// 清理已销毁的冰刺
			CleanupDestroyedIceSpikes();
		}
		
		private void UpdateHealthBar()
		{
			var healthBar = GetNode<ProgressBar>("HealthBar");
			if (healthBar != null)
			{
				healthBar.Value = (CurrentHealth / MaxHealth) * 100;
			}
		}
		
		private void CleanupDestroyedIceSpikes()
		{
			_activeIceSpikes.RemoveAll(spike => spike == null || !IsInstanceValid(spike));
		}
		
		private void UpdateState(float delta)
		{
			// 如果已经死亡，不更新状态
			if (_currentState == BossState.Dead) return;
			
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
					// 检查是否可以使用冰刺攻击
					if (_iceSpikeTimer >= IceSpikeCooldown)
					{
						_currentState = BossState.IceSpikeAttack;
						_stateTimer = 0f;
						_iceSpikeTimer = 0f;
						GD.Print("冰骑士开始释放冰刺攻击");
					}
					// 检查是否可以使用突刺攻击
					else if (_thrustTimer >= ThrustCooldown && IsTargetInRange(_target, AttackRange * 1.5f))
					{
						_currentState = BossState.Thrust;
						_stateTimer = 0f;
						_thrustTimer = 0f;
						_attackDirection = (_target.GlobalPosition - GlobalPosition).Normalized();
						GD.Print("冰骑士开始突刺攻击");
					}
					// 检查是否可以使用普通劈砍
					else if (_slashTimer >= SlashCooldown && IsTargetInRange(_target, AttackRange))
					{
						_currentState = BossState.Slashing;
						_stateTimer = 0f;
						_slashTimer = 0f;
						_attackDirection = (_target.GlobalPosition - GlobalPosition).Normalized();
						GD.Print("冰骑士开始普通劈砍");
					}
					break;
					
				case BossState.Slashing:
					// 普通劈砍结束后回到追击状态
					if (_stateTimer >= 0.8f)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
					}
					break;
					
				case BossState.Thrust:
					// 突刺攻击结束后回到追击状态
					if (_stateTimer >= 1.2f)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
					}
					break;
					
				case BossState.IceSpikeAttack:
					// 冰刺攻击结束后回到追击状态
					if (_stateTimer >= 1.5f)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
					}
					break;
			}
		}
		
		private void UpdateBehavior(float delta)
		{
			switch (_currentState)
			{
				case BossState.Chase:
					ChaseTarget(delta);
					break;
					
				case BossState.Slashing:
					PerformSlashAttack(delta);
					break;
					
				case BossState.Thrust:
					PerformThrustAttack(delta);
					break;
					
				case BossState.IceSpikeAttack:
					PerformIceSpikeAttack(delta);
					break;
					
				case BossState.Dead:
					// 死亡状态不执行任何行为
					break;
			}
		}
		
		private void ChaseTarget(float delta)
		{
			if (_target == null) return;
			
			Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
			Vector2 velocity = direction * MoveSpeed;
			
			Velocity = velocity;
			MoveAndSlide();
		}
		
		private void PerformSlashAttack(float delta)
		{
			// 普通劈砍攻击
			if (_stateTimer < 0.1f) // 只在攻击开始时执行一次
			{
				// 检测是否击中玩家
				if (IsTargetInRange(_target, AttackRange))
				{
					// 对玩家造成伤害
					_target.TakeDamage(AttackDamage);
					GD.Print($"冰骑士普通劈砍命中玩家，造成 {AttackDamage} 点伤害");
					
					// 尝试冰冻玩家
					TryFreezeTarget();
				}
			}
			
			// 攻击动画效果
			if (_stateTimer < 0.4f)
			{
				// 向前冲刺一小段距离
				Velocity = _attackDirection * MoveSpeed * 0.5f;
			}
			else
			{
				Velocity = Vector2.Zero;
			}
			
			MoveAndSlide();
		}
		
		private void PerformThrustAttack(float delta)
		{
			// 突刺攻击
			if (_stateTimer < 0.8f)
			{
				// 向前冲刺
				Velocity = _attackDirection * MoveSpeed * 2.0f;
				MoveAndSlide();
				
				// 检测是否击中玩家
				if (IsTargetInRange(_target, AttackRange * 0.8f))
				{
					// 对玩家造成伤害
					_target.TakeDamage(AttackDamage * 1.5f);
					GD.Print($"冰骑士突刺攻击命中玩家，造成 {AttackDamage * 1.5f} 点伤害");
					
					// 高概率冰冻玩家
					TryFreezeTarget(FreezeChance + 0.2f);
				}
			}
			else
			{
				Velocity = Vector2.Zero;
			}
		}
		
		private void PerformIceSpikeAttack(float delta)
		{
			// 冰刺攻击
			if (_stateTimer < 0.1f) // 只在攻击开始时执行一次
			{
				// 生成多个冰刺
				SpawnIceSpikes();
			}
			
			// 攻击期间不移动
			Velocity = Vector2.Zero;
		}
		
		private void SpawnIceSpikes()
		{
			for (int i = 0; i < IceSpikeCount; i++)
			{
				// 创建冰刺
				var iceSpike = new IceSpike();
				
				// 设置冰刺属性
				iceSpike.Damage = IceSpikeDamage;
				iceSpike.DotDamage = IceSpikeDotDamage;
				iceSpike.FreezeChance = FreezeChance;
				iceSpike.FreezeDuration = FreezeDuration;
				iceSpike.Target = _target;
				
				// 随机位置生成冰刺
				Vector2 offset = new Vector2(
					(float)GD.RandRange(-100, 100),
					(float)GD.RandRange(-100, 100)
				);
				iceSpike.GlobalPosition = GlobalPosition + offset;
				
				// 添加到场景
				GetTree().CurrentScene.AddChild(iceSpike);
				
				// 跟踪活跃的冰刺
				_activeIceSpikes.Add(iceSpike);
				
				GD.Print($"冰骑士生成了冰刺 #{i+1}");
			}
		}
		
		private void TryFreezeTarget(float chance = -1)
		{
			// 使用默认冰冻几率或指定几率
			float freezeChance = chance < 0 ? FreezeChance : chance;
			
			// 随机判断是否冰冻
			if (GD.Randf() < freezeChance)
			{
				// 通知玩家被冰冻
				GD.Print($"玩家被冰冻，持续 {FreezeDuration} 秒");
				// 这里应该调用玩家的冰冻方法，但需要玩家类支持
				// _target.ApplyFreeze(FreezeDuration);
				
				// 创建冰冻视觉效果
				CreateFreezeEffect(_target.GlobalPosition);
			}
		}
		
		private void CreateFreezeEffect(Vector2 position)
		{
			// 创建一个简单的冰冻视觉效果
			var freezeEffect = new CPUParticles2D();
			freezeEffect.Emitting = true;
			freezeEffect.OneShot = true;
			freezeEffect.Explosiveness = 0.8f;
			freezeEffect.Amount = 16;
			freezeEffect.Lifetime = 1.0f;
			freezeEffect.Direction = Vector2.Up;
			freezeEffect.Spread = 180.0f;
			freezeEffect.InitialVelocity = 30.0f;
			freezeEffect.Scale = 2.0f;
			freezeEffect.GlobalPosition = position;
			
			// 设置粒子颜色
			var gradient = new Gradient();
			gradient.Colors = new Color[] { new Color(0.7f, 0.9f, 1.0f, 0.8f), new Color(0.8f, 0.95f, 1.0f, 0.0f) };
			freezeEffect.ColorRamp = gradient;
			
			GetTree().CurrentScene.AddChild(freezeEffect);
			
			// 设置自动销毁
			var timer = new Timer();
			timer.WaitTime = 2.0f;
			timer.OneShot = true;
			timer.Timeout += () => freezeEffect.QueueFree();
			freezeEffect.AddChild(timer);
			timer.Start();
		}
		
		private bool IsTargetInRange(Node2D target, float range)
		{
			if (target == null) return false;
			return GlobalPosition.DistanceTo(target.GlobalPosition) <= range;
		}
		
		private void UpdateEmojiAnimation(float delta)
		{
			_animationTime += delta;
			
			switch (_currentState)
			{
				case BossState.Idle:
				case BossState.Chase:
					// 正常状态下轻微缩放
					float scale = 1.0f + Mathf.Sin(_animationTime * 2.0f) * 0.1f;
					_emojiLabel.Scale = new Vector2(scale, scale);
					_emojiLabel.Rotation = 0;
					break;
					
				case BossState.Slashing:
					// 普通劈砍状态
					_emojiLabel.Rotation = Mathf.Sin(_animationTime * 10.0f) * 0.3f;
					break;
					
				case BossState.Thrust:
					// 突刺状态
					_emojiLabel.Scale = new Vector2(1.2f, 1.2f);
					break;
					
				case BossState.IceSpikeAttack:
					// 冰刺攻击状态
					_emojiLabel.Rotation = _animationTime * 5.0f;
					_emojiLabel.Modulate = new Color(
						0.7f + 0.3f * Mathf.Sin(_animationTime * 10.0f),
						0.7f + 0.3f * Mathf.Sin(_animationTime * 10.0f),
						1.0f
					);
					break;
					
				case BossState.Dead:
					// 死亡状态
					_emojiLabel.Rotation = Mathf.Pi/2; // 横躺
					_emojiLabel.Scale = new Vector2(1.0f, 1.0f);
					_emojiLabel.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 变灰
					break;
			}
		}
		
		public override void TakeDamage(float damage)
		{
			base.TakeDamage(damage);
			
			// 受伤闪烁效果
			var tween = CreateTween();
			tween.TweenProperty(_bodyRect, "color", new Color(1, 1, 1, 0.7f), 0.1f);
			tween.TweenProperty(_bodyRect, "color", new Color(0.7f, 0.9f, 1.0f, 0.7f), 0.1f);
		}
		
		public override void Die()
		{
			_currentState = BossState.Dead;
			GD.Print("冰骑士被击败");
			
			// 销毁所有活跃的冰刺
			foreach (var iceSpike in _activeIceSpikes)
			{
				if (iceSpike != null && IsInstanceValid(iceSpike))
				{
					iceSpike.QueueFree();
				}
			}
			_activeIceSpikes.Clear();
			
			// 发出被击败信号
			EmitSignal(SignalName.BossDefeated);
			
			// 死亡视觉效果
			_bodyRect.Color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			_emojiLabel.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			
			// 延迟一段时间后移除
			var timer = new Timer();
			timer.WaitTime = 2.0f;
			timer.OneShot = true;
			AddChild(timer);
			timer.Timeout += () => QueueFree();
			timer.Start();
		}
	}
	
	// 冰刺类
	public partial class IceSpike : Area2D
	{
		// 冰刺属性
		public float Damage { get; set; } = 15.0f;
		public float DotDamage { get; set; } = 5.0f;
		public float FreezeChance { get; set; } = 0.7f;
		public float FreezeDuration { get; set; } = 2.0f;
		public float Speed { get; set; } = 250.0f;
		public float Lifetime { get; set; } = 5.0f;
		public Game.Player Target { get; set; }
		
		private float _timer = 0f;
		private bool _hasHitTarget = false;
		private float _dotTimer = 0f;
		private const float DOT_INTERVAL = 0.5f;
		
		private Label _iceLabel;
		private ColorRect _iceRect;
		
		public override void _Ready()
		{
			// 设置碰撞
			CollisionLayer = 4;  // 敌人层
			CollisionMask = 1;   // 只与玩家碰撞
			
			// 添加碰撞形状
			var shape = new CircleShape2D();
			shape.Radius = 16f;
			var collision = new CollisionShape2D();
			collision.Shape = shape;
			AddChild(collision);
			
			// 创建视觉效果
			SetupVisuals();
			
			// 连接信号
			BodyEntered += OnBodyEntered;
		}
		
		private void SetupVisuals()
		{
			// 创建背景矩形
			_iceRect = new ColorRect();
			_iceRect.Size = new Vector2(32, 32);
			_iceRect.Position = new Vector2(-16, -16);
			_iceRect.Color = new Color(0.7f, 0.9f, 1.0f, 0.7f); // 淡蓝色半透明
			AddChild(_iceRect);
			
			// 创建冰刺表情
			_iceLabel = new Label();
			_iceLabel.Text = "❄️";
			_iceLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_iceLabel.VerticalAlignment = VerticalAlignment.Center;
			_iceLabel.AddThemeFontSizeOverride("font_size", 32);
			_iceLabel.Position = new Vector2(-16, -16);
			_iceLabel.ZIndex = 1;
			AddChild(_iceLabel);
		}
		
		public override void _Process(double delta)
		{
			float deltaF = (float)delta;
			
			// 更新生命周期
			_timer += deltaF;
			if (_timer >= Lifetime)
			{
				QueueFree();
				return;
			}
			
			// 如果已经击中目标，不再移动，但应用持续伤害
			if (_hasHitTarget)
			{
				ApplyDotDamage(deltaF);
				return;
			}
			
			// 追踪目标
			if (Target != null && IsInstanceValid(Target))
			{
				Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
				Position += direction * Speed * deltaF;
				
				// 旋转效果
				_iceLabel.Rotation += deltaF * 5.0f;
			}
			else
			{
				// 如果没有目标，自动销毁
				QueueFree();
			}
		}
		
		private void OnBodyEntered(Node2D body)
		{
			if (body is Game.Player player && !_hasHitTarget)
			{
				// 对玩家造成伤害
				player.TakeDamage(Damage);
				GD.Print($"冰刺击中玩家，造成 {Damage} 点伤害");
				
				// 尝试冰冻玩家
				if (GD.Randf() < FreezeChance)
				{
					GD.Print($"玩家被冰刺冰冻，持续 {FreezeDuration} 秒");
					// 这里应该调用玩家的冰冻方法，但需要玩家类支持
					// player.ApplyFreeze(FreezeDuration);
					
					// 创建冰冻视觉效果
					CreateFreezeEffect();
				}
				
				// 标记为已击中，开始应用持续伤害
				_hasHitTarget = true;
				
				// 改变视觉效果
				_iceRect.Color = new Color(0.5f, 0.8f, 1.0f, 0.9f);
				_iceLabel.Scale = new Vector2(1.2f, 1.2f);
				
				// 减少移动速度
				Speed = 0;
			}
		}
		
		private void ApplyDotDamage(float delta)
		{
			if (Target == null || !IsInstanceValid(Target)) return;
			
			_dotTimer += delta;
			if (_dotTimer >= DOT_INTERVAL)
			{
				_dotTimer = 0f;
				
				// 检查玩家是否在范围内
				if (GlobalPosition.DistanceTo(Target.GlobalPosition) <= 32f)
				{
					// 应用持续伤害
					Target.TakeDamage(DotDamage);
					GD.Print($"冰刺对玩家造成 {DotDamage} 点持续伤害");
					
					// 闪烁效果
					var tween = CreateTween();
					tween.TweenProperty(_iceRect, "color", new Color(1, 1, 1, 0.9f), 0.1f);
					tween.TweenProperty(_iceRect, "color", new Color(0.5f, 0.8f, 1.0f, 0.9f), 0.1f);
				}
			}
		}
		
		private void CreateFreezeEffect()
		{
			// 创建一个简单的冰冻视觉效果
			var freezeEffect = new CPUParticles2D();
			freezeEffect.Emitting = true;
			freezeEffect.OneShot = true;
			freezeEffect.Explosiveness = 0.8f;
			freezeEffect.Amount = 16;
			freezeEffect.Lifetime = 1.0f;
			freezeEffect.Direction = Vector2.Up;
			freezeEffect.Spread = 180.0f;
			freezeEffect.InitialVelocity = 30.0f;
			freezeEffect.Scale = 2.0f;
			
			// 设置粒子颜色
			var gradient = new Gradient();
			gradient.Colors = new Color[] { new Color(0.7f, 0.9f, 1.0f, 0.8f), new Color(0.8f, 0.95f, 1.0f, 0.0f) };
			freezeEffect.ColorRamp = gradient;
			
			AddChild(freezeEffect);
			
			// 设置自动销毁
			var timer = new Timer();
			timer.WaitTime = 2.0f;
			timer.OneShot = true;
			timer.Timeout += () => freezeEffect.QueueFree();
			freezeEffect.AddChild(timer);
			timer.Start();
		}
	}
}