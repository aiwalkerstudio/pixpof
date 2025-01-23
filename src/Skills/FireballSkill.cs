using Godot;

public partial class FireballSkill : Skill
{
	private PackedScene _fireballScene;
	private bool _isMultishot = false;
	
	public FireballSkill()
	{
		Name = "火球术";
		Description = "发射一颗火球,对敌人造成火焰伤害";
		Cooldown = 1.0f;
		IsPassive = false;
		TriggerType = SkillTriggerType.Manual;
		
		// 加载火球场景
		_fireballScene = GD.Load<PackedScene>("res://scenes/skills/Fireball.tscn");
	}

	public override void Trigger(Node source)
	{
		if (source is Player player)
		{
			GD.Print($"释放{Name}!");
			CurrentCooldown = Cooldown;

			// 获取鼠标位置作为目标方向
			var mousePos = player.GetGlobalMousePosition();
			var direction = (mousePos - player.GlobalPosition).Normalized();

			if (_isMultishot)
			{
				// 多重投射：发射3个火球
				ShootMultipleFireballs(player, player.GlobalPosition, direction);
			}
			else
			{
				// 发射单个火球
				ShootFireball(player, player.GlobalPosition, direction);
			}
		}
	}

	private void ShootFireball(Player player, Vector2 position, Vector2 direction)
	{
		var fireball = _fireballScene.Instantiate<Fireball>();
		player.GetParent().AddChild(fireball);
		fireball.GlobalPosition = position;
		fireball.Initialize(direction, _isMultishot);
	}

	private void ShootMultipleFireballs(Player player, Vector2 position, Vector2 direction)
	{
		// 发射3个火球，呈扇形分布
		float[] angles = { -15f, 0f, 15f }; // 角度偏移
		
		foreach (float angle in angles)
		{
			var rotatedDirection = direction.Rotated(Mathf.DegToRad(angle));
			ShootFireball(player, position, rotatedDirection);
		}
	}

	public void EnableMultishot()
	{
		_isMultishot = true;
	}

	public void DisableMultishot()
	{
		_isMultishot = false;
	}
} 
