using Godot;
using System;
using Game.Skills;
using Game.Skills.Base;
using Game.Skills.Active;
using Game.Skills.Support;
using Game;

public partial class SkillSlot : Node
{
	private Skill[] slots = new Skill[3];
	private bool _initialized = false;

	public void Initialize()
	{
		if (_initialized) return;
		
		GD.Print("SkillSlot.Initialize() called");
		
		try
		{
			// 创建技能
			var castOnDamageTaken = new Game.Skills.Support.CastOnDamageTakenSupport();
			var multipleProjectiles = new Game.Skills.Support.LesserMultipleProjectilesSupport();
			var soulRend = new Game.Skills.Active.SoulRendSkill();
			var fireball = new Game.Skills.Active.FireballSkill();
			
			// 设置技能链接
			soulRend.AddSupport(multipleProjectiles);
			fireball.AddSupport(castOnDamageTaken);
			
			// 配置技能槽
			slots[0] = castOnDamageTaken;
			slots[1] = soulRend;
			slots[2] = multipleProjectiles;
			
			// 初始化所有技能
			foreach (var skill in slots)
			{
				if (skill != null)
				{
					skill.Initialize();
					GD.Print($"初始化技能: {skill.Name}");
					GD.Print($"  类型: {skill.GetType().Name}");
					GD.Print($"  触发类型: {skill.TriggerType}");
				}
			}
			
			_initialized = true;
			GD.Print("SkillSlot初始化完成!");
		}
		catch (Exception e)
		{
			GD.PrintErr($"SkillSlot初始化错误: {e.Message}\n{e.StackTrace}");
		}
	}

	public override void _Ready()
	{
		Initialize();
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
		GD.Print($"SkillSlot.TriggerSkill被调用: slot={slot}, source={source.Name}");
		
		if(slot >= 0 && slot < slots.Length)
		{
			if(slots[slot] != null)
			{
				var skill = slots[slot];
				GD.Print($"找到技能: {skill.Name}, 类型: {skill.GetType().Name}");
				
				if(skill.CanTrigger())
				{
					GD.Print($"技能 {skill.Name} 可以触发，开始释放...");
					skill.Trigger(source);
				}
				else
				{
					GD.Print($"技能 {skill.Name} 正在冷却中，剩余冷却时间: {skill.CurrentCooldown}");
				}
			}
			else
			{
				GD.Print($"技能槽 {slot} 为空");
			}
		}
		else
		{
			GD.Print($"无效的技能槽索引: {slot}");
		}
	}

	public void OnHit(Game.Player player)
	{
		GD.Print("SkillSlot.OnHit被调用，开始检查技能...");
		// 触发被动技能
		foreach(var skill in slots)
		{
			if(skill != null)
			{
				GD.Print($"检查技能: {skill.Name}, 类型: {skill.GetType().Name}");
				if(skill is SupportSkill)
				{
					GD.Print($"发现辅助技能: {skill.Name}");
					skill.OnDamageTaken(10f); // 传递伤害值
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
