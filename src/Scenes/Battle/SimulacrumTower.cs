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
			SpawnBoss(randomBoss);
		}
		
		// 生成普通怪物
		int monsterCount = 3 + _currentFloor / 2; // 随层数增加怪物数量
		for (int i = 0; i < monsterCount; i++)
		{
			SpawnMonsterAtPosition("Monster1", GetRandomSpawnPosition());
		}
	}

	protected override void OnBossDefeated()
	{
		base.OnBossDefeated();
		CheckFloorComplete();
	}

	private void CheckFloorComplete()
	{
		if (_activeMonsters.Count == 0)
		{
			GD.Print($"第 {_currentFloor} 层完成!");
			DropFloorReward();
			
			// 延迟开始下一层
			var timer = GetTree().CreateTimer(2.0);
			timer.Connect("timeout", new Callable(this, nameof(StartNextFloor)));
		}
	}

	private void DropFloorReward()
	{
		// TODO: 实现层数奖励
		GD.Print("获得层数奖励!");
	}

	private void Victory()
	{
		GD.Print($"模拟回廊通关! 总层数: {_currentFloor}");
		// TODO: 添加最终奖励
		EmitSignal(SignalName.BattleCompleted);
	}
} 
