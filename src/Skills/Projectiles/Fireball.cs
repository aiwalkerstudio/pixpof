using Godot;

public partial class Fireball : Area2D
{
    [Export]
    public float Speed { get; set; } = 300.0f;
    
    [Export]
    public float Damage { get; set; } = 20.0f;
    
    private Vector2 _direction = Vector2.Right;
    private bool _isMultishot = false;
    
    public override void _Ready()
    {
        // 连接信号
        BodyEntered += OnBodyEntered;
        GetNode<Timer>("LifetimeTimer").Timeout += QueueFree;
    }

    public override void _Process(double delta)
    {
        Position += _direction * Speed * (float)delta;
    }

    public void Initialize(Vector2 direction, bool isMultishot = false)
    {
        _direction = direction.Normalized();
        _isMultishot = isMultishot;
        
        if (_isMultishot)
        {
            // 分裂火球伤害减少
            Damage *= 0.7f;
        }
        
        // 设置旋转
        Rotation = _direction.Angle();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Monster monster)
        {
            // 对怪物造成伤害
            monster.TakeDamage(Damage);
            
            // 播放命中特效
            PlayHitEffect();
            
            // 销毁火球
            QueueFree();
        }
    }

    private void PlayHitEffect()
    {
        // TODO: 添加命中特效
        GD.Print($"火球命中！造成{Damage}点伤害");
    }
} 