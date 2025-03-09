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
		SpawnBoss(bossName, new Vector2(700, 200));
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
		
		GD.Print($"Boss Rush完成! 总波数: {_currentWave}");
		
		// 发送完成信号
		EmitSignal(SignalName.BattleCompleted);
		
		// 清理自身
		QueueFree();
	}
} 
