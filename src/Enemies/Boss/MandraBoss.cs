using Godot;
using System;
using System.Collections.Generic;
using Game.Enemies;

namespace Game.Enemies.Boss
{
	public partial class MandraBoss : Enemy
	{
		[Signal]
		public delegate void BossDefeatedEventHandler();

		private enum BossState
		{
			Idle,
			Moving,
			Attacking,
			Spinning,
			Summoning
		}

		[Export]
		public float MoveSpeed { get; set; } = 80.0f;
		
		[Export]
		public float AttackRange { get; set; } = 150.0f;
		
		[Export]
		public float SpinDuration { get; set; } = 3.0f;
		
		[Export]
		public float SpinCooldown { get; set; } = 8.0f;
		
		[Export]
		public int PetalProjectileCount { get; set; } = 8;
		
		// 添加Unicode表情显示
		private Label _emojiLabel;
		private string _bossEmoji = "🏵️";

		private BossState _currentState = BossState.Idle;
		private float _spinTimer = 0f;
		private float _spinCooldownTimer = 0f;
		private Game.Player _target;
		private List<Node2D> _summonedPetals = new();

		public override void _Ready()
		{
			base._Ready();
			MaxHealth = 200.0f;
			AttackDamage = 25.0f;
			CurrentHealth = MaxHealth;
			
			// 创建显示Unicode表情的Label
			SetupEmojiDisplay();
		}
		
		private void SetupEmojiDisplay()
		{
			_emojiLabel = new Label();
			_emojiLabel.Text = _bossEmoji;
			_emojiLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_emojiLabel.VerticalAlignment = VerticalAlignment.Center;
			
			// 设置字体大小
			_emojiLabel.AddThemeColorOverride("font_color", Colors.Pink);
			_emojiLabel.AddThemeFontSizeOverride("font_size", 64);
			
			// 调整位置，使其与碰撞形状居中对齐
			_emojiLabel.Position = new Vector2(-32, -32);
			
			AddChild(_emojiLabel);
		}

		public override void _PhysicsProcess(double delta)
		{
			UpdateAI((float)delta);
			
			// 根据状态更新表情动画
			UpdateEmojiAnimation((float)delta);
		}
		
		private void UpdateEmojiAnimation(float delta)
		{
			// 根据Boss状态调整表情显示效果
			switch (_currentState)
			{
				case BossState.Spinning:
					break;
				case BossState.Attacking:
					_emojiLabel.Scale = new Vector2(1.2f, 1.2f);
					break;
				default:
					_emojiLabel.Rotation = 0;
					_emojiLabel.Scale = Vector2.One;
					break;
			}
		}

		private void UpdateAI(float delta)
		{
			if (_target == null)
			{
				_target = GetNode<Game.Player>("/root/Main/Player");
				if (_target == null) return;
			}

			UpdateTimers(delta);
			
			switch (_currentState)
			{
				case BossState.Idle:
					HandleIdleState();
					break;
				case BossState.Moving:
					HandleMovingState();
					break;
				case BossState.Attacking:
					HandleAttackingState();
					break;
				case BossState.Spinning:
					HandleSpinningState(delta);
					break;
				case BossState.Summoning:
					HandleSummoningState();
					break;
			}
		}

		private void UpdateTimers(float delta)
		{
			if (_spinCooldownTimer > 0)
			{
				_spinCooldownTimer -= delta;
			}

			if (_currentState == BossState.Spinning)
			{
				_spinTimer -= delta;
				if (_spinTimer <= 0)
				{
					_currentState = BossState.Idle;
					_spinCooldownTimer = SpinCooldown;
				}
			}
		}

		private void HandleIdleState()
		{
			float distanceToPlayer = GlobalPosition.DistanceTo(_target.GlobalPosition);
			
			if (distanceToPlayer <= AttackRange)
			{
				if (_spinCooldownTimer <= 0)
				{
					StartSpinAttack();
				}
				else
				{
					_currentState = BossState.Attacking;
				}
			}
			else
			{
				_currentState = BossState.Moving;
			}
		}

		private void HandleMovingState()
		{
			Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
			Velocity = direction * MoveSpeed;
			MoveAndSlide();

			if (GlobalPosition.DistanceTo(_target.GlobalPosition) <= AttackRange)
			{
				_currentState = BossState.Idle;
			}
		}

		private void HandleAttackingState()
		{
			// 发射单个花瓣
			ShootPetal();
			_currentState = BossState.Idle;
		}

		private void HandleSpinningState(float delta)
		{
			// 旋转并发射花瓣
			Rotate(delta * 5);
			if (Mathf.FloorToInt(_spinTimer * 2) % 2 == 0)
			{
				ShootPetalSpread();
			}
		}

		private void HandleSummoningState()
		{
			// 召唤小花
			SummonPetals();
			_currentState = BossState.Idle;
		}

		private void StartSpinAttack()
		{
			_currentState = BossState.Spinning;
			_spinTimer = SpinDuration;
		}

		private void ShootPetal()
		{
			var petal = new PetalProjectile();
			petal.Direction = (_target.GlobalPosition - GlobalPosition).Normalized();
			petal.GlobalPosition = GlobalPosition;
			GetTree().CurrentScene.AddChild(petal);
		}

		private void ShootPetalSpread()
		{
			float angleStep = 2 * Mathf.Pi / PetalProjectileCount;
			for (int i = 0; i < PetalProjectileCount; i++)
			{
				var petal = new PetalProjectile();
				float angle = i * angleStep;
				petal.Direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				petal.GlobalPosition = GlobalPosition;
				GetTree().CurrentScene.AddChild(petal);
			}
		}

		private void SummonPetals()
		{
			// 清理已经销毁的花瓣
			_summonedPetals.RemoveAll(p => !IsInstanceValid(p));
			
			if (_summonedPetals.Count >= 4) return;

			for (int i = 0; i < 2; i++)
			{
				var petalMinion = new PetalMinion();
				Vector2 offset = new Vector2(
					(float)GD.RandRange(-100, 100),
					(float)GD.RandRange(-100, 100)
				);
				petalMinion.GlobalPosition = GlobalPosition + offset;
				GetTree().CurrentScene.AddChild(petalMinion);
				_summonedPetals.Add(petalMinion);
			}
		}

		public override void Die()
		{
			GD.Print($"MandraBoss死亡! ID: {GetInstanceId()}");
			
			// 清理所有召唤物
			foreach (var petal in _summonedPetals)
			{
				if (IsInstanceValid(petal))
				{
					petal.QueueFree();
				}
			}
			
			// 发送Boss被击败信号
			EmitSignal(SignalName.BossDefeated);
			
			// 确保从场景中移除
			QueueFree();
		}
	}
}
