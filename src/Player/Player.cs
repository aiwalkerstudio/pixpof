using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 300.0f;

	public override void _Ready()
	{
		AddToGroup("Player");
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

	public void OnAttackPressed()
	{
		// TODO: 实现攻击逻辑
		GD.Print("Player Attack!");
	}

	public void OnSkillPressed(int skillIndex)
	{
		// TODO: 实现技能逻辑
		GD.Print($"Player Use Skill {skillIndex}!");
	}
} 
