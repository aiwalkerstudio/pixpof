using Godot;
using System.Collections.Generic;

namespace Game.Skills.Base
{
	public abstract class SupportSkill : Skill
	{
		public override SkillTriggerType TriggerType => SkillTriggerType.Support;
		protected List<Skill> LinkedSkills { get; set; } = new();
		protected Node Source { get; set; }
		protected Node Owner { get; set; }

		public override void Initialize()
		{
			base.Initialize();
			if (SceneTree != null)
			{
				Owner = SceneTree.Root?.GetNode<Node>("/root/Main/Player");
				if (Owner != null)
				{
					Source = Owner;
				}
			}
		}

		public override void Trigger(Node source)
		{
			Source = source;
			Owner = source;
		}

		public override void OnDamageTaken(float damage)
		{
			if (Source == null && Owner != null)
			{
				Source = Owner;
			}
			base.OnDamageTaken(damage);
		}
		
		public virtual void LinkSkill(Skill skill)
		{
			if (CanLinkSkill(skill))
			{
				LinkedSkills.Add(skill);
				GD.Print($"技能 {skill.Name} 已被 {Name} 辅助");
			}
		}
		
		protected virtual bool CanLinkSkill(Skill skill)
		{
			return skill.TriggerType == SkillTriggerType.Active;
		}
	}
} 
