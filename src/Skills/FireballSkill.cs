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
			GD.Print($"开始释放{Name}!");
			CurrentCooldown = Cooldown;

			// 获取鼠标位置作为目标方向
			var mousePos = player.GetGlobalMousePosition();
			var direction = (mousePos - player.GlobalPosition).Normalized();

			// 对于受伤触发，使用最近的怪物作为目标
			if (source is Player && TriggerType == SkillTriggerType.OnHit)
			{
				var nearestMonster = FindNearestMonster(player);
				if (nearestMonster != null)
				{
					direction = (nearestMonster.GlobalPosition - player.GlobalPosition).Normalized();
					GD.Print($"找到最近的怪物，发射方向: {direction}");
				}
				else
				{
					GD.Print("未找到目标怪物，使用默认方向");
					direction = Vector2.Right; // 默认向右发射
				}
			}

			if (_isMultishot)
			{
				GD.Print("使用多重投射模式");
				ShootMultipleFireballs(player, player.GlobalPosition, direction);
			}
			else
			{
				GD.Print("使用单发模式");
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

	private Node2D FindNearestMonster(Node2D source)
	{
		Node2D nearestMonster = null;
		float nearestDistance = float.MaxValue;

		// 获取所有怪物
		var monsters = source.GetTree().GetNodesInGroup("Monsters");
		foreach (Node monster in monsters)
		{
			if (monster is Node2D monster2D)
			{
				float distance = source.GlobalPosition.DistanceTo(monster2D.GlobalPosition);
				if (distance < nearestDistance)
				{
					nearestDistance = distance;
					nearestMonster = monster2D;
				}
			}
		}

		return nearestMonster;
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
