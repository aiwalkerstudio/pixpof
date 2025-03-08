using Godot;

namespace Game.Classes
{
    public abstract class BaseClass
    {
        public string Name { get; protected set; }
        public virtual int BaseStrength { get; protected set; }
        public virtual int BaseAgility { get; protected set; }
        public virtual int BaseIntelligence { get; protected set; }
        public virtual float BaseHealth { get; protected set; }
        public virtual float BaseMana { get; protected set; }
        public virtual int BaseGold { get; protected set; }

        public virtual void Initialize(Game.Player player)
        {
            // 基础初始化逻辑
        }

        public virtual void Update(Game.Player player, double delta)
        {
            // 基础更新逻辑
        }

        public virtual void OnDeath(Game.Player player)
        {
            // 死亡处理逻辑
        }
    }
} 