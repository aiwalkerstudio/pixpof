using Godot;
using System;
using System.Collections.Generic;
using Game.Enemies;

namespace Game.Enemies.Boss
{
	public partial class FireKnightBoss : Enemy
	{
		[Signal]
		public delegate void BossDefeatedEventHandler();

		// 添加Unicode表情显示
		private Label _emojiLabel;
		private string _bossEmoji = "🔥"; // 火骑士表情
		private float _animationTime = 0f;
		
		// 火骑士特有的状态
		private enum BossState
		{
			Idle,
			Chase,           // 追击玩家
			Slashing,        // 普通劈砍
			Explosion,       // 火苗爆炸状态
			Berserk,         // 狂暴状态
			Dead             // 死亡状态
		}
		
		// 基本属性
		[Export]
		public float NormalMoveSpeed { get; set; } = 150.0f; // 正常移动速度
		
		[Export]
		public float BerserkMoveSpeed { get; set; } = 250.0f; // 狂暴状态移动速度
		
		[Export]
		public float AttackRange { get; set; } = 80.0f; // 攻击范围
		
		[Export]
		public float BurnRange { get; set; } = 120.0f; // 灼烧范围
		
		[Export]
		public float BurnDamage { get; set; } = 5.0f; // 灼烧伤害（每秒）
		
		[Export]
		public float AttackCooldown { get; set; } = 1.5f; // 普通攻击冷却
		
		[Export]
		public float BerserkCooldown { get; set; } = 8.0f; // 未受到攻击进入狂暴的时间
		
		[Export]
		public float ExplosionDamage { get; set; } = 80.0f; // 爆炸伤害
		
		[Export]
		public float ExplosionRadius { get; set; } = 200.0f; // 爆炸半径
		
		[Export]
		public int MaxFireStacks { get; set; } = 10; // 最大火苗层数
		
		// 内部状态变量
		private BossState _currentState = BossState.Idle;
		private Game.Player _target;
		private float _stateTimer = 0f;
		private float _attackTimer = 0f;
		private float _lastHitTime = 0f;
		private int _fireStacks = 0; // 当前火苗层数
		private bool _isBerserk = false; // 是否处于狂暴状态
		private ColorRect _bodyRect;
		private Label _fireStackLabel; // 显示火苗层数
		private CPUParticles2D _fireParticles; // 火焰粒子效果
		
		public override void _Ready()
		{
			base._Ready();
			MaxHealth = 600.0f;
			AttackDamage = 25.0f;
			CurrentHealth = MaxHealth;
			
			// 设置碰撞
			CollisionLayer = 4;  // 敌人层
			CollisionMask = 3;   // 与玩家(1)和墙(2)碰撞
			
			// 创建身体视觉效果
			SetupBodyVisual();
			
			// 创建显示Unicode表情的Label
			SetupEmojiDisplay();
			
			// 创建火苗层数显示
			SetupFireStackDisplay();
			
			// 创建火焰粒子效果
			SetupFireParticles();
			
			// 初始化时间
			_lastHitTime = Time.GetTicksMsec() / 1000.0f;
		}
		
		private void SetupBodyVisual()
		{
			_bodyRect = new ColorRect();
			_bodyRect.Size = new Vector2(48, 48);
			_bodyRect.Position = new Vector2(-24, -24);
			_bodyRect.Color = new Color(0.8f, 0.3f, 0.1f, 0.7f); // 橙红色半透明
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
		
		private void SetupFireStackDisplay()
		{
			_fireStackLabel = new Label();
			_fireStackLabel.Text = "0";
			_fireStackLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_fireStackLabel.Position = new Vector2(-15, -60);
			_fireStackLabel.Size = new Vector2(30, 20);
			_fireStackLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.5f, 0.0f));
			_fireStackLabel.AddThemeFontSizeOverride("font_size", 16);
			AddChild(_fireStackLabel);
		}
		
		private void SetupFireParticles()
		{
			_fireParticles = new CPUParticles2D();
			_fireParticles.Amount = 30;
			_fireParticles.Lifetime = 0.8f;
			_fireParticles.OneShot = false;
			_fireParticles.Explosiveness = 0.0f;
			_fireParticles.Direction = Vector2.Up;
			_fireParticles.Spread = 60.0f;
			_fireParticles.Gravity = new Vector2(0, -50);
			_fireParticles.InitialVelocity = 30.0f;
			_fireParticles.Scale = 2.0f;
			_fireParticles.Emitting = true;
			
			// 设置粒子颜色
			var gradient = new Gradient();
			gradient.Colors = new Color[] { 
				new Color(1.0f, 0.8f, 0.0f, 0.8f), 
				new Color(1.0f, 0.4f, 0.0f, 0.6f),
				new Color(0.8f, 0.0f, 0.0f, 0.0f) 
			};
			_fireParticles.ColorRamp = gradient;
			
			AddChild(_fireParticles);
		}
		
		public override void _PhysicsProcess(double delta)
		{
			float deltaF = (float)delta;
			
			// 更新计时器
			_stateTimer += deltaF;
			_attackTimer += deltaF;
			
			// 获取目标（玩家）
			if (_target == null)
			{
				_target = GetTree().GetFirstNodeInGroup("Player") as Game.Player;
				if (_target == null) return;
			}
			
			// 检查是否应该进入狂暴状态
			CheckBerserkState();
			
			// 更新状态
			UpdateState(deltaF);
			
			// 根据状态执行行为
			UpdateBehavior(deltaF);
			
			// 更新表情动画
			UpdateEmojiAnimation(deltaF);
			
			// 更新健康条
			UpdateHealthBar();
			
			// 更新火焰粒子效果
			UpdateFireParticles();
			
			// 检查近身灼烧
			CheckBurnDamage(deltaF);
		}
		
		private void UpdateHealthBar()
		{
			var healthBar = GetNode<ProgressBar>("HealthBar");
			if (healthBar != null)
			{
				healthBar.Value = (CurrentHealth / MaxHealth) * 100;
			}
		}
		
		private void CheckBerserkState()
		{
			float currentTime = Time.GetTicksMsec() / 1000.0f;
			
			// 如果长时间未受到攻击，进入狂暴状态
			if (!_isBerserk && currentTime - _lastHitTime > BerserkCooldown)
			{
				EnterBerserkState();
			}
		}
		
		private void EnterBerserkState()
		{
			_isBerserk = true;
			
			// 视觉效果变化
			_bodyRect.Size = new Vector2(64, 64);
			_bodyRect.Position = new Vector2(-32, -32);
			_bodyRect.Color = new Color(1.0f, 0.2f, 0.0f, 0.8f); // 更鲜艳的红色
			
			// 表情变大
			_emojiLabel.Position = new Vector2(-32, -32);
			_emojiLabel.AddThemeFontSizeOverride("font_size", 64);
			
			// 增加粒子效果
			_fireParticles.Amount = 50;
			
			GD.Print("火骑士进入狂暴状态！");
		}
		
		private void ExitBerserkState()
		{
			_isBerserk = false;
			
			// 恢复正常视觉效果
			_bodyRect.Size = new Vector2(48, 48);
			_bodyRect.Position = new Vector2(-24, -24);
			_bodyRect.Color = new Color(0.8f, 0.3f, 0.1f, 0.7f);
			
			// 表情恢复正常大小
			_emojiLabel.Position = new Vector2(-24, -24);
			_emojiLabel.AddThemeFontSizeOverride("font_size", 48);
			
			// 恢复正常粒子效果
			_fireParticles.Amount = 30;
			
			GD.Print("火骑士退出狂暴状态");
		}
		
		private void UpdateState(float delta)
		{
			// 如果已经死亡，不更新状态
			if (_currentState == BossState.Dead) return;
			
			// 检查是否应该爆炸
			if (_fireStacks >= MaxFireStacks && _currentState != BossState.Explosion)
			{
				_currentState = BossState.Explosion;
				_stateTimer = 0f;
				GD.Print("火骑士火苗层数达到最大，即将爆炸！");
				return;
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
					// 检查是否可以攻击
					if (_attackTimer >= AttackCooldown && IsTargetInRange(_target, AttackRange))
					{
						_currentState = BossState.Slashing;
						_stateTimer = 0f;
						_attackTimer = 0f;
						GD.Print("火骑士开始攻击");
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
					
				case BossState.Explosion:
					// 爆炸结束后回到追击状态
					if (_stateTimer >= 1.5f)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
						_fireStacks = 0; // 重置火苗层数
						UpdateFireStackDisplay();
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
					
				case BossState.Explosion:
					PerformExplosion(delta);
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
			float currentSpeed = _isBerserk ? BerserkMoveSpeed : NormalMoveSpeed;
			Vector2 velocity = direction * currentSpeed;
			
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
					// 计算伤害，狂暴状态下伤害提高
					float damage = _isBerserk ? AttackDamage * 1.5f : AttackDamage;
					
					// 对玩家造成伤害
					_target.TakeDamage(damage);
					GD.Print($"火骑士攻击命中玩家，造成 {damage} 点伤害");
				}
			}
			
			// 攻击动画效果
			if (_stateTimer < 0.4f)
			{
				// 向前冲刺一小段距离
				Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
				Velocity = direction * (_isBerserk ? NormalMoveSpeed * 1.5f : NormalMoveSpeed);
			}
			else
			{
				Velocity = Vector2.Zero;
			}
			
			MoveAndSlide();
		}
		
		private void PerformExplosion(float delta)
		{
			// 爆炸攻击
			if (_stateTimer < 0.1f) // 只在爆炸开始时执行一次
			{
				// 创建爆炸视觉效果
				CreateExplosionEffect();
				
				// 对范围内的玩家造成伤害
				if (_target != null && GlobalPosition.DistanceTo(_target.GlobalPosition) <= ExplosionRadius)
				{
					_target.TakeDamage(ExplosionDamage);
					GD.Print($"火骑士爆炸命中玩家，造成 {ExplosionDamage} 点伤害");
				}
			}
			
			// 爆炸期间不移动
			Velocity = Vector2.Zero;
		}
		
		private void CreateExplosionEffect()
		{
			// 创建一个爆炸视觉效果
			var explosion = new CPUParticles2D();
			explosion.Emitting = true;
			explosion.OneShot = true;
			explosion.Explosiveness = 1.0f;
			explosion.Amount = 100;
			explosion.Lifetime = 1.0f;
			explosion.Direction = Vector2.Right;
			explosion.Spread = 180.0f;
			explosion.InitialVelocity = ExplosionRadius;
			explosion.Scale = 3.0f;
			
			// 设置粒子颜色
			var gradient = new Gradient();
			gradient.Colors = new Color[] { 
				new Color(1.0f, 1.0f, 0.0f, 1.0f), 
				new Color(1.0f, 0.5f, 0.0f, 0.8f),
				new Color(0.8f, 0.0f, 0.0f, 0.0f) 
			};
			explosion.ColorRamp = gradient;
			
			AddChild(explosion);
			
			// 设置自动销毁
			var timer = new Timer();
			timer.WaitTime = 2.0f;
			timer.OneShot = true;
			timer.Timeout += () => explosion.QueueFree();
			AddChild(timer);
			timer.Start();
			
			// 屏幕震动效果（如果有相机节点的话）
			// 这里需要根据实际项目结构调整
			var camera = GetViewport().GetCamera2D();
			if (camera != null)
			{
				// 添加震动效果
				// camera.ApplyShake(0.5f, 10.0f);
			}
		}
		
		private void CheckBurnDamage(float delta)
		{
			// 检查玩家是否在灼烧范围内
			if (_target != null && GlobalPosition.DistanceTo(_target.GlobalPosition) <= BurnRange)
			{
				// 对玩家造成持续灼烧伤害
				float burnDamagePerFrame = BurnDamage * delta;
				_target.TakeDamage(burnDamagePerFrame);
				
				// 不需要每帧都打印日志，可以降低频率
				if (GD.Randf() < 0.05f)
				{
					GD.Print($"玩家受到火骑士灼烧，每秒 {BurnDamage} 点伤害");
				}
			}
		}
		
		private void UpdateFireParticles()
		{
			// 根据火苗层数和狂暴状态调整粒子效果
			if (_isBerserk)
			{
				_fireParticles.InitialVelocity = 50.0f;
				_fireParticles.Scale = 3.0f;
			}
			else
			{
				_fireParticles.InitialVelocity = 30.0f + _fireStacks * 2.0f;
				_fireParticles.Scale = 2.0f + _fireStacks * 0.1f;
			}
			
			// 火苗层数越高，火焰越大
			_fireParticles.Amount = 30 + _fireStacks * 3;
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
					break;
					
				case BossState.Slashing:
					// 攻击状态下旋转
					_emojiLabel.Rotation = Mathf.Sin(_animationTime * 10.0f) * 0.3f;
					break;
					
				case BossState.Explosion:
					// 爆炸状态下快速旋转和缩放
					_emojiLabel.Rotation = _animationTime * 10.0f;
					_emojiLabel.Scale = new Vector2(
						1.5f + Mathf.Sin(_animationTime * 20.0f) * 0.5f,
						1.5f + Mathf.Sin(_animationTime * 20.0f) * 0.5f
					);
					break;
					
				case BossState.Dead:
					// 死亡状态
					_emojiLabel.Rotation = Mathf.Pi/2; // 横躺
					_emojiLabel.Scale = new Vector2(1.0f, 1.0f);
					_emojiLabel.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 变灰
					break;
			}
			
			// 狂暴状态下的额外动画效果
			if (_isBerserk && _currentState != BossState.Dead)
			{
				// 狂暴状态下闪烁
				float redPulse = 0.7f + 0.3f * Mathf.Sin(_animationTime * 5.0f);
				_emojiLabel.Modulate = new Color(1.0f, redPulse, redPulse, 1.0f);
			}
			else if (_currentState != BossState.Dead)
			{
				_emojiLabel.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			}
		}
		
		// 重写TakeDamage方法以实现火苗叠加机制
		public override void TakeDamage(float damage)
		{
			base.TakeDamage(damage);
			
			// 记录最后一次受到攻击的时间
			_lastHitTime = Time.GetTicksMsec() / 1000.0f;
			
			// 如果处于狂暴状态，受到攻击后退出狂暴
			if (_isBerserk)
			{
				ExitBerserkState();
			}
			
			// 增加火苗层数
			_fireStacks++;
			UpdateFireStackDisplay();
			
			// 受伤闪烁效果
			var tween = CreateTween();
			tween.TweenProperty(_bodyRect, "color", new Color(1, 1, 1, 0.7f), 0.1f);
			tween.TweenProperty(_bodyRect, "color", new Color(0.8f, 0.3f, 0.1f, 0.7f), 0.1f);
			
			GD.Print($"火骑士受到攻击，火苗层数增加到 {_fireStacks}");
		}
		
		private void UpdateFireStackDisplay()
		{
			_fireStackLabel.Text = _fireStacks.ToString();
			
			// 根据火苗层数改变颜色
			float intensity = (float)_fireStacks / MaxFireStacks;
			_fireStackLabel.AddThemeColorOverride("font_color", new Color(
				1.0f,
				1.0f - intensity * 0.5f,
				0.0f
			));
			
			// 火苗层数接近最大值时闪烁警告
			if (_fireStacks >= MaxFireStacks * 0.7f)
			{
				var tween = CreateTween();
				tween.TweenProperty(_fireStackLabel, "modulate", new Color(1, 1, 1, 0.5f), 0.3f);
				tween.TweenProperty(_fireStackLabel, "modulate", new Color(1, 1, 1, 1.0f), 0.3f);
			}
		}
		
		public override void Die()
		{
			_currentState = BossState.Dead;
			GD.Print("火骑士被击败");
			
			// 发出被击败信号
			EmitSignal(SignalName.BossDefeated);
			
			// 死亡视觉效果
			_bodyRect.Color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			_emojiLabel.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			_fireParticles.Emitting = false;
			
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