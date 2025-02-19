using Godot;
using System;

namespace Game.Skills
{
	public abstract class Skill
	{
		public abstract string Name { get; protected set; }
		public virtual string Description { get; protected set; } = "";
		public virtual float Cooldown { get; protected set; } = 0f;
		public float CurrentCooldown { get; protected set; } = 0f;
		public virtual bool IsPassive { get; protected set; } = false;
		public abstract SkillTriggerType TriggerType { get; }
		public virtual bool HasReservation { get; protected set; } = false;
		public virtual bool IsChanneling { get; protected set; } = false;
		protected SceneTree SceneTree { get; private set; }
		public virtual float ManaCost { get; protected set; } = 0f;  // 技能魔法消耗
		protected Node Source { get; private set; }  // 添加Source属性

		public virtual void Initialize()
		{
			SceneTree = Engine.GetMainLoop() as SceneTree;
			CurrentCooldown = 0;
		}

		public virtual bool CanTrigger()
		{
			if (Source is Player player)
			{
				return CurrentCooldown <= 0 && player.ConsumeMana(ManaCost);
			}
			return CurrentCooldown <= 0;
		}

		public virtual void Trigger(Node source)
		{
			Source = source;  // 在触发技能时设置Source
		}

		public virtual void Update(float delta)
		{
			if (CurrentCooldown > 0)
			{
				CurrentCooldown -= delta;
			}
		}

		protected void StartCooldown()
		{
			CurrentCooldown = Cooldown;
		}

		public virtual void OnDamageTaken(float damage) { }
	}
} 
