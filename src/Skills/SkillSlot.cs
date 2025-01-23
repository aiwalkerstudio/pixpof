using Godot;
using System;

public partial class SkillSlot : Node
{
	private Skill[] slots = new Skill[3];

	public override void _Ready()
	{
		// 默认技能配置
		slots[0] = new OnHitSkill();
		slots[1] = new FireballSkill(); 
		slots[2] = new MultipleProjectileSkill();

		foreach(var skill in slots)
		{
			if(skill != null)
			{
				skill.Initialize();
			}
		}
	}

	public override void _Process(double delta)
	{
		foreach(var skill in slots)
		{
			if(skill != null)
			{
				skill.Update((float)delta);
			}
		}
	}

	public void TriggerSkill(int slot, Node source)
	{
		if(slot >= 0 && slot < slots.Length && slots[slot] != null)
		{
			if(slots[slot].CanTrigger())
			{
				slots[slot].Trigger(source);
			}
		}
	}

	public void OnHit(Player player)
	{
		GD.Print("SkillSlot.OnHit被调用，开始检查技能...");
		// 触发被动技能
		foreach(var skill in slots)
		{
			if(skill != null)
			{
				GD.Print($"检查技能: {skill.Name}, 类型: {skill.TriggerType}, 是否可触发: {skill.CanTrigger()}");
				if(skill.TriggerType == SkillTriggerType.OnHit && skill.CanTrigger())
				{
					GD.Print($"触发技能: {skill.Name}");
					skill.Trigger(player);
				}
			}
		}
	}

	// 获取所有技能
	public Skill[] GetSkills()
	{
		return slots;
	}
} 
