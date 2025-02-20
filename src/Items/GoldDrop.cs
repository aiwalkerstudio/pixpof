using Godot;

namespace Game.Items
{
	public partial class GoldDrop : Area2D
	{
		public int Amount { get; set; }
		private bool _isCollected = false;

		private float _attractRadius = 100.0f;  // 吸引范围
		private float _attractSpeed = 200.0f;   // 吸引速度
		private Game.Player _player;
		private bool _isAttracting = false;

		public override void _Ready()
		{
			// 设置碰撞
			CollisionLayer = 16;  // 第5层，用于物品
			CollisionMask = 1;    // 第1层，检测玩家
			
			// 延迟添加碰撞形状
			CallDeferred(nameof(SetupCollision));
			
			// 添加视觉效果
			SetupVisuals();
			
			// 连接信号
			BodyEntered += OnBodyEntered;
			
			GD.Print($"GoldDrop created: Amount={Amount}, Position={GlobalPosition}");
		}

		private void SetupCollision()
		{
			// 添加碰撞形状
			var shape = new CircleShape2D();
			shape.Radius = 16f;
			var collision = new CollisionShape2D();
			collision.Shape = shape;
			AddChild(collision);
		}

		private void SetupVisuals()
		{
			// 使用Label显示金币表情
			var label = new Label();
			label.Text = "💰";  // 使用金币袋表情
			
			// 设置字体大小
			var fontSettings = new SystemFont();
			label.AddThemeFontSizeOverride("font_size", 32);
			
			// 居中对齐
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.VerticalAlignment = VerticalAlignment.Center;
			
			// 设置位置偏移
			label.Position = new Vector2(-16, -24);
			
			AddChild(label);
			
			// 添加发光效果
			var glow = new Label();
			glow.Text = label.Text;
			glow.AddThemeFontSizeOverride("font_size", 40);
			glow.Modulate = new Color(1, 1, 0, 0.3f);
			glow.Position = label.Position - new Vector2(4, 4);
			glow.ZIndex = -1;
			
			AddChild(glow);
			
			// 添加动画
			var tween = CreateTween();
			tween.SetLoops();
			
			// 上下浮动动画
			tween.TweenProperty(this, "position:y", Position.Y - 5, 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);
			tween.TweenProperty(this, "position:y", Position.Y + 5, 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);
			
			// 发光效果动画
			var glowTween = CreateTween();
			glowTween.SetLoops();
			glowTween.TweenProperty(glow, "modulate:a", 0.1f, 1.0f)
				.SetTrans(Tween.TransitionType.Sine);
			glowTween.TweenProperty(glow, "modulate:a", 0.3f, 1.0f)
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
			
			// 在吸引范围内时移动向玩家
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
				GD.Print($"GoldDrop detected collision with: {body.Name}");
				GD.Print($"Player collected gold: {Amount}");
				
				// 将 AddGold 改为 CollectGold
				player.CollectGold(Amount);
				
				// 移除金币
				QueueFree();
			}
		}
		
		private void PlayCollectAnimation()
		{
			// 创建一个更华丽的收集动画
			var tween = CreateTween();
			
			// 向上飘动
			tween.Parallel().TweenProperty(this, "position:y", Position.Y - 50, 0.3f)
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.Out);
			
			// 缩小消失
			tween.Parallel().TweenProperty(this, "scale", Vector2.Zero, 0.3f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.In);
			
			// 淡出
			tween.Parallel().TweenProperty(this, "modulate:a", 0.0f, 0.3f);
			
			// 动画结束后删除
			tween.TweenCallback(Callable.From(() => QueueFree()));
		}
	}
} 
