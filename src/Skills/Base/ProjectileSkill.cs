using Godot;

namespace Game.Skills.Base
{
	public abstract class ProjectileSkill : ActiveSkill
	{
		protected virtual Vector2 GetAimDirection(Node2D source)
		{
			// 默认朝向鼠标位置
			Vector2 mousePos = source.GetGlobalMousePosition();
			return (mousePos - source.GlobalPosition).Normalized();
		}

		protected virtual void OnTrigger(Node source)
		{
			// 基类的默认实现
		}

		public override void Trigger(Node source)
		{
			base.Trigger(source);
			if (source is Node2D node2D)
			{
				OnTrigger(source);  // 调用子类实现的OnTrigger
				CreateProjectile(node2D);
				StartCooldown();
			}
		}

		// 添加多重投射相关的虚拟方法
		public virtual void EnableMultiProjectiles()
		{
			// 子类可以重写此方法以支持多重投射
		}

		public virtual void DisableMultiProjectiles()
		{
			// 子类可以重写此方法以支持多重投射
		}

		protected abstract void CreateProjectile(Node2D source);
	}
} 
