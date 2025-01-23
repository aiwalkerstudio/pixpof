using Godot;
using System;
using System.Collections.Generic;
using Game;

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
			base._Ready();
			MaxHealth = 50.0f; // 设置怪物的具体生命值
			
			// 寻找玩家作为目标
			_target = GetTree().GetFirstNodeInGroup("Player") as Game.Player;
			AddToGroup("Monsters");
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

		protected override void UpdateAI(double delta)
		{
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

		protected override void Die()
		{
			// 在死亡前生成掉落物
			SpawnLoot();
			
			// 调用基类的死亡处理
			base.Die();
		}

		private void SpawnLoot()
		{
			// TODO: 实现掉落物生成逻辑
			GD.Print($"Monster dropped loot at {GlobalPosition}");
		}
	}
} 
