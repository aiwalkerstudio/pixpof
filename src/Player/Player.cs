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
		_skillSlot = GetNode<SkillSlot>("SkillSlot");
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
		
		// 检查是否触发受伤技能
		if (damage >= OnHitSkillThreshold)
		{
			_skillSlot.OnHit(this);
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
