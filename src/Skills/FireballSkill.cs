using Godot;
using Game.Skills;

namespace Game.Skills
{
	public class FireballSkill : ProjectileSkill
	{
		private PackedScene _fireballScene;
		private bool _isMultishot = false;
		
		public override string Name { get; protected set; } = "火球术";
		public override SkillTriggerType TriggerType { get; protected set; } = SkillTriggerType.Active;
		
		public FireballSkill()
		{
			Description = "发射一颗火球,对敌人造成火焰伤害";
			Cooldown = 1.0f;
			IsPassive = false;
			
			// 修正场景路径并添加调试信息
			var scenePath = "res://scenes/skills/Fireball.tscn";
			GD.Print($"尝试加载火球场景: {scenePath}");
			
			_fireballScene = GD.Load<PackedScene>(scenePath);
			
			if (_fireballScene == null)
			{
				GD.PrintErr($"Failed to load Fireball scene from path: {scenePath}");
				// 尝试列出可用的场景文件
				var dir = DirAccess.Open("res://scenes/skills");
				if (dir != null)
				{
					GD.Print("Available files in scenes/skills:");
					dir.ListDirBegin();
					var fileName = dir.GetNext();
					while (fileName != "")
					{
						GD.Print($"- {fileName}");
						fileName = dir.GetNext();
					}
				}
			}
			else
			{
				GD.Print("火球场景加载成功!");
			}
		}

		protected override void CreateProjectile(Node2D source)
		{
			if (_fireballScene == null)
			{
				GD.PrintErr("Fireball scene is null!");
				return;
			}

			GD.Print($"开始释放{Name}!");

			var direction = GetAimDirection(source);
			if (source is Player player)
			{
				var nearestMonster = FindNearestMonster(player);
				if (nearestMonster != null)
				{
					direction = (nearestMonster.GlobalPosition - player.GlobalPosition).Normalized();
					GD.Print($"找到最近的怪物，发射方向: {direction}");
				}
			}

			if (_isMultishot)
			{
				GD.Print("使用多重投射模式");
				ShootMultipleFireballs(source, source.GlobalPosition, direction);
			}
			else
			{
				GD.Print("使用单发模式");
				ShootFireball(source, source.GlobalPosition, direction);
			}
		}

		private void ShootFireball(Node2D source, Vector2 position, Vector2 direction)
		{
			if (_fireballScene == null)
			{
				GD.PrintErr("无法创建火球：场景未加载");
				return;
			}

			GD.Print("开始实例化火球...");
			var fireball = _fireballScene.Instantiate<Fireball>();
			
			if (fireball == null)
			{
				GD.PrintErr("火球实例化失败!");
				return;
			}
			
			GD.Print($"创建火球，位置: {position}, 方向: {direction}");
			
			// 添加到当前场景而不是根节点
			var currentScene = source.GetTree().CurrentScene;
			currentScene.AddChild(fireball);
			
			fireball.GlobalPosition = position;
			fireball.Initialize(direction, _isMultishot);
			
			// 调试信息
			GD.Print($"火球添加到场景: Parent={fireball.GetParent().Name}, Position={fireball.GlobalPosition}, ZIndex={fireball.ZIndex}");
		}

		private void ShootMultipleFireballs(Node2D source, Vector2 position, Vector2 direction)
		{
			// 发射3个火球，呈扇形分布
			float[] angles = { -15f, 0f, 15f }; // 角度偏移
			
			foreach (float angle in angles)
			{
				var rotatedDirection = direction.Rotated(Mathf.DegToRad(angle));
				ShootFireball(source, position, rotatedDirection);
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
} 
