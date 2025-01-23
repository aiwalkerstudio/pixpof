using Godot;

namespace Game.Enemies
{
    public partial class Monster : Enemy
    {
        [Export]
        public float MoveSpeed { get; set; } = 100.0f;
        
        [Export]
        public float DetectionRange { get; set; } = 200.0f;
        
        private Node2D _target;

        public override void _Ready()
        {
            base._Ready();
            MaxHealth = 50.0f; // 设置怪物的具体生命值
            
            // 寻找玩家作为目标
            _target = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        }

        protected override void UpdateAI(double delta)
        {
            if (_target != null)
            {
                // 简单的追踪AI
                float distance = GlobalPosition.DistanceTo(_target.GlobalPosition);
                
                if (distance <= DetectionRange)
                {
                    Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
                    Velocity = direction * MoveSpeed;
                }
                else
                {
                    Velocity = Vector2.Zero;
                }
                
                MoveAndSlide();
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