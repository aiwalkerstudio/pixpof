using Godot;

namespace Game.Enemies.Boss
{
    public partial class PetalProjectile : Area2D
    {
        [Export]
        public float Speed { get; set; } = 200.0f;
        
        [Export]
        public float Damage { get; set; } = 15.0f;
        
        public Vector2 Direction { get; set; } = Vector2.Right;
        
        private float _lifetime = 3.0f;

        public override void _Ready()
        {
            // 设置碰撞
            CollisionLayer = 4;  // 敌人层
            CollisionMask = 1;   // 玩家层
            
            // 添加碰撞形状
            var shape = new CircleShape2D();
            shape.Radius = 8f;
            var collision = new CollisionShape2D();
            collision.Shape = shape;
            AddChild(collision);
            
            // 添加视觉效果
            var sprite = new ColorRect();
            sprite.Color = new Color(0.8f, 0.2f, 0.8f);  // 粉色
            sprite.Size = new Vector2(16, 16);
            sprite.Position = new Vector2(-8, -8);
            AddChild(sprite);
            
            // 连接信号
            BodyEntered += OnBodyEntered;
        }

        public override void _Process(double delta)
        {
            _lifetime -= (float)delta;
            if (_lifetime <= 0)
            {
                QueueFree();
                return;
            }
            
            Position += Direction * Speed * (float)delta;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body is Game.Player player)
            {
                player.TakeDamage(Damage);
                QueueFree();
            }
        }
    }
} 