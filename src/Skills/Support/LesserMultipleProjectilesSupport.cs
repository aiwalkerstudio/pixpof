using Godot;
using Game.Skills.Base;

namespace Game.Skills.Support
{
	public class LesserMultipleProjectilesSupport : SupportSkill
	{
		public override string Name { get; protected set; } = "LesserMultipleProjectilesSupport";
		
		public override void Initialize()
		{
			base.Initialize();
			Cooldown = 0f; // 被动效果
			IsPassive = true;
		}

		public override void Trigger(Node source)
		{
			base.Trigger(source);
			// 被动技能，不需要实际触发效果
		}

		public override void LinkSkill(Skill skill)
		{
			base.LinkSkill(skill);
			if (skill is ProjectileSkill projSkill)
			{
				projSkill.EnableMultiProjectiles();
			}
		}

		protected override bool CanLinkSkill(Skill skill)
		{
			// 只能辅助投射物技能
			return skill is ProjectileSkill && base.CanLinkSkill(skill);
		}
	}
} 
