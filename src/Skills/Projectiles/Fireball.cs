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
		
		// 确保火球在顶层显示
		ZIndex = 10;
		
		// 调试信息
		GD.Print($"火球创建成功! 位置: {GlobalPosition}, 方向: {_direction}");
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
		
		// 调试信息
		GD.Print($"火球初始化: 位置={GlobalPosition}, 方向={_direction}, 多重={_isMultishot}");
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
		// 创建爆炸效果
		var explosion = new Node2D();
		var circle = new ColorRect();
		circle.Color = new Color(1, 0.5f, 0, 0.8f);
		circle.Size = new Vector2(32, 32);
		circle.Position = -circle.Size / 2;
		
		explosion.AddChild(circle);
		explosion.GlobalPosition = GlobalPosition;
		
		GetTree().Root.AddChild(explosion);
		
		// 创建爆炸动画
		var tween = explosion.CreateTween();
		tween.TweenProperty(circle, "size", circle.Size * 2, 0.2f);
		tween.Parallel().TweenProperty(circle, "modulate:a", 0.0f, 0.2f);
		tween.TweenCallback(Callable.From(() => explosion.QueueFree()));
		
		GD.Print($"火球命中！造成{Damage}点伤害");
	}
} 
