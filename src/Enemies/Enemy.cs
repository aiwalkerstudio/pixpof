using Godot;

namespace Game.Enemies
{
    public abstract partial class Enemy : CharacterBody2D
    {
        [Export]
        public float MaxHealth { get; set; } = 100.0f;
        
        [Export]
        public float AttackDamage { get; set; } = 15.0f;
        
        [Export]
        public float AttackRange { get; set; } = 50.0f;
        
        protected float CurrentHealth { get; set; }
        
        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
        }
        
        public virtual void TakeDamage(float amount)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            if (CurrentHealth <= 0)
            {
                Die();
            }
        }
        
        protected virtual void Die()
        {
            QueueFree();
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