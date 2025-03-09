using Godot;
using Game.Skills;
using Game.Enemies;
using Game.UI.Battle;
using Game.Classes;

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
			set
			{
				if (_gold != value)
				{
					_gold = value;
					EmitSignal(SignalName.GoldChanged, _gold);
					GD.Print($"Player Gold changed: {_gold}, emitting GoldChanged signal");
				}
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
			set
			{
				_currentHealth = value;
				// 可以在这里添加生命值改变的事件
			}
		}

		public float CurrentMana
		{
			get => _currentMana;
			set
			{
				_currentMana = value;
				// 可以在这里添加魔法值改变的事件
			}
		}

		// 基础属性
		private BaseClass _class;
		private float _baseMoveSpeed = 200f;
		private float _moveSpeed;
		private int _strength;
		private int _agility;
		private int _intelligence;

		// 属性访问器
		public float BaseMoveSpeed => _baseMoveSpeed;
		public float MoveSpeed 
		{ 
			get => _moveSpeed;
			set => _moveSpeed = value;
		}
		public int Strength 
		{ 
			get => _strength;
			set => _strength = value;
		}
		public int Agility 
		{ 
			get => _agility;
			set => _agility = value;
		}
		public int Intelligence 
		{ 
			get => _intelligence;
			set => _intelligence = value;
		} 

		[Signal]
		public delegate void HealthChangedEventHandler(float currentHealth, float maxHealth);

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

			// 设置默认职业为穷鬼
			_class = new Cracker();
			_class.Initialize(this);
			
			// 初始化移动速度
			_moveSpeed = _baseMoveSpeed;
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
			// 更新职业机制
			_class?.Update(this, delta);
			
			// 处理移动
			HandleMovement();
			
			// 更新表情动画
			UpdateEmojiAnimation((float)delta);
		}
		
		private void HandleMovement()
		{
			Vector2 velocity = Vector2.Zero;
			
			// 检查输入映射是否存在
			if (!InputMap.HasAction("move_right"))
			{
				GD.Print("添加移动输入映射...");
				// 添加默认的移动输入映射
				AddDefaultInputMappings();
			}
			
			// 记录输入状态
			bool right = Input.IsActionPressed("move_right");
			bool left = Input.IsActionPressed("move_left");
			bool down = Input.IsActionPressed("move_down");
			bool up = Input.IsActionPressed("move_up");
			
			if (GD.Randi() % 60 == 0) // 每秒左右打印一次
			{
				GD.Print($"移动输入状态: 右={right}, 左={left}, 下={down}, 上={up}");
			}
			
			if (right)
				velocity.X += 1;
			if (left)
				velocity.X -= 1;
			if (down)
				velocity.Y += 1;
			if (up)
				velocity.Y -= 1;

			if (velocity != Vector2.Zero)
			{
				velocity = velocity.Normalized() * _moveSpeed;
				//GD.Print($"计算移动速度: 方向={velocity.Normalized()}, 速度={_moveSpeed}, 最终速度={velocity}");
			}
			
			Velocity = velocity;
			MoveAndSlide();
		}

		private void AddDefaultInputMappings()
		{
			// 添加默认的移动输入映射
			if (!InputMap.HasAction("move_right"))
			{
				InputMap.AddAction("move_right");
				InputMap.ActionAddEvent("move_right", new InputEventKey { PhysicalKeycode = Key.Right });
			}
			
			if (!InputMap.HasAction("move_left"))
			{
				InputMap.AddAction("move_left");
				InputMap.ActionAddEvent("move_left", new InputEventKey { PhysicalKeycode = Key.Left });
			}
			
			if (!InputMap.HasAction("move_down"))
			{
				InputMap.AddAction("move_down");
				InputMap.ActionAddEvent("move_down", new InputEventKey { PhysicalKeycode = Key.Down });
			}
			
			if (!InputMap.HasAction("move_up"))
			{
				InputMap.AddAction("move_up");
				InputMap.ActionAddEvent("move_up", new InputEventKey { PhysicalKeycode = Key.Up });
			}
			
			GD.Print("已添加默认移动输入映射: 方向键");
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

			EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
		}

		private void UpdateHealthUI()
		{
			// TODO: 更新血量UI显示
			//GD.Print($"Player Health: {CurrentHealth}/{MaxHealth}");
		}

		private void Die()
		{
			_class?.OnDeath(this);
			
			// 发送死亡信号给战斗场景
			EmitSignal(SignalName.HealthChanged, 0, MaxHealth);
			
			// 隐藏表情
			if (_playerEmoji != null)
			{
				_playerEmoji.QueueFree();
				_playerEmoji = null;
			}
			
			// 通知父节点玩家死亡
			GetParent()?.Call("OnPlayerDied");
			
			// 清理自身
			QueueFree();
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
			// 调用AddGold方法保持功能一致
			AddGold(amount);
		}

		public void AddGold(int amount)
		{
			if(_class is Cracker cracker)
			{
				amount = (int)cracker.GetGoldBonus(amount);
			}
			Gold += amount;  // 使用属性以触发信号
			GD.Print($"获得金币: {amount}, 当前总金币: {_gold}");
		}

		public void Heal(float amount)
		{
			_currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
			EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
		}
	}
}
