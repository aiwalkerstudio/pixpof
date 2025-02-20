using Godot;
using System;
using System.Collections.Generic;
using Game;
using Game.Items;

namespace Game.Enemies
{
	public partial class Monster : Enemy
	{
		[Export]
		public float MoveSpeed { get; set; } = 100.0f;
		
		[Export]
		public float DetectionRange { get; set; } = 200.0f;
		
		private Game.Player _target;
		private List<DotEffect> _dotEffects = new();

		// 添加新的状态系统
		private enum State
		{
			Idle,
			Chase,
			Attack,
			Dead
		}
		
		private State _currentState = State.Idle;
		private float _attackTimer = 0.0f;
		private bool _canAttack = true;
		private Area2D _attackArea;
		
		[Export]
		public float AttackCooldown { get; set; } = 1.0f;

		[Export]
		public float MaxHealth { get; set; } = 100;
		
		[Export]
		public float CurrentHealth { get; set; }

		private class DotEffect
		{
			public float DamagePerSecond { get; set; }
			public float RemainingDuration { get; set; }

			public DotEffect(float dps, float duration)
			{
				DamagePerSecond = dps;
				RemainingDuration = duration;
			}

			public void Update(float delta)
			{
				RemainingDuration -= delta;
			}
		}

		public override void _Ready()
		{
			GD.Print($"=== Monster {Name} Initializing ===");
			
			// 先设置属性
			MaxHealth = 50.0f;  // 设置最大生命值
			
			// 然后调用基类的_Ready()，它会设置CurrentHealth = MaxHealth
			base._Ready();  
			
			GD.Print($"Monster {Name} health set to: {CurrentHealth}/{MaxHealth}");
			
			// 设置碰撞层
			CollisionLayer = 4;  // 敌人层（第3层）
			CollisionMask = 9;   // 可以与玩家层（第1层）和投射物层（第4层）碰撞
			
			// 添加碰撞形状
			var shape = new CircleShape2D();
			shape.Radius = 16f;
			var collision = new CollisionShape2D();
			collision.Shape = shape;
			AddChild(collision);
			
			// 确保碰撞检测已启用
			SetCollisionLayerValue(3, true);  // 设置第3层（值为4）
			SetCollisionMaskValue(1, true);   // 设置第1层
			SetCollisionMaskValue(4, true);   // 设置第4层（投射物）
			
			GD.Print($"Monster {Name} collision setup complete");
			
			_target = GetTree().GetFirstNodeInGroup("Player") as Game.Player;
			AddToGroup("Monsters");
			
			// 添加攻击区域
			CreateAttackArea();
			
			GD.Print($"=== Monster {Name} Initialization Complete ===");
		}

		private void CreateAttackArea()
		{
			_attackArea = new Area2D();
			_attackArea.CollisionLayer = 0;
			_attackArea.CollisionMask = 1; // 只检测玩家层

			var shape = new CollisionShape2D();
			var circle = new CircleShape2D();
			circle.Radius = AttackRange;
			shape.Shape = circle;
			
			_attackArea.AddChild(shape);
			AddChild(_attackArea);
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			UpdateDotEffects((float)delta);
		}

		private void UpdateDotEffects(float delta)
		{
			// 更新所有持续伤害效果
			for (int i = _dotEffects.Count - 1; i >= 0; i--)
			{
				var effect = _dotEffects[i];
				effect.Update(delta);

				// 应用伤害
				TakeDamage(effect.DamagePerSecond * delta);

				// 移除过期的效果
				if (effect.RemainingDuration <= 0)
				{
					_dotEffects.RemoveAt(i);
				}
			}
		}

		public void ApplyDotDamage(float damagePerSecond, float duration)
		{
			_dotEffects.Add(new DotEffect(damagePerSecond, duration));
			GD.Print($"对 {Name} 施加持续伤害效果: {damagePerSecond}/秒, 持续{duration}秒");
		}

		public void UpdateAI(Game.Player target, float delta)
		{
			_target = target;
			
			if (_target != null)
			{
				var distance = GlobalPosition.DistanceTo(_target.GlobalPosition);
				
				if (distance <= AttackRange)
				{
					Attack(_target);
				}
				else
				{
					// 移动向玩家
					var direction = (_target.GlobalPosition - GlobalPosition).Normalized();
					Velocity = direction * MoveSpeed;
					MoveAndSlide();
				}
			}
		}

		public override void TakeDamage(float damage)
		{
			GD.Print($"=== Monster {Name} TakeDamage Start ===");
			GD.Print($"Monster {Name} receiving damage: {damage}");
			GD.Print($"Monster {Name} current health before damage: {CurrentHealth}/{MaxHealth}");
			
			base.TakeDamage(damage);  // 让基类处理伤害计算
			
			GD.Print($"Monster {Name} health after damage: {CurrentHealth}/{MaxHealth}");
			GD.Print($"=== Monster {Name} TakeDamage End ===");
		}

		public override void Die()
		{
			GD.Print($"=== Monster Death Process Start ===");
			GD.Print($"Monster {Name} at {GlobalPosition} is dying...");
			GD.Print($"Monster final health: {CurrentHealth}/{MaxHealth}");
			
			// 掉落金币
			try 
			{
				// 创建金币掉落物
				var goldDrop = new Game.Items.GoldDrop();  // 使用完整的命名空间
				int amount = (int)GD.RandRange(5, 15);
				goldDrop.Amount = amount;
				goldDrop.GlobalPosition = GlobalPosition;
				
				// 确保添加到正确的场景中
				var currentScene = GetTree().CurrentScene;
				GD.Print($"Current scene: {currentScene?.Name ?? "null"}");
				GD.Print($"Current scene path: {currentScene?.GetPath() ?? "null"}");
				
				if (currentScene != null)
				{
					// 尝试获取Monsters节点
					var monstersNode = GetParent();
					GD.Print($"Monsters node: {monstersNode?.Name ?? "null"}");
					GD.Print($"Monsters node path: {monstersNode?.GetPath() ?? "null"}");
					
					if (monstersNode != null)
					{
						GD.Print($"Adding GoldDrop to parent node: {monstersNode.Name}");
						monstersNode.AddChild(goldDrop);
						GD.Print($"GoldDrop added to {monstersNode.Name}");
					}
					else
					{
						GD.Print($"Adding GoldDrop to current scene: {currentScene.Name}");
						currentScene.AddChild(goldDrop);
						GD.Print($"GoldDrop added to {currentScene.Name}");
					}
					
					GD.Print($"Monster {Name} dropped {amount} gold at position {GlobalPosition}");
					GD.Print($"GoldDrop node path: {goldDrop.GetPath()}");
					GD.Print($"GoldDrop scene tree status: {goldDrop.IsInsideTree()}");
				}
				else
				{
					GD.PrintErr("Failed to drop gold: Current scene is null!");
				}
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error dropping gold: {e.Message}");
				GD.PrintErr($"Stack trace: {e.StackTrace}");
			}
			
			GD.Print("=== Calling base.Die() ===");
			base.Die();
			GD.Print("=== Monster Death Process End ===");
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_currentState == State.Dead) return;

			// 更新攻击冷却
			if (!_canAttack)
			{
				_attackTimer += (float)delta;
				if (_attackTimer >= AttackCooldown)
				{
					_canAttack = true;
					_attackTimer = 0;
				}
			}

			UpdateState();
			UpdateBehavior(delta);
		}

		private void UpdateState()
		{
			if (_target == null) return;

			var distanceToTarget = GlobalPosition.DistanceTo(_target.GlobalPosition);

			if (distanceToTarget <= AttackRange && _canAttack)
			{
				_currentState = State.Attack;
			}
			else if (distanceToTarget <= DetectionRange)
			{
				_currentState = State.Chase;
			}
			else
			{
				_currentState = State.Idle;
			}
		}

		private void UpdateBehavior(double delta)
		{
			switch (_currentState)
			{
				case State.Idle:
					// 待机状态 - 可以添加随机徘徊
					Velocity = Vector2.Zero;
					break;

				case State.Chase:
					// 追击玩家 - 添加预测和避障
					ChaseTarget(delta);
					break;

				case State.Attack:
					// 攻击玩家
					Attack();
					break;
			}

			MoveAndSlide();
		}

		private void ChaseTarget(double delta)
		{
			if (_target == null) return;

			// 计算到目标的方向
			var direction = (_target.GlobalPosition - GlobalPosition).Normalized();
			
			// 检查前方是否有障碍物
			var spaceState = GetWorld2D().DirectSpaceState;
			var query = PhysicsRayQueryParameters2D.Create(
				GlobalPosition,
				GlobalPosition + direction * DetectionRange,
				CollisionMask
			);
			var result = spaceState.IntersectRay(query);

			// 如果有障碍物，尝试绕过
			if (result.Count > 0)
			{
				// 简单的避障 - 可以改进为更复杂的寻路
				var normal = result["normal"].AsVector2();
				direction = (direction + normal).Normalized();
			}

			Velocity = direction * MoveSpeed;
		}

		private void Attack()
		{
			if (!_canAttack) return;

			// 获取攻击范围内的物体
			var overlappingBodies = _attackArea.GetOverlappingBodies();
			foreach (var body in overlappingBodies)
			{
				if (body is Game.Player player)
				{
					// 对玩家造成伤害
					player.TakeDamage(AttackDamage);
					break;
				}
			}

			// 开始攻击冷却
			_canAttack = false;
			_attackTimer = 0;

			// 播放攻击动画
			PlayAttackAnimation();
		}

		private void PlayAttackAnimation()
		{
			// 创建简单的攻击动画效果
			var tween = CreateTween();
			tween.TweenProperty(this, "modulate", new Color(1, 0, 0), 0.1f);
			tween.TweenProperty(this, "modulate", new Color(1, 1, 1), 0.1f);
		}

		// ... 添加其他新功能 ...
	}
} 
