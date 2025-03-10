using Godot;
using System.Collections.Generic;
using Game.Enemies;
using Game.Enemies.Boss;

public partial class BossRush : BattleMap
{
	private int _currentWave = 0;
	private int _maxWaves = 5;
	private Label _waveLabel;
	private bool _isTransitioningToNextWave = false;

	// Boss列表
	private readonly string[] _bossList = {
		"MandraBoss",
		"BoarKingBoss"
	};

	public override void _Ready()
	{
		base._Ready();

		// 设置UI
		SetupUI();

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

	private void StartNextWave()
	{
		GD.Print($"开始加载第 {_currentWave + 1} 波");
		
		_currentWave++;
		
		// 更新UI显示
		if (_waveLabel != null)
		{
			_waveLabel.Text = $"Boss Rush - Wave {_currentWave}/{_maxWaves}";
		}
		
		// 生成Boss
		SpawnWaveBoss();
		
		GD.Print($"开始第 {_currentWave} 波");
	}

	private void SpawnWaveBoss()
	{
		// 选择当前波次的Boss
		string bossName = _bossList[(_currentWave - 1) % _bossList.Length];
		
		// 在中央生成Boss
		Vector2 position = new Vector2(500, 300);
		
		GD.Print($"尝试生成Boss: {bossName} 在位置 {position}");
		
		try
		{
			// 加载Boss场景
			var bossScene = ResourceLoader.Load<PackedScene>($"res://scenes/enemies/boss/{bossName}.tscn");
			if(bossScene != null)
			{
				var boss = bossScene.Instantiate<Enemy>();
				_monsters.AddChild(boss);
				_bosses.Add(boss);
				
				// 设置生成位置
				boss.GlobalPosition = position;
				
				// 连接Boss的Died信号
				boss.Died -= OnBossDied; // 先断开以防重复连接
				boss.Died += OnBossDied;
				GD.Print($"成功连接Boss的Died信号");
				
				// 确保连接Boss的BossDefeated信号
				if (boss is MandraBoss mandraBoss)
				{
					mandraBoss.BossDefeated -= OnBossDefeated; // 先断开以防重复连接
					mandraBoss.BossDefeated += OnBossDefeated;
					GD.Print($"成功连接MandraBoss的BossDefeated信号");
				}
				else if (boss is BoarKingBoss boarBoss)
				{
					boarBoss.BossDefeated -= OnBossDefeated; // 先断开以防重复连接
					boarBoss.BossDefeated += OnBossDefeated;
					GD.Print($"成功连接BoarKingBoss的BossDefeated信号");
				}
				else
				{
					GD.PrintErr($"未知Boss类型: {boss.GetType()}");
				}
				
				GD.Print($"成功生成Boss: {bossName} 在位置 {position}");
			}
			else
			{
				GD.PrintErr($"Failed to load {bossName} scene!");
			}
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Error spawning boss {bossName}: {e.Message}\n{e.StackTrace}");
		}
	}

	// 添加OnBossDied方法处理Boss的Died信号
	private void OnBossDied(Enemy enemy)
	{
		GD.Print($"收到Boss的Died信号: {enemy.Name}, ID: {enemy.GetInstanceId()}");
		
		// 从_bosses列表中移除
		_bosses.Remove(enemy);
		
		GD.Print($"Boss已从列表中移除，剩余Boss数: {_bosses.Count}");
		
		// 检查是否可以进入下一波
		if (_bosses.Count == 0)
		{
			GD.Print("所有Boss都被击败，准备进入下一波");
			
			// 检查是否已经完成所有波次
			if (_currentWave >= _maxWaves)
			{
				GD.Print("已完成所有波次，发送战斗完成信号");
				Victory();
				return;
			}
			
			// 如果已经在过渡到下一波，直接返回
			if (_isTransitioningToNextWave)
			{
				GD.Print("已经在过渡到下一波，忽略重复调用");
				return;
			}
			
			// 设置过渡标志
			_isTransitioningToNextWave = true;
			
			// 进入下一波
			GD.Print("进入下一波");
			CallDeferred(nameof(DelayedStartNextWave));
		}
	}

	// 添加延迟启动下一波的方法
	private void DelayedStartNextWave()
	{
		// 创建一次性计时器
		var timer = GetTree().CreateTimer(2.0);
		timer.Timeout += () => {
			StartNextWave();
			// 重置过渡标志
			_isTransitioningToNextWave = false;
		};
	}

	protected override void OnBossDefeated()
	{
		base.OnBossDefeated();
		
		GD.Print($"Boss被击败! 当前波次: {_currentWave}/{_maxWaves}, 剩余Boss数: {_bosses.Count}");
		
		// 打印所有Boss的状态
		for (int i = 0; i < _bosses.Count; i++)
		{
			var boss = _bosses[i];
			GD.Print($"Boss[{i}] IsValid: {IsInstanceValid(boss)}, ID: {(IsInstanceValid(boss) ? boss.GetInstanceId().ToString() : "无效")}");
			
			// 手动销毁Boss
			if (IsInstanceValid(boss))
			{
				GD.Print($"手动销毁Boss: {i}");
				boss.QueueFree();
				_bosses.RemoveAt(i);
				i--; // 调整索引
			}
		}
		
		GD.Print($"清理后剩余Boss数: {_bosses.Count}");
		
		// 检查是否可以进入下一波
		if (_bosses.Count == 0)
		{
			GD.Print("所有Boss都被击败，准备进入下一波");
			
			// 检查是否已经完成所有波次
			if (_currentWave >= _maxWaves)
			{
				GD.Print("已完成所有波次，发送战斗完成信号");
				Victory();
				return;
			}
			
			// 如果已经在过渡到下一波，直接返回
			if (_isTransitioningToNextWave)
			{
				GD.Print("已经在过渡到下一波，忽略重复调用");
				return;
			}
			
			// 设置过渡标志
			_isTransitioningToNextWave = true;
			
			// 进入下一波
			GD.Print("进入下一波");
			CallDeferred(nameof(DelayedStartNextWave));
		}
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
		
		GD.Print($"Boss Rush通关! 总波次: {_currentWave}");
		
		// 发送完成信号
		EmitSignal(SignalName.BattleCompleted);
		
		// 清理自身
		QueueFree();
	}
} 
