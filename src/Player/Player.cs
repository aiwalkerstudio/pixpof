using Godot;
using Game.Skills;

namespace Game
{
	public partial class Player : CharacterBody2D
	{
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
		
		private float _currentHealth;
		private float _currentEnergyShield;
		private float _currentMana;  // 当前魔法值
		private float _manaRegenRate = 5.0f;  // 每秒魔法恢复速度
		private SkillSlot _skillSlot;

		public override void _Ready()
		{
			AddToGroup("Player");
			_currentHealth = MaxHealth;
			_currentEnergyShield = MaxEnergyShield;
			_currentMana = MaxMana;  // 初始化魔法值
			
			// 获取技能槽并添加错误检查
			_skillSlot = GetNode<SkillSlot>("SkillSlot");
			if (_skillSlot == null)
			{
				GD.PrintErr("SkillSlot node not found! Make sure the SkillSlot node exists in the Player scene.");
			}
			else
			{
				GD.Print("SkillSlot initialized successfully.");
			}
		}

		public override void _PhysicsProcess(double delta)
		{
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
			_currentMana = Mathf.Min(_currentMana + _manaRegenRate * delta, MaxMana);
		}

		public bool ConsumeMana(float amount)
		{
			if (_currentMana >= amount)
			{
				_currentMana -= amount;
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
				battleUI.UpdateHealth(_currentHealth, MaxHealth);
				battleUI.UpdateMana(_currentMana, MaxMana);  // 更新魔法值显示
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
				float oldHealth = _currentHealth;
				_currentHealth = Mathf.Max(0, _currentHealth - damage);
				
				//GD.Print($"Player受到{damage}点伤害! 血量: {oldHealth} -> {_currentHealth}");
				
				// 添加空检查
				if (_skillSlot == null)
				{
					GD.PrintErr("Cannot trigger skill: SkillSlot is null!");
					return;
				}
				
				// 检查是否触发受伤技能
				if (damage >= OnHitSkillThreshold)
				{
					GD.Print($"伤害({damage})超过阈值({OnHitSkillThreshold})，触发受伤技能!");
					_skillSlot.OnHit(this);
				}
				else
				{
					//GD.Print($"伤害({damage})未达到阈值({OnHitSkillThreshold})，不触发技能");
				}

				// 更新UI显示
				UpdateHealthUI();

				// 检查死亡
				if (_currentHealth <= 0)
				{
					Die();
				}
			}

			// 触发受伤事件
			_skillSlot?.OnHit(this);
			
			GD.Print($"Player受到{damage}点伤害，当前生命值: {_currentHealth}, 能量护盾: {_currentEnergyShield}");
		}

		private void UpdateHealthUI()
		{
			// TODO: 更新血量UI显示
			GD.Print($"Player Health: {_currentHealth}/{MaxHealth}");
		}

		private void Die()
		{
			// TODO: 处理玩家死亡
			GD.Print("Player Died!");
		}

		public void OnAttackPressed()
		{
			// 处理普通攻击
			GD.Print("Player Attack!");
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
			if (@event.IsActionPressed("skill_1"))
			{
				GD.Print("按下技能1键");
				OnSkillPressed(0);
			}
			else if (@event.IsActionPressed("skill_2"))
			{
				GD.Print("按下技能2键");
				OnSkillPressed(1);
			}
			else if (@event.IsActionPressed("skill_3"))
			{
				GD.Print("按下技能3键");
				OnSkillPressed(2);
			}
		}
	}
} 
