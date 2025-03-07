using Godot;
using Game.Skills;
using Game.Enemies;
using Game.UI.Battle;

namespace Game
{
	public partial class Player : CharacterBody2D
	{
		// 添加表情显示相关字段
		private Label _playerEmoji;
		private string _emojiText = "😄";
		private float _animationTime = 0f;

		[Export]
		public float Speed = 300.0f;

		[Export]
		public float MaxHealth { get; set; } = 100000.0f;
		
		[Export]
		public float OnHitSkillThreshold { get; set; } = 10.0f; // 触发受伤技能的伤害阈值

		[Export]
		public float MaxEnergyShield { get; set; } = 50.0f;
		
		[Export]
		public float MaxMana { get; set; } = 100.0f;  // 最大魔法值
		
		[Export]
		public int Gold
		{
			get => _gold;
			private set
			{
				_gold = value;
				EmitSignal(SignalName.GoldChanged, _gold);  // 在金币数量改变时发送信号
				GD.Print($"Player Gold changed: {_gold}, emitting GoldChanged signal");
			}
		}

		[Signal]
		public delegate void GoldChangedEventHandler(int newAmount);
		
		private float _currentHealth;
		private float _currentEnergyShield;
		private float _currentMana;  // 当前魔法值
		private float _manaRegenRate = 5.0f;  // 每秒魔法恢复速度
		private SkillSlot _skillSlot;
		private int _gold = 0;  // 添加私有字段

		// 添加公共属性
		public float CurrentHealth
		{
			get => _currentHealth;
			private set
			{
				_currentHealth = value;
				// 可以在这里添加生命值改变的事件
			}
		}

		public float CurrentMana
		{
			get => _currentMana;
			private set
			{
				_currentMana = value;
				// 可以在这里添加魔法值改变的事件
			}
		}

		public override void _Ready()
		{
			AddToGroup("Player");
			_currentHealth = MaxHealth;
			_currentEnergyShield = MaxEnergyShield;
			_currentMana = MaxMana;
			
			// 设置碰撞层
			CollisionLayer = 1;  // 第1层，玩家
			CollisionMask = 20;  // 可以与敌人（4）和物品（16）碰撞
			
			// 获取技能槽
			_skillSlot = GetNode<SkillSlot>("SkillSlot");
			if (_skillSlot == null)
			{
				GD.PrintErr("SkillSlot node not found!");
			}
			else
			{
				GD.Print("SkillSlot initialized successfully.");
			}
			
			// 创建表情符号显示
			SetupEmojiDisplay();
		}
		
		private void SetupEmojiDisplay()
		{
			_playerEmoji = new Label();
			_playerEmoji.Text = _emojiText;
			_playerEmoji.HorizontalAlignment = HorizontalAlignment.Center;
			_playerEmoji.VerticalAlignment = VerticalAlignment.Center;
			
			// 设置字体大小和颜色
			_playerEmoji.AddThemeFontSizeOverride("font_size", 32);
			_playerEmoji.AddThemeColorOverride("font_color", Colors.Yellow);
			
			// 设置位置
			_playerEmoji.Position = new Vector2(-16, -16);
			
			AddChild(_playerEmoji);
		}

		public override void _PhysicsProcess(double delta)
		{
			// 现有移动代码
			Vector2 velocity = Velocity;
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			
			if (direction != Vector2.Zero)
			{
				velocity = direction * Speed;
			}
			else
			{
				velocity = Vector2.Zero;
			}

			Velocity = velocity;
			MoveAndSlide();
			
			// 更新表情动画
			UpdateEmojiAnimation((float)delta);
		}
		
		private void UpdateEmojiAnimation(float delta)
		{
			_animationTime += delta;
			
			// 根据玩家状态调整表情
			if (Velocity.Length() > 0.1f)
			{
				// 移动时轻微上下跳动
				// float bounce = Mathf.Sin(_animationTime * 10) * 2;
				// _playerEmoji.Position = new Vector2(0, bounce);
			}
			else
			{
				// 静止时轻微呼吸效果
				float scale = 1.0f + 0.05f * Mathf.Sin(_animationTime * 2);
				_playerEmoji.Scale = new Vector2(_playerEmoji.Scale.X * Mathf.Sign(_playerEmoji.Scale.X), scale);
			}
			
			// 受伤时变红
			if (_currentHealth < MaxHealth * 0.3f)
			{
				_playerEmoji.Text = "😰"; // 低血量时改变表情
				_playerEmoji.Modulate = new Color(1, 0.5f + 0.5f * Mathf.Sin(_animationTime * 5), 0.5f);
			}
			else
			{
				_playerEmoji.Text = _emojiText;
				_playerEmoji.Modulate = Colors.White;
			}
		}

		public override void _Process(double delta)
		{
			// 魔法值自动恢复
			RegenerateMana((float)delta);
			
			// 更新UI显示
			UpdateUI();
		}

		private void RegenerateMana(float delta)
		{
			// 使用属性而不是字段
			CurrentMana = Mathf.Min(CurrentMana + _manaRegenRate * delta, MaxMana);
		}

		public bool ConsumeMana(float amount)
		{
			if (CurrentMana >= amount)
			{
				CurrentMana -= amount;
				UpdateUI();
				return true;
			}
			return false;
		}

		private void UpdateUI()
		{
			// 获取BattleUI实例
			var battleUI = GetNode<BattleUI>("/root/Main/UI/BattleUI");
			if (battleUI != null)
			{
				battleUI.UpdateHealth(CurrentHealth, MaxHealth);
				battleUI.UpdateMana(CurrentMana, MaxMana);  // 更新魔法值显示
			}
		}

		// 处理受到伤害
		public void TakeDamage(float damage)
		{
			// 优先消耗能量护盾
			if (_currentEnergyShield > 0)
			{
				float shieldDamage = Mathf.Min(damage, _currentEnergyShield);
				_currentEnergyShield -= shieldDamage;
				damage -= shieldDamage;
			}
			
			// 剩余伤害扣除生命值
			if (damage > 0)
			{
				float oldHealth = CurrentHealth;
				CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
				
				//GD.Print($"Player受到{damage}点伤害! 血量: {oldHealth} -> {CurrentHealth}");
				
				// 添加空检查
				if (_skillSlot == null)
				{
					GD.PrintErr("Cannot trigger skill: SkillSlot is null!");
					return;
				}
				
				// 检查是否触发受伤技能
				if (damage >= OnHitSkillThreshold)
				{
					//GD.Print($"伤害({damage})超过阈值({OnHitSkillThreshold})，触发受伤技能!");
					_skillSlot.OnHit(this);
				}
				else
				{
					//GD.Print($"伤害({damage})未达到阈值({OnHitSkillThreshold})，不触发技能");
				}

				// 更新UI显示
				UpdateHealthUI();

				// 检查死亡
				if (CurrentHealth <= 0)
				{
					Die();
				}
			}

			// 触发受伤事件
			_skillSlot?.OnHit(this);
			
			//GD.Print($"Player受到{damage}点伤害，当前生命值: {CurrentHealth}, 能量护盾: {_currentEnergyShield}");
		}

		private void UpdateHealthUI()
		{
			// TODO: 更新血量UI显示
			//GD.Print($"Player Health: {CurrentHealth}/{MaxHealth}");
		}

		private void Die()
		{
			// TODO: 处理玩家死亡
			// GD.Print("Player Died!");
		}

		public void OnAttackPressed()
		{
			GD.Print("Player Attack!");
			
			var spaceState = GetWorld2D().DirectSpaceState;
			var attackRange = 100.0f;
			var attackDamage = 20.0f;
			
			var query = new PhysicsShapeQueryParameters2D();
			var shape = new CircleShape2D();
			shape.Radius = attackRange;
			query.Shape = shape;
			query.Transform = new Transform2D(0, GlobalPosition);
			query.CollisionMask = 4; // 敌人层
			query.CollideWithBodies = true;
			query.CollideWithAreas = false;
			
			// 添加更多调试信息
			GD.Print($"Player position: {GlobalPosition}");
			GD.Print($"Attack query: Range={attackRange}, CollisionMask={query.CollisionMask}");
			GD.Print($"Query transform: {query.Transform}");
			
			// 临时添加可视化攻击范围
			var rangeIndicator = new ColorRect();
			rangeIndicator.Color = new Color(1, 0, 0, 0.2f);
			rangeIndicator.Size = new Vector2(attackRange * 2, attackRange * 2);
			rangeIndicator.Position = GlobalPosition - rangeIndicator.Size / 2;
			GetTree().CurrentScene.AddChild(rangeIndicator);
			
			var tween = CreateTween();
			tween.TweenProperty(rangeIndicator, "modulate:a", 0.0f, 0.3f);
			tween.TweenCallback(Callable.From(() => rangeIndicator.QueueFree()));
			
			var results = spaceState.IntersectShape(query);
			GD.Print($"Found {results.Count} potential targets");
			
			// 打印所有碰撞结果的详细信息
			foreach (var result in results)
			{
				var collider = result["collider"].As<Node2D>();
				var colliderPos = result["position"].AsVector2();
				GD.Print($"Collision result:");
				GD.Print($"  Type: {collider?.GetType().Name}");
				GD.Print($"  Position: {colliderPos}");
				GD.Print($"  Distance: {GlobalPosition.DistanceTo(colliderPos)}");
				//GD.Print($"  CollisionLayer: {collider?.GetType().Name == \"CharacterBody2D\" ? (collider as CharacterBody2D)?.CollisionLayer : \"Unknown\"}");
				
				if (collider is Monster monster)
				{
					GD.Print($"Player attacks monster at {monster.GlobalPosition}");
					monster.TakeDamage(attackDamage);
				}
				else if (collider is Enemy enemy)
				{
					GD.Print($"Player attacks enemy at {enemy.GlobalPosition}");
					enemy.TakeDamage(attackDamage);
				}
			}
		}

		public void OnSkillPressed(int skillIndex)
		{
			GD.Print($"Player尝试使用技能 {skillIndex}");
			if (_skillSlot != null)
			{
				_skillSlot.TriggerSkill(skillIndex, this);
			}
			else
			{
				GD.PrintErr("Player的SkillSlot为空!");
			}
		}

		public void AddEnergyShield(float amount)
		{
			_currentEnergyShield = Mathf.Min(_currentEnergyShield + amount, MaxEnergyShield);
			GD.Print($"Energy Shield: {_currentEnergyShield}/{MaxEnergyShield}");
		}

		public override void _Input(InputEvent @event)
		{
			// 使用已存在的输入动作或自定义按键检测
			if (@event is InputEventKey eventKey)
			{
				if (eventKey.Pressed)
				{
					switch (eventKey.Keycode)
					{
						case Key.Q:
							GD.Print("按下技能1键");
							OnSkillPressed(0);
							break;
						case Key.W:
							GD.Print("按下技能2键");
							OnSkillPressed(1);
							break;
						case Key.E:
							GD.Print("按下技能3键");
							OnSkillPressed(2);
							break;
					}
				}
			}
		}

		public void CollectGold(int amount)
		{
			Gold += amount;  // 使用属性来确保信号被发送
			GD.Print($"Player collected {amount} gold, total: {Gold}");
		}
	}
}
