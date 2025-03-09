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
		if (_currentFloor >= _maxFloors)
		{
			Victory();
			return;
		}

		_currentFloor++;
		
		// 选择随机词条
		_currentModifier = _possibleModifiers[GD.RandRange(0, _possibleModifiers.Length - 1)];
		
		UpdateUI();
		ApplyFloorModifier();

		// 生成怪物和Boss
		SpawnFloorEnemies();
	}

	private void UpdateUI()
	{
		_floorLabel.Text = $"Simulacrum Tower - {_currentFloor}/{_maxFloors} floors";
		_modifierLabel.Text = $"current modifiers: {_currentModifier}";
	}

	private void ApplyFloorModifier()
	{
		// TODO: 实现词条效果
		GD.Print($"应用词条效果: {_currentModifier}");
	}

	private void SpawnFloorEnemies()
	{
		// 每5层生成一个Boss
		if (_currentFloor % 5 == 0)
		{
			string[] bosses = { "MandraBoss", "BoarKingBoss" };
			string randomBoss = bosses[GD.RandRange(0, bosses.Length - 1)];
			SpawnBoss(randomBoss,new Vector2(700, 200));
		}
		
		// 生成普通怪物
		int monsterCount = 3 + _currentFloor / 2; // 随层数增加怪物数量
		for (int i = 0; i < monsterCount; i++)
		{
			SpawnMonsterAtPosition("Monster1", GetRandomSpawnPosition());
		}
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

	protected override void OnBossDefeated()
	{
		base.OnBossDefeated();
		
		// Boss被击败后，检查当前层是否完成
		CheckFloorComplete();
	}

	private void OnMonsterDied(Enemy enemy)
	{
		if (enemy is Monster monster)
		{
			_activeMonsters.Remove(monster);
			
			// 怪物死亡后，检查当前层是否完成
			CheckFloorComplete();
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
} 
