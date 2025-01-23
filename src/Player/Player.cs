using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 300.0f;

	[Export]
	public float MaxHealth { get; set; } = 100.0f;
	
	[Export]
	public float OnHitSkillThreshold { get; set; } = 10.0f; // 触发受伤技能的伤害阈值

	private float _currentHealth;
	private SkillSlot _skillSlot;

	public override void _Ready()
	{
		AddToGroup("Player");
		_currentHealth = MaxHealth;
		
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

	// 处理受到伤害
	public void TakeDamage(float damage)
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
		_skillSlot.TriggerSkill(skillIndex, this);
	}
} 
