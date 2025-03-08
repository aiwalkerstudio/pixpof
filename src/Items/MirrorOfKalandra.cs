using Godot;

namespace Game.Items
{
	public partial class MirrorOfKalandra : Area2D
	{
		private float _attractRadius = 100.0f;
		private float _attractSpeed = 200.0f;
		private Game.Player _player;
		private bool _isAttracting = false;

		public override void _Ready()
		{
			// 设置碰撞
			CollisionLayer = 16;  // 第5层，用于物品
			CollisionMask = 1;    // 第1层，检测玩家
			
			// 设置碰撞形状
			var shape = new CircleShape2D();
			shape.Radius = 16f;
			var collision = new CollisionShape2D();
			collision.Shape = shape;
			AddChild(collision);
			
			// 添加视觉效果
			SetupVisuals();
			
			// 连接信号
			BodyEntered += OnBodyEntered;
		}

		private void SetupVisuals()
		{
			// 使用Sprite2D显示镜子图片
			var sprite = new Sprite2D();
			
			// 加载并设置纹理
			var texture = ResourceLoader.Load<Texture2D>("res://sprites/items/MirrorOfKalandra.png");
			if (texture != null)
			{
				sprite.Texture = texture;
				GD.Print("成功加载 MirrorOfKalandra 图片");
			}
			else
			{
				GD.PrintErr("无法加载 MirrorOfKalandra 图片");
			}
			
			// 调整精灵大小 - 调大一点更容易看到
			sprite.Scale = new Vector2(0.8f, 0.8f);
			
			// 调整位置偏移 - 使其更容易看到
			sprite.Position = new Vector2(0, -32);
			
			sprite.ZIndex = 1; // 确保在其他物体上面
			
			AddChild(sprite);
			
			// 添加浮动动画
			var tween = CreateTween();
			tween.SetLoops();
			tween.TweenProperty(this, "position:y", Position.Y - 5, 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);
			tween.TweenProperty(this, "position:y", Position.Y + 5, 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);
			
			// 发光动画
			var glowTween = CreateTween();
			glowTween.SetLoops();
			glowTween.TweenProperty(sprite, "modulate:a", 0.5f, 1.0f)
				.SetTrans(Tween.TransitionType.Sine);
			glowTween.TweenProperty(sprite, "modulate:a", 1.0f, 1.0f)
				.SetTrans(Tween.TransitionType.Sine);
		}

		public override void _Process(double delta)
		{
			if (_player == null)
			{
				_player = GetNode<Game.Player>("/root/Main/Player");
				return;
			}

			float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
			
			if (distance <= _attractRadius || _isAttracting)
			{
				_isAttracting = true;
				Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
				GlobalPosition += direction * _attractSpeed * (float)delta;
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body is Game.Player player)
			{
				GD.Print("玩家获得了 Mirror of Kalandra!");
				// TODO: 实现镜子效果
				PlayCollectAnimation();
			}
		}
		
		private void PlayCollectAnimation()
		{
			var tween = CreateTween();
			
			tween.Parallel().TweenProperty(this, "position:y", Position.Y - 50, 0.3f)
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.Out);
			
			tween.Parallel().TweenProperty(this, "scale", Vector2.Zero, 0.3f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.In);
			
			tween.Parallel().TweenProperty(this, "modulate:a", 0.0f, 0.3f);
			
			tween.TweenCallback(Callable.From(() => QueueFree()));
		}
	}
} 
