using Godot;

namespace Game.Enemies
{
	public abstract partial class Enemy : CharacterBody2D
	{
		[Signal]
		public delegate void DiedEventHandler(Enemy enemy);
		
		[Export]
		public float MaxHealth { get; set; } = 100.0f;
		
		[Export]
		public float AttackDamage { get; set; } = 15.0f;
		
		[Export]
		public float AttackRange { get; set; } = 50.0f;
		
		protected float CurrentHealth { get; set; }
		
		public override void _Ready()
		{
			GD.Print($"=== Enemy {Name} Initializing ===");
			CurrentHealth = MaxHealth;
			GD.Print($"Enemy {Name} initial health: {CurrentHealth}/{MaxHealth}");
			GD.Print($"=== Enemy {Name} Initialization Complete ===");
		}
		
		public virtual void TakeDamage(float damage)
		{
			GD.Print($"=== Enemy TakeDamage Start ===");
			GD.Print($"Enemy {Name} receiving damage: {damage}");
			GD.Print($"Enemy {Name} current health before damage: {CurrentHealth}/{MaxHealth}");
			
			CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
			
			GD.Print($"Enemy {Name} health after damage: {CurrentHealth}/{MaxHealth}");
			
			if (CurrentHealth <= 0)
			{
				GD.Print($"Enemy {Name} health depleted, calling Die()");
				Die();
			}
			
			GD.Print($"=== Enemy TakeDamage End ===");
		}
		
		public virtual void Die()
		{
			GD.Print($"=== Enemy Die Start ===");
			GD.Print($"Enemy {Name} Die() called");
			GD.Print($"Enemy {Name} final health: {CurrentHealth}/{MaxHealth}");
			EmitSignal(SignalName.Died, this);
			QueueFree();
			GD.Print($"=== Enemy Die End ===");
		}
		
		protected virtual void Attack(Node2D target)
		{
			// 基类提供基本的攻击实现
			if (target is Game.Player player)
			{
				player.TakeDamage(AttackDamage);
			}
		}
		
		protected virtual void UpdateAI(double delta)
		{
			// 子类实现具体的AI逻辑
		}
	}
} 
