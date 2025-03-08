using Godot;

namespace Game.Classes
{
	public class Cracker : BaseClass
	{
		private int _deathCount = 0;
		
		public Cracker()
		{
			Name = "Cracker";//穷鬼
			BaseStrength = 10;
			BaseAgility = 10;
			BaseIntelligence = 8;
			BaseHealth = 100;
			BaseMana = 80;
			BaseGold = 100000;
		}

		public override void Initialize(Game.Player player)
		{
			base.Initialize(player);
			
			// 设置初始属性
			player.MaxHealth = BaseHealth;
			player.CurrentHealth = BaseHealth;
			player.MaxMana = BaseMana;
			player.CurrentMana = BaseMana;
			player.Gold = BaseGold;
			
			// 初始化其他属性
			player.Strength = BaseStrength;
			player.Agility = BaseAgility;
			player.Intelligence = BaseIntelligence;
		}

		public override void Update(Game.Player player, double delta)
		{
			base.Update(player, delta);

			// 计算金币带来的速度加成
			float speedBonus = player.Gold / 10 * 0.01f; // 每10金币提供1%速度加成
			player.MoveSpeed = player.BaseMoveSpeed * (1 + speedBonus);

			// 金币获取加成在获得金币时处理
		}

		public override void OnDeath(Game.Player player)
		{
			base.OnDeath(player);

			// 增加死亡计数
			_deathCount++;

			// 计算损失的金币
			int goldLoss = _deathCount * 10000;
			player.Gold = Mathf.Max(0, player.Gold - goldLoss);

			GD.Print($"穷鬼死亡 {_deathCount} 次,损失 {goldLoss} 金币");
		}

		// 金币获取加成
		public float GetGoldBonus(int baseGold)
		{
			return baseGold * 1.2f; // 20%的金币获取加成
		}
	}
} 
