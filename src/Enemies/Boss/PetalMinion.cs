using Godot;

namespace Game.Enemies.Boss
{
    public partial class PetalMinion : Enemy
    {
        [Export]
        public float MoveSpeed { get; set; } = 120.0f;
        
        [Export]
        public float AttackRange { get; set; } = 100.0f;
        
        private Game.Player _target;
        private float _attackCooldown = 2.0f;
        private float _currentCooldown = 0f;

        public override void _Ready()
        {
            base._Ready();
            MaxHealth = 50.0f;
            AttackDamage = 10.0f;
            CurrentHealth = MaxHealth;
            
            // 设置外观
            var sprite = new ColorRect();
            sprite.Color = new Color(0.8f, 0.4f, 0.8f);  // 浅粉色
            sprite.Size = new Vector2(24, 24);
            sprite.Position = new Vector2(-12, -12);
            AddChild(sprite);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_target == null)
            {
                _target = GetNode<Game.Player>("/root/Main/Player");
                if (_target == null) return;
            }

            UpdateAI((float)delta);
        }

        private void UpdateAI(float delta)
        {
            if (_currentCooldown > 0)
            {
                _currentCooldown -= delta;
            }

            float distanceToPlayer = GlobalPosition.DistanceTo(_target.GlobalPosition);
            
            if (distanceToPlayer <= AttackRange && _currentCooldown <= 0)
            {
                Attack(_target);
                _currentCooldown = _attackCooldown;
            }
            else if (distanceToPlayer > AttackRange)
            {
                Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
                Velocity = direction * MoveSpeed;
                MoveAndSlide();
            }
        }
    }
} 