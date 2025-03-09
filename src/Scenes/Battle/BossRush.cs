using Godot;
using System.Collections.Generic;
using Game.Enemies;
using Game.Enemies.Boss;

public partial class BossRush : BattleMap
{
	private int _currentWave = 0;
	private int _maxWaves = 5;
	private bool _waveInProgress = false;
	private Label _waveLabel;
	private List<string> _bossSequence;

	public override void _Ready()
	{
		base._Ready();

		// 设置UI
		SetupUI();

		// 初始化Boss序列
		InitializeBossSequence();

		// 开始第一波
		StartNextWave();
	}

	private void SetupUI()
	{
		var ui = new Control();
		ui.SetAnchorsPreset(Control.LayoutPreset.TopLeft);

		_waveLabel = new Label();
		_waveLabel.Position = new Vector2(20, 20);
		ui.AddChild(_waveLabel);

		AddChild(ui);
	}

	private void InitializeBossSequence()
	{
		_bossSequence = new List<string>
		{
			"MandraBoss",
			"BoarKingBoss",
		};
	}

	private void StartNextWave()
	{
		if (_currentWave >= _maxWaves)
		{
			Victory();
			return;
		}

		_currentWave++;
		_waveInProgress = true;
		UpdateUI();

		// 生成当前波次的Boss
		string bossName = _bossSequence[_currentWave % _bossSequence.Count];
		SpawnBoss(bossName);
	}

	private void UpdateUI()
	{
		_waveLabel.Text = $"Boss Rush - {_currentWave}/{_maxWaves} Wave";
	}

	protected override void OnBossDefeated()
	{
		base.OnBossDefeated();
		_waveInProgress = false;
		
		// 给玩家一个随机增益
		ApplyRandomBuff();
		
		// 延迟开始下一波
		var timer = GetTree().CreateTimer(2.0);
		timer.Connect("timeout", new Callable(this, nameof(StartNextWave)));
	}

	private void ApplyRandomBuff()
	{
		// TODO: 实现随机增益效果
		GD.Print("获得随机增益!");
	}

	private void Victory()
	{
		GD.Print($"Boss Rush完成! 总波数: {_currentWave}");
		// TODO: 添加奖励
		EmitSignal(SignalName.BattleCompleted);
	}
} 
