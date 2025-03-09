using Godot;
using System.Collections.Generic;
using Game.Enemies;
using Game.Enemies.Boss;

public partial class SurvivalGauntlet : BattleMap
{
	private float _survivalTime = 120.0f; // 2分钟
	private float _currentTime = 0f;
	private int _bossesDefeated = 0;
	private Timer _bossSpawnTimer;
	private Label _timeLabel;
	private Label _scoreLabel;

	public override void _Ready()
	{
		base._Ready();

		// 添加UI
		SetupUI();

		// 设置Boss生成计时器
		_bossSpawnTimer = new Timer();
		AddChild(_bossSpawnTimer);
		_bossSpawnTimer.WaitTime = 20.0f; // 每20秒生成新Boss
		_bossSpawnTimer.Connect("timeout", new Callable(this, nameof(SpawnRandomBoss)));
		_bossSpawnTimer.Start();

		// 生成初始Boss
		SpawnRandomBoss();
	}

	private void SetupUI()
	{
		var ui = new Control();
		ui.SetAnchorsPreset(Control.LayoutPreset.TopLeft);

		_timeLabel = new Label();
		_timeLabel.Position = new Vector2(20, 20);
		ui.AddChild(_timeLabel);

		_scoreLabel = new Label();
		_scoreLabel.Position = new Vector2(20, 50);
		ui.AddChild(_scoreLabel);

		AddChild(ui);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		_currentTime += (float)delta;
		UpdateUI();

		// 检查是否通关
		if (_currentTime >= _survivalTime)
		{
			Victory();
		}
	}

	private void UpdateUI()
	{
		float remainingTime = _survivalTime - _currentTime;
		_timeLabel.Text = $"remaining time: {remainingTime:F1} s";
		_scoreLabel.Text = $"bosses defeated: {_bossesDefeated}";
	}

	private void SpawnRandomBoss()
	{
		string[] bosses = { "MandraBoss", "BoarKingBoss" };
		string randomBoss = bosses[GD.RandRange(0, bosses.Length - 1)];
		
		// 随机位置
		float x = (float)GD.RandRange(200, 800);
		float y = (float)GD.RandRange(150, 450);
		
		SpawnBoss(randomBoss);
	}

	private void Victory()
	{
		_bossSpawnTimer.Stop();
		GD.Print($"生存挑战成功! 击败Boss数: {_bossesDefeated}");
		// TODO: 添加奖励
		EmitSignal(SignalName.BattleCompleted);
	}

	protected override void OnBossDefeated()
	{
		base.OnBossDefeated();
		_bossesDefeated++;
		GD.Print($"击败Boss! 当前击败数: {_bossesDefeated}");
	}
} 
