using Godot;
using System;
using System.Collections.Generic;
using Game.Enemies;

namespace Game.Enemies.Boss
{
	public partial class GuardianKnightBoss : Enemy
	{
		[Signal]
		public delegate void BossDefeatedEventHandler();

		// 添加Unicode表情显示
		private Label _emojiLabel;
		private string _bossEmoji = "🛡️"; // 守护骑士表情
		private float _animationTime = 0f;
		
		// 守护骑士特有的状态
		private enum BossState
		{
			Idle,
			Chase,           // 追击玩家
			Slashing,        // 铁锤劈砍
			DefenseSwitch,   // 防御切换状态
			Dead             // 死亡状态
		}
		
		// 守护骑士特有的防御状态
		private enum DefenseState
		{
			Physical,  // 蓝条状态：仅受物理攻击
			Magical    // 红条状态：仅受魔法攻击
		}
		
		// 基本属性
		[Export]
		public float MoveSpeed { get; set; } = 150.0f; // 移动速度
		
		[Export]
		public float AttackRange { get; set; } = 80.0f; // 攻击范围
		
		[Export]
		public float DetectionRange { get; set; } = 300.0f; // 检测范围
		
		[Export]
		public float AttackCooldown { get; set; } = 2.0f; // 攻击冷却
		
		[Export]
		public float DefenseSwitchCooldown { get; set; } = 10.0f; // 防御切换冷却
		
		[Export]
		public float DefenseSwitchDuration { get; set; } = 2.0f; // 防御切换持续时间
		
		[Export]
		public float ReflectionDamage { get; set; } = 15.0f; // 反伤伤害
		
		[Export]
		public float HealAmount { get; set; } = 10.0f; // 反伤回血量
		
		[Export]
		public float StunDuration { get; set; } = 1.5f; // 眩晕持续时间
		
		// 内部状态变量
		private BossState _currentState = BossState.Idle;
		private DefenseState _defenseState = DefenseState.Physical; // 默认蓝条状态
		private Game.Player _target;
		private float _stateTimer = 0f;
		private float _attackTimer = 0f;
		private float _defenseSwitchTimer = 0f;
		private Vector2 _slashDirection = Vector2.Zero;
		private ColorRect _bodyRect;
		private Label _stateLabel;
		
		public override void _Ready()
		{
			base._Ready();
			MaxHealth = 500.0f;
			AttackDamage = 30.0f;
			CurrentHealth = MaxHealth;
			
			// 设置碰撞
			CollisionLayer = 4;  // 敌人层
			CollisionMask = 3;   // 与玩家(1)和墙(2)碰撞
			
			// 创建身体视觉效果
			SetupBodyVisual();
			
			// 创建显示Unicode表情的Label
			SetupEmojiDisplay();
			
			// 创建状态标签
			SetupStateLabel();
			
			// 更新防御状态视觉效果
			UpdateDefenseStateVisual();
		}
		
		private void SetupBodyVisual()
		{
			_bodyRect = new ColorRect();
			_bodyRect.Size = new Vector2(48, 48);
			_bodyRect.Position = new Vector2(-24, -24);
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
		
		private void SetupStateLabel()
		{
			_stateLabel = new Label();
			_stateLabel.Text = "物理防御";
			_stateLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_stateLabel.Position = new Vector2(-40, -50);
			_stateLabel.Size = new Vector2(80, 20);
			AddChild(_stateLabel);
		}
		
		private void UpdateDefenseStateVisual()
		{
			if (_defenseState == DefenseState.Physical)
			{
				// 蓝条状态
				_bodyRect.Color = new Color(0.2f, 0.4f, 0.8f, 0.7f); // 蓝色半透明
				_stateLabel.Text = "物理防御";
				_stateLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.4f, 0.8f));
				
				// 更新健康条颜色
				var healthBar = GetNode<ProgressBar>("HealthBar");
				if (healthBar != null)
				{
					var styleBox = new StyleBoxFlat();
					styleBox.BgColor = new Color(0.2f, 0.4f, 0.8f);
					healthBar.AddThemeStyleboxOverride("fill", styleBox);
				}
			}
			else
			{
				// 红条状态
				_bodyRect.Color = new Color(0.8f, 0.2f, 0.2f, 0.7f); // 红色半透明
				_stateLabel.Text = "魔法防御";
				_stateLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.2f, 0.2f));
				
				// 更新健康条颜色
				var healthBar = GetNode<ProgressBar>("HealthBar");
				if (healthBar != null)
				{
					var styleBox = new StyleBoxFlat();
					styleBox.BgColor = new Color(0.8f, 0.2f, 0.2f);
					healthBar.AddThemeStyleboxOverride("fill", styleBox);
				}
			}
		}
		
		public override void _PhysicsProcess(double delta)
		{
			float deltaF = (float)delta;
			
			// 更新计时器
			_stateTimer += deltaF;
			_attackTimer += deltaF;
			_defenseSwitchTimer += deltaF;
			
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
					// 检查是否可以进入防御切换状态
					if (_defenseSwitchTimer >= DefenseSwitchCooldown)
					{
						_currentState = BossState.DefenseSwitch;
						_stateTimer = 0f;
						_defenseSwitchTimer = 0f;
						GD.Print("守护骑士开始切换防御状态");
					}
					// 检查是否可以攻击
					else if (_attackTimer >= AttackCooldown && IsTargetInRange(_target, AttackRange))
					{
						_currentState = BossState.Slashing;
						_stateTimer = 0f;
						_attackTimer = 0f;
						_slashDirection = (_target.GlobalPosition - GlobalPosition).Normalized();
						GD.Print("守护骑士开始铁锤劈砍攻击");
					}
					break;
					
				case BossState.Slashing:
					// 劈砍攻击结束后回到追击状态
					if (_stateTimer >= 1.0f)
					{
						_currentState = BossState.Chase;
						_stateTimer = 0f;
					}
					break;
					
				case BossState.DefenseSwitch:
					// 防御切换结束后回到追击状态
					if (_stateTimer >= DefenseSwitchDuration)
					{
						// 随机切换防御状态
						_defenseState = (Random.Shared.Next(2) == 0) ? DefenseState.Physical : DefenseState.Magical;
						UpdateDefenseStateVisual();
						
						_currentState = BossState.Chase;
						_stateTimer = 0f;
						GD.Print($"守护骑士切换到{(_defenseState == DefenseState.Physical ? "蓝条" : "红条")}状态");
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
					
				case BossState.DefenseSwitch:
					// 防御切换时显示特效
					ShowDefenseSwitchEffect(delta);
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
			// 向前冲刺一段距离
			Velocity = _slashDirection * MoveSpeed * 2.0f;
			MoveAndSlide();
			
			// 检测是否击中玩家
			if (IsTargetInRange(_target, AttackRange))
			{
				// 对玩家造成伤害
				_target.TakeDamage(AttackDamage);
				
				// 概率造成眩晕和击飞
				if (Random.Shared.Next(100) < 30) // 30%概率
				{
					// 通知玩家被眩晕
					GD.Print("玩家被守护骑士眩晕和击飞");
					// 这里应该调用玩家的眩晕方法，但需要玩家类支持
					// _target.ApplyStun(StunDuration);
					
					// 击飞效果（给玩家一个向后的冲量）
					Vector2 knockbackDirection = (_target.GlobalPosition - GlobalPosition).Normalized();
					// 这里应该调用玩家的击飞方法，但需要玩家类支持
					// _target.ApplyKnockback(knockbackDirection * 300.0f);
				}
			}
		}
		
		private void ShowDefenseSwitchEffect(float delta)
		{
			// 在防御切换状态下闪烁效果
			float alpha = Mathf.Sin(_stateTimer * 10.0f) * 0.5f + 0.5f;
			_bodyRect.Modulate = new Color(1, 1, 1, alpha);
			
			// 显示切换提示
			_stateLabel.Text = "切换中...";
			_stateLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
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
					_emojiLabel.Rotation = Mathf.Sin(_animationTime * 10.0f) * 0.5f;
					_emojiLabel.Scale = new Vector2(1.2f, 1.2f);
					break;
					
				case BossState.DefenseSwitch:
					// 切换状态下旋转
					_emojiLabel.Rotation = _animationTime * 5.0f;
					break;
					
				case BossState.Dead:
					// 死亡状态
					_emojiLabel.Rotation = Mathf.Pi/2; // 横躺
					_emojiLabel.Scale = new Vector2(1.0f, 1.0f);
					break;
			}
		}
		
		// 重写TakeDamage方法以实现属性吸收机制
		public override void TakeDamage(float damage, bool isMagical = false)
		{
			// 如果在防御切换状态，不受伤害
			if (_currentState == BossState.DefenseSwitch) 
			{
				GD.Print("守护骑士在切换防御状态，免疫伤害");
				return;
			}
			
			// 根据当前防御状态和攻击类型决定是否受伤
			if ((_defenseState == DefenseState.Physical && !isMagical) || 
				(_defenseState == DefenseState.Magical && isMagical))
			{
				// 正常受伤
				base.TakeDamage(damage);
				GD.Print($"守护骑士受到{damage}点{(isMagical ? "魔法" : "物理")}伤害，当前生命值：{CurrentHealth}");
				
				// 受伤闪烁效果
				var tween = CreateTween();
				tween.TweenProperty(_bodyRect, "color", new Color(1, 1, 1, 0.7f), 0.1f);
				tween.TweenProperty(_bodyRect, "color", _defenseState == DefenseState.Physical ? 
					new Color(0.2f, 0.4f, 0.8f, 0.7f) : new Color(0.8f, 0.2f, 0.2f, 0.7f), 0.1f);
			}
			else
			{
				// 错误的攻击类型，触发反伤和回血
				if (_target != null)
				{
					// 对玩家造成反伤
					_target.TakeDamage(ReflectionDamage);
					GD.Print($"守护骑士反弹{ReflectionDamage}点伤害给玩家");
					
					// 自身回血
					CurrentHealth = Mathf.Min(CurrentHealth + HealAmount, MaxHealth);
					GD.Print($"守护骑士回复{HealAmount}点生命值，当前生命值：{CurrentHealth}");
					
					// 回血特效
					var tween = CreateTween();
					tween.TweenProperty(_bodyRect, "color", new Color(0, 1, 0, 0.7f), 0.2f);
					tween.TweenProperty(_bodyRect, "color", _defenseState == DefenseState.Physical ? 
						new Color(0.2f, 0.4f, 0.8f, 0.7f) : new Color(0.8f, 0.2f, 0.2f, 0.7f), 0.2f);
				}
			}
		}
		
		public override void Die()
		{
			_currentState = BossState.Dead;
			GD.Print("守护骑士被击败");
			
			// 发出被击败信号
			EmitSignal(SignalName.BossDefeated);
			
			// 死亡视觉效果
			_bodyRect.Color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			_emojiLabel.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			_stateLabel.Text = "已击败";
			_stateLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			
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