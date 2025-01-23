using Godot;
using Game.Skills;

namespace Game.Skills
{
	public class MultipleProjectileSkill : Skill
	{
		public override string Name { get; protected set; } = "多重投射";
		public override SkillTriggerType TriggerType { get; protected set; } = SkillTriggerType.OnHit;
		
		public MultipleProjectileSkill()
		{
			Description = "使投射物分裂成多个";
			Cooldown = 5.0f;
			IsPassive = true;
		}
		
		public override void Trigger(Node source)
		{
			if (!CanTrigger()) return;

			GD.Print($"触发{Name}!");
			StartCooldown();

			// 查找裂魂术并启用多重投射
			if (source is SkillSlot skillSlot)
			{
				foreach (var skill in skillSlot.GetSkills())
				{
					if (skill is SoulRendSkill soulRendSkill)
					{
						soulRendSkill.EnableMultishot();
						
						// 创建定时器在持续时间后关闭多重投射
						var timer = new Timer();
						timer.WaitTime = 3.0f; // 持续3秒
						timer.OneShot = true;
						timer.Timeout += () => {
							soulRendSkill.DisableMultishot();
							timer.QueueFree();
						};
						source.AddChild(timer);
						timer.Start();
						
						break;
					}
				}
			}
		}
	}
} 
