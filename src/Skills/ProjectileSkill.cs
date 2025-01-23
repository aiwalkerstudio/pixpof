using Godot;

namespace Game.Skills
{
	public abstract class ProjectileSkill : Skill
	{
		protected virtual Vector2 GetAimDirection(Node2D source)
		{
			// 默认朝向鼠标位置
			Vector2 mousePos = source.GetGlobalMousePosition();
			return (mousePos - source.GlobalPosition).Normalized();
		}

		protected abstract void CreateProjectile(Node2D source);

		public override void Trigger(Node source)
		{
			if (CanTrigger() && source is Node2D source2D)
			{
				CreateProjectile(source2D);
				StartCooldown();
			}
		}
	}
} 
