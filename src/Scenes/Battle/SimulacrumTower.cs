using Godot;
using System.Collections.Generic;
using Game.Enemies;
using Game.Enemies.Boss;

public partial class SimulacrumTower : BattleMap
{
	private int _currentFloor = 0;
	private int _maxFloors = 15;
	private Label _floorLabel;
	private Label _modifierLabel;
	private string _currentModifier = "";

	private readonly string[] _possibleModifiers = {
		"Reduce health by 30%",
		"Increase enemy speed by 50%",
		"Reduce player damage by 25%",
		"Double the number of enemies",
		"Increase skill cooldown by 50%"
	};

	public override void _Ready()
	{
		base._Ready();

		// 设置UI
		SetupUI();

		// 开始第一层
		StartNextFloor();
	}

	private void SetupUI()
	{
		var ui = new Control();
		ui.SetAnchorsPreset(Control.LayoutPreset.TopLeft);

		_floorLabel = new Label();
		_floorLabel.Position = new Vector2(20, 20);
		ui.AddChild(_floorLabel);

		_modifierLabel = new Label();
		_modifierLabel.Position = new Vector2(20, 50);
		ui.AddChild(_modifierLabel);

		AddChild(ui);
	}

	private void StartNextFloor()
	{
		_currentFloor++;
		
		// 更新UI显示
		if (_floorLabel != null)
		{
			_floorLabel.Text = $"Simulacrum Tower - {_currentFloor}/{_maxFloors} floors";
		}
		
		// 随机选择一个修饰符
		if (_currentFloor > 1) // 第一层没有修饰符
		{
			int modifierIndex = (int)GD.RandRange(0, _possibleModifiers.Length - 1);
			_currentModifier = _possibleModifiers[modifierIndex];
			
			if (_modifierLabel != null)
			{
				_modifierLabel.Text = $"current modifiers: {_currentModifier}";
			}
			
			GD.Print($"层级 {_currentFloor} 修饰符: {_currentModifier}");
			
			// 应用修饰符效果
			ApplyFloorModifier();
		}
		
		// 生成怪物和Boss
		SpawnFloorEnemies();
		
		GD.Print($"开始第 {_currentFloor} 层");
	}

	private void UpdateUI()
	{
		_floorLabel.Text = $"Simulacrum Tower - {_currentFloor}/{_maxFloors} floors";
		_modifierLabel.Text = $"current modifiers: {_currentModifier}";
	}

	private void ApplyFloorModifier()
	{
		// 实现词条效果
		GD.Print($"应用词条效果: {_currentModifier}");
		
		switch (_currentModifier)
		{
			case "Reduce health by 30%":
				if (_player != null)
				{
					float newHealth = _player.CurrentHealth * 0.7f;
					_player.TakeDamage(_player.CurrentHealth - newHealth);
					GD.Print($"玩家生命值降低30%: {_player.CurrentHealth}");
				}
				break;
				
			case "Increase enemy speed by 50%":
				// 这个效果需要在怪物生成时应用
				GD.Print("怪物速度提高50%");
				break;
				
			case "Reduce player damage by 25%":
				// 这个效果需要在玩家攻击时应用
				GD.Print("玩家伤害降低25%");
				break;
				
			case "Double the number of enemies":
				// 这个效果在SpawnFloorEnemies中应用
				GD.Print("怪物数量翻倍");
				break;
				
			case "Increase skill cooldown by 50%":
				// 这个效果需要在技能系统中应用
				GD.Print("技能冷却时间增加50%");
				break;
		}
	}

	private void SpawnFloorEnemies()
	{
		GD.Print($"生成第 {_currentFloor} 层的怪物和Boss");
		
		// 每5层生成一个Boss
		if (_currentFloor % 5 == 0)
		{
			// 在地图中央生成Boss
			Vector2 bossPosition = new Vector2(500, 300);
			SpawnRandomBoss();
			GD.Print($"在第 {_currentFloor} 层生成了Boss");
		}
		
		// 生成普通怪物
		int monsterCount = 3 + _currentFloor / 2; // 随层数增加怪物数量
		
		// 应用"Double the number of enemies"修饰符
		if (_currentModifier == "Double the number of enemies")
		{
			monsterCount *= 2;
			GD.Print($"应用修饰符: 怪物数量翻倍，当前数量: {monsterCount}");
		}
		
		// 确保至少生成1个怪物
		monsterCount = Mathf.Max(1, monsterCount);
		
		GD.Print($"准备生成 {monsterCount} 个怪物");
		
		// 在地图四周生成怪物
		for (int i = 0; i < monsterCount; i++)
		{
			Vector2 position = GetRandomSpawnPosition();
			SpawnMonsterAtPosition("Monster1", position);
			GD.Print($"生成怪物 {i+1}/{monsterCount} 在位置 {position}");
		}
		
		// 打印当前怪物和Boss数量
		GD.Print($"当前层级 {_currentFloor} 的怪物数量: {_activeMonsters.Count}, Boss数量: {_bosses.Count}");
	}

	private void CheckFloorComplete()
	{
		// 检查当前层是否还有存活的怪物或Boss
		bool hasLivingMonsters = _activeMonsters.Count > 0;
		bool hasLivingBosses = _bosses.Count > 0;
		
		// 如果是Boss层(5的倍数)，需要等Boss被击败
		bool isBossFloor = _currentFloor % 5 == 0;
		
		if (isBossFloor && hasLivingBosses)
		{
			// Boss层且Boss还活着，继续战斗
			return;
		}
		
		if (!isBossFloor && hasLivingMonsters)
		{
			// 普通层且还有怪物，继续战斗
			return;
		}
		
		// 到这里说明当前层已完成
		GD.Print($"第 {_currentFloor} 层完成!");
		DropFloorReward();
		
		// 延迟开始下一层
		var timer = GetTree().CreateTimer(2.0);
		timer.Connect("timeout", new Callable(this, nameof(StartNextFloor)));
	}

	protected override void OnAllMonstersDefeated()
	{
		GD.Print($"所有怪物已击败! 当前层数: {_currentFloor}/{_maxFloors}, 活跃怪物数: {_activeMonsters.Count}, Boss数: {_bosses.Count}");
		
		// 检查是否还有Boss存活
		if (_bosses.Count > 0)
		{
			GD.Print("还有Boss存活，等待Boss被击败");
			return;
		}
		
		// 检查是否已经完成所有层级
		if (_currentFloor >= _maxFloors)
		{
			GD.Print("已完成所有层级，发送战斗完成信号");
			Victory();
			return;
		}
		
		// 否则进入下一层
		GD.Print("进入下一层");
		
		// 延迟一下再进入下一层，给玩家一些喘息时间
		var timer = GetTree().CreateTimer(2.0);
		timer.Timeout += StartNextFloor;
	}

	private void OnMonsterDied(Enemy enemy)
	{
		// 类型转换
		if (enemy is Monster monster)
		{
			_activeMonsters.Remove(monster);
			GD.Print($"怪物死亡，剩余怪物数量: {_activeMonsters.Count}");
			
			// 检查是否所有怪物都被击败
			CheckMonstersDefeated();
		}
	}

	private void DropFloorReward()
	{
		// TODO: 实现层数奖励
		GD.Print("获得层数奖励!");
	}

	private void Victory()
	{
		// 清理所有Boss
		for (int i = _bosses.Count - 1; i >= 0; i--)
		{
			var boss = _bosses[i];
			if (IsInstanceValid(boss))
			{
				boss.QueueFree();
			}
		}
		_bosses.Clear();
		
		// 清理所有怪物
		for (int i = _activeMonsters.Count - 1; i >= 0; i--)
		{
			var monster = _activeMonsters[i];
			if (IsInstanceValid(monster))
			{
				monster.QueueFree();
			}
		}
		_activeMonsters.Clear();
		
		GD.Print($"模拟回廊通关! 总层数: {_currentFloor}");
		
		// 发送完成信号
		EmitSignal(SignalName.BattleCompleted);
		
		// 清理自身
		QueueFree();
	}

	protected new Vector2 GetRandomSpawnPosition()
	{
		// 将地图分为四个象限，确保怪物分布均匀
		int quadrant = GD.RandRange(0, 3);
		float x = 0, y = 0;
		
		switch (quadrant)
		{
			case 0: // 左上
				x = GD.RandRange(200, 400);
				y = GD.RandRange(150, 250);
				break;
			case 1: // 右上
				x = GD.RandRange(600, 800);
				y = GD.RandRange(150, 250);
				break;
			case 2: // 左下
				x = GD.RandRange(200, 400);
				y = GD.RandRange(350, 450);
				break;
			case 3: // 右下
				x = GD.RandRange(600, 800);
				y = GD.RandRange(350, 450);
				break;
		}
		
		return new Vector2(x, y);
	}
} 
