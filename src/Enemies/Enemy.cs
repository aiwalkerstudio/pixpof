using Godot;

namespace Game.Enemies
{
    public abstract partial class Enemy : CharacterBody2D
    {
        [Export]
        public float MaxHealth { get; set; } = 100.0f;
        
        protected float _currentHealth;
        protected bool _isDead;

        public override void _Ready()
        {
            _currentHealth = MaxHealth;
            _isDead = false;
        }

        public virtual void TakeDamage(float damage)
        {
            if (_isDead) return;

            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            
            GD.Print($"{GetType().Name} took {damage} damage! Health: {oldHealth} -> {_currentHealth}");
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public virtual void ApplyDotDamage(float damagePerSecond, float duration)
        {
            // 基础DoT系统 - 子类可以覆盖实现更复杂的效果
            TakeDamage(damagePerSecond * duration);
        }

        protected virtual void Die()
        {
            if (_isDead) return;
            _isDead = true;
            
            GD.Print($"{GetType().Name} died!");
            QueueFree(); // 子类可以覆盖此方法来添加死亡动画等
        }

        // 基础AI行为 - 子类必须实现
        protected abstract void UpdateAI(double delta);
        
        public override void _PhysicsProcess(double delta)
        {
            if (!_isDead)
            {
                UpdateAI(delta);
            }
        }
    }
} 