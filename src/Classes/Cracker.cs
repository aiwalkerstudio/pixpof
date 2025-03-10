using Godot;

namespace Game.Classes
{
	public class Cracker : BaseClass
	{
		private int _deathCount = 0;
		
		public Cracker()
		{
			Name = "Cracker";
			BaseStrength = 10;
			BaseAgility = 10;
			BaseIntelligence = 8;
			BaseHealth = 100;
			BaseMana = 800000;
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

			// 确保设置正确的移动速度
			player.MoveSpeed = player.BaseMoveSpeed;
			
			GD.Print($"穷鬼职业初始化完成: 生命={BaseHealth}, 魔法={BaseMana}, 金币={BaseGold}, 移动速度={player.MoveSpeed}");
		}

		public override void Update(Game.Player player, double delta)
		{
			base.Update(player, delta);

			// 计算金币带来的速度加成
			float speedBonus = (player.Gold / 1000f) * 0.01f; // 每1000金币提供1%速度加成
			speedBonus = Mathf.Min(speedBonus, 2.0f); // 最大200%加成
			
			float finalSpeed = player.BaseMoveSpeed * (1 + speedBonus);
			finalSpeed = Mathf.Min(finalSpeed, 1000f); // 限制最大速度
			
			player.MoveSpeed = finalSpeed;

			// 调试输出
			if (GD.Randi() % 100 == 0) // 降低打印频率
			{
				//GD.Print($"穷鬼当前状态: 金币={player.Gold}, 速度加成={speedBonus:P}, 实际速度={finalSpeed}");
			}
		}

		public override void OnDeath(Game.Player player)
		{
			base.OnDeath(player);

			// 增加死亡计数
			_deathCount++;

			// 计算损失的金币
			int goldLoss = _deathCount * 1;
			player.Gold = Mathf.Max(0, player.Gold - goldLoss);

			//GD.Print($"穷鬼死亡 {_deathCount} 次,损失 {goldLoss} 金币");
		}

		// 金币获取加成
		public float GetGoldBonus(int baseGold)
		{
			return baseGold * 1.2f; // 20%的金币获取加成
		}
	}
} 
