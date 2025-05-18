using Godot;
using System;
using System.Collections.Generic;
using Game.Enemies;

namespace Game.Enemies.Boss
{
	public partial class LightKnightBoss : Enemy
	{
		[Signal]
		public delegate void BossDefeatedEventHandler();

		// 添加Unicode表情显示
		private Label _emojiLabel;
		private string _bossEmoji = "⚡"; // 光骑士表情
		private float _animationTime = 0f;
		
		// 光骑士特有的状态
		private enum BossState
		{
			Idle,
			Chase,           // 追击玩家
			Slashing,        // 普通劈砍
			LightningCast,   // 雷电读条
			LightningRelease,// 释放雷电
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
		public float AttackCooldown { get; set; } = 2.0f; // 普通攻击冷却
		
		[Export]
		public float LightningCooldown { get; set; } = 10.0f; // 雷电技能冷却
		
		[Export]
		public float LightningCastTime { get; set; } = 2.0f; // 雷电读条时间
		
		[Export]
		public float LightningDamage { get; set; } = 40.0f; // 雷电伤害
		
		[Export]
		public float StunDuration { get; set; } = 2.0f; // 眩晕持续时间
		
		[Export]
		public float StunProbability { get; set; } = 0.3f; // 普通攻击眩晕概率
		
		// 内部状态变量
		private BossState _currentState = BossState.Idle;
		private Game.Player _target;
		private float _stateTimer = 0f;
		private float _attackTimer = 0f;
		private float _lightningTimer = 0f;
		private Vector2 _slashDirection = Vector2.Zero;
		private ColorRect _bodyRect;
		private Label _castingLabel; // 显示读条状态
		private CPUParticles2D _lightningParticles; // 雷电粒子效果
		
		public override void _Ready()
		{
			base._Ready();
			MaxHealth = 550.0f;
			AttackDamage = 25.0f;
			CurrentHealth = MaxHealth;
			
			// 设置碰撞
			CollisionLayer = 4;  // 敌人层
			CollisionMask = 3;   // 与玩家(1)和墙(2)碰撞
			
			// 创建身体视觉效果
			SetupBodyVisual();
			
			// 创建显示Unicode表情的Label
			SetupEmojiDisplay();
			
			// 创建读条显示
			SetupCastingDisplay();
			
			// 创建雷电粒子效果
			SetupLightningParticles();
		}
		
		private void SetupBodyVisual()
		{
			_bodyRect = new ColorRect();
			_bodyRect.Size = new Vector2(48, 48);
			_bodyRect.Position = new Vector2(-24, -24);
			_bodyRect.Color = new Color(0.9f, 0.9f, 0.2f, 0.7f); // 金黄色半透明
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
		
		private void SetupCastingDisplay()
		{
			_castingLabel = new Label();
			_castingLabel.Text = "";
			_castingLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_castingLabel.Position = new Vector2(-40, -60);
			_castingLabel.Size = new Vector2(80, 20);
			_castingLabel.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 0.0f));
			_castingLabel.AddThemeFontSizeOverride("font_size", 16);
			_castingLabel.Visible = false;
			AddChild(_castingLabel);
		}
		
		private void SetupLightningParticles()
		{
			_lightningParticles = new CPUParticles2D();
			_lightningParticles.Amount = 50;
			_lightningParticles.Lifetime = 0.5f;
			_lightningParticles.OneShot = true;
			_lightningParticles.Explosiveness = 1.0f;
			_lightningParticles.Direction = Vector2.Up;
			_lightningParticles.Spread = 180.0f;
			_lightningParticles.Gravity = new Vector2(0, 0);
			_lightningParticles.InitialVelocity = 100.0f;
			_lightningParticles.Scale = 3.0f;
			_lightningParticles.Emitting = false;
			
			// 设置粒子颜色
			var gradient = new Gradient();
			gradient.Colors = new Color[] { 
				new Color(1.0f, 1.0f, 0.5f, 1.0f), 
				new Color(0.8f, 0.8f, 0.2f, 0.6f),
				new Color(1.0f, 1.0f, 0.0f, 0.0f) 
			};
			_lightningParticles.ColorRamp = gradient;
			
			AddChild(_lightningParticles);
		}
		
		public override void _PhysicsProcess(double delta)
		{
			float deltaF = (float)delta;
			
			// 更新计时器
			_stateTimer += deltaF;
			_attackTimer += deltaF;
			_lightningTimer += deltaF;
			
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
		}
		
		private void UpdateHealthBar()
		{
			var healthBar = GetNode<ProgressBar>("HealthBar");
			if (healthBar != null)
			{
				healthBar.Value = (CurrentHealth / MaxHealth) * 100;
			}
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
					// 检查是否可以释放雷电
					if (_lightningTimer >= LightningCooldown)
					{
						_currentState = BossState.LightningCast;
						_stateTimer = 0f;
						_castingLabel.Text = "雷电读条中...";
						_castingLabel.Visible = true;
						GD.Print("光骑士开始读条释放雷电");
					}
					// 检查是否可以攻击
					else if (_attackTimer >= AttackCooldown && IsTargetInRange(_target, AttackRange))
					{
						_currentState = BossState.Slashing;
						_stateTimer = 0f;
						_attackTimer = 0f;
						_slashDirection = (_target.GlobalPosition - GlobalPosition).Normalized();
						GD.Print("光骑士开始普通攻击");
					}
					break;
					
				case BossState.Slashing:
					// 攻击结束后回到追击状态
					if (_stateTimer >= 0.8f)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
					}
					break;
					
				case BossState.LightningCast:
					// 读条结束后释放雷电
					if (_stateTimer >= LightningCastTime)
					{
						_currentState = BossState.LightningRelease;
						_stateTimer = 0f;
						_lightningTimer = 0f;
						_castingLabel.Text = "雷电释放！";
						GD.Print("光骑士释放全屏雷电");
					}
					break;
					
				case BossState.LightningRelease:
					// 雷电释放结束后回到追击状态
					if (_stateTimer >= 1.0f)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
						_castingLabel.Visible = false;
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
					
				case BossState.LightningCast:
					// 读条期间不移动，但可以旋转面向玩家
					Velocity = Vector2.Zero;
					break;
					
				case BossState.LightningRelease:
					PerformLightningAttack(delta);
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
			// 普通攻击
			if (_stateTimer < 0.1f) // 只在攻击开始时执行一次
			{
				// 检测是否击中玩家
				if (IsTargetInRange(_target, AttackRange))
				{
					// 对玩家造成伤害
					_target.TakeDamage(AttackDamage);
					GD.Print($"光骑士攻击命中玩家，造成 {AttackDamage} 点伤害");
					
					// 概率造成眩晕
					if (GD.Randf() < StunProbability)
					{
						// 通知玩家被眩晕
						GD.Print("玩家被光骑士眩晕");
						// 这里应该调用玩家的眩晕方法，但需要玩家类支持
						// _target.ApplyStun(StunDuration);
					}
				}
			}
			
			// 攻击动画效果
			if (_stateTimer < 0.4f)
			{
				// 向前冲刺一小段距离
				Velocity = _slashDirection * MoveSpeed * 1.5f;
			}
			else
			{
				Velocity = Vector2.Zero;
			}
			
			MoveAndSlide();
		}
		
		private void PerformLightningAttack(float delta)
		{
			// 全屏雷电攻击
			if (_stateTimer < 0.1f) // 只在雷电释放开始时执行一次
			{
				// 创建雷电视觉效果
				CreateLightningEffect();
				
				// 检查玩家是否面对Boss
				if (_target != null)
				{
					bool isFacingBoss = IsPlayerFacingBoss();
					
					// 对玩家造成伤害
					_target.TakeDamage(LightningDamage);
					GD.Print($"光骑士雷电攻击命中玩家，造成 {LightningDamage} 点伤害");
					
					// 如果玩家面对Boss，则被眩晕
					if (isFacingBoss)
					{
						GD.Print("玩家面对光骑士，被雷电眩晕");
						// 这里应该调用玩家的眩晕方法，但需要玩家类支持
						// _target.ApplyStun(StunDuration);
					}
					else
					{
						GD.Print("玩家背对光骑士，免疫雷电眩晕");
					}
				}
			}
			
			// 雷电释放期间不移动
			Velocity = Vector2.Zero;
		}
		
		private void CreateLightningEffect()
		{
			// 激活雷电粒子效果
			_lightningParticles.Emitting = true;
			
			// 创建全屏闪光效果
			var flashRect = new ColorRect();
			flashRect.Size = GetViewport().GetVisibleRect().Size;
			flashRect.Position = -GlobalPosition;
			flashRect.Color = new Color(1, 1, 0.8f, 0.7f);
			AddChild(flashRect);
			
			// 闪光效果淡出
			var tween = CreateTween();
			tween.TweenProperty(flashRect, "color:a", 0.0f, 0.5f);
			tween.TweenCallback(Callable.From(() => flashRect.QueueFree()));
			
			// 创建额外的雷电效果
			for (int i = 0; i < 5; i++)
			{
				var lightning = new CPUParticles2D();
				lightning.Amount = 20;
				lightning.Lifetime = 0.3f;
				lightning.OneShot = true;
				lightning.Explosiveness = 1.0f;
				lightning.Direction = Vector2.Down;
				lightning.Spread = 30.0f;
				lightning.Position = new Vector2(GD.RandRange(-200, 200), -100);
				lightning.Gravity = new Vector2(0, 500);
				lightning.InitialVelocity = 200.0f;
				lightning.Scale = 2.0f;
				lightning.Emitting = true;
				
				// 设置粒子颜色
				var gradient = new Gradient();
				gradient.Colors = new Color[] { 
					new Color(1.0f, 1.0f, 1.0f, 1.0f), 
					new Color(1.0f, 1.0f, 0.0f, 0.6f)
				};
				lightning.ColorRamp = gradient;
				
				AddChild(lightning);
				
				// 设置自动销毁
				var timer = new Timer();
				timer.WaitTime = 1.0f;
				timer.OneShot = true;
				timer.Timeout += () => lightning.QueueFree();
				AddChild(timer);
				timer.Start();
			}
		}
		
		private bool IsPlayerFacingBoss()
		{
			if (_target == null) return false;
			
			// 获取从玩家到Boss的方向
			Vector2 playerToBossDirection = (GlobalPosition - _target.GlobalPosition).Normalized();
			
			// 获取玩家的朝向（这里假设玩家有一个名为Facing的属性表示朝向）
			// 由于我们没有玩家的具体实现，这里使用一个简化的方法
			// 假设玩家的速度方向就是朝向
			Vector2 playerFacing = _target.Velocity.Normalized();
			
			// 如果玩家速度为零，则假设玩家面向右侧
			if (playerFacing.LengthSquared() < 0.01f)
			{
				playerFacing = Vector2.Right;
			}
			
			// 计算点积，如果大于0，则玩家面向Boss
			float dotProduct = playerFacing.Dot(playerToBossDirection);
			return dotProduct > 0;
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
					// 攻击状态下旋转
					_emojiLabel.Rotation = Mathf.Sin(_animationTime * 10.0f) * 0.3f;
					_emojiLabel.Scale = new Vector2(1.2f, 1.2f);
					break;
					
				case BossState.LightningCast:
					// 读条状态下闪烁
					float alpha = 0.5f + 0.5f * Mathf.Sin(_animationTime * 10.0f);
					_emojiLabel.Modulate = new Color(1.0f, 1.0f, alpha, 1.0f);
					_emojiLabel.Scale = new Vector2(1.0f + _stateTimer / LightningCastTime * 0.5f, 1.0f + _stateTimer / LightningCastTime * 0.5f);
					break;
					
				case BossState.LightningRelease:
					// 释放状态下快速旋转
					_emojiLabel.Rotation = _animationTime * 20.0f;
					_emojiLabel.Scale = new Vector2(2.0f, 2.0f);
					_emojiLabel.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
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
			tween.TweenProperty(_bodyRect, "color", new Color(0.9f, 0.9f, 0.2f, 0.7f), 0.1f);
			
			GD.Print($"光骑士受到攻击，当前生命值：{CurrentHealth}");
		}
		
		public override void Die()
		{
			_currentState = BossState.Dead;
			GD.Print("光骑士被击败");
			
			// 发出被击败信号
			EmitSignal(SignalName.BossDefeated);
			
			// 死亡视觉效果
			_bodyRect.Color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			_emojiLabel.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			_castingLabel.Visible = false;
			
			// 延迟一段时间后移除
			var timer = new Timer();
			timer.WaitTime = 2.0f;
			timer.OneShot = true;
			AddChild(timer);
			timer.Timeout += () => QueueFree();
			timer.Start();
		}
	}
}