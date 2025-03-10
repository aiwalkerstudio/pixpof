using Godot;
using System;
using System.Collections.Generic;
using Game;  // 添加这行来引用 Game 命名空间
using Game.Enemies.Boss;  // 添加这行来引用 MandraBoss
using Game.Enemies;  // Monster 类在这个命名空间下
using System.Linq;
using Game.UI.Battle;  // 添加命名空间引用

public partial class BattleMap : Node2D
{
	protected Node2D _monsters;
	protected List<Monster> _activeMonsters = new();
	protected Game.Player _player;  // 使用完整的命名空间路径
	protected BattleUI _battleUI;
	protected List<Enemy> _bosses = new();  // 改为Boss列表

	// 添加BOSS预制体引用
	[Export] 
	protected PackedScene EaterOfWorldsScene;
	
	[Export]
	protected PackedScene SearingExarchScene;

	// BOSS生成点
	[Export] 
	protected Node2D BossSpawnPoint;

	[Signal]
	public delegate void BattleCompletedEventHandler();

	public override void _Ready()
	{
		_monsters = GetNode<Node2D>("Monsters");
		
		// 启用Y-sort，使得物体根据Y坐标自动排序
		_monsters.YSortEnabled = true;
	}

	// 新增：由Main调用的初始化方法
	public void Initialize(Game.Player player, BattleUI battleUI)
	{
		_player = player;
		_battleUI = battleUI;

		// 初始化战斗场景
		InitializeBattle();
		
		// 连接玩家和UI的信号
		if (_player != null && _battleUI != null)
		{
			// 连接玩家的生命值变化信号到UI
			_player.HealthChanged += _battleUI.UpdateHealth;
			
			// 连接玩家的护盾值变化信号到UI
			_player.ShieldChanged += _battleUI.UpdateShield;
			
			// 连接UI按钮到玩家
			_battleUI.AttackPressed += _player.OnAttackPressed;
			_battleUI.SkillPressed += _player.OnSkillPressed;
			
			// 立即更新UI显示当前生命值和护盾值
			_battleUI.UpdateHealth(_player.CurrentHealth, _player.MaxHealth);
			_battleUI.UpdateShield(_player.CurrentEnergyShield, _player.MaxEnergyShield);
			
			GD.Print($"Connected player signals to UI. Player health: {_player.CurrentHealth}/{_player.MaxHealth}, Shield: {_player.CurrentEnergyShield}/{_player.MaxEnergyShield}");
		}
		else
		{
			GD.PrintErr("Failed to connect player signals: player or UI is null");
		}
	}

	private void InitializeBattle()
	{
		try
		{
			// 生成玩家
			// SpawnPlayer();
			
			// 生成怪物
			// SpawnMonsters();

			// // 加载曼陀罗Boss场景
			// var mandraBossScene = GD.Load<PackedScene>("res://scenes/enemies/boss/MandraBoss.tscn");
			// if (mandraBossScene != null)
			// {
			// 	var mandraBoss = mandraBossScene.Instantiate<MandraBoss>();
			// 	SpawnBoss(mandraBoss, "曼陀罗Boss", new Vector2(700, 200));
			// 	_bosses.Add(mandraBoss);
			// }
			// else
			// {
			// 	GD.PrintErr("Failed to load MandraBoss scene!");
			// }

			// // 加载野猪王Boss场景
			// var boarBossScene = GD.Load<PackedScene>("res://scenes/enemies/boss/BoarKingBoss.tscn");
			// if (boarBossScene != null)
			// {
			// 	var boarBoss = boarBossScene.Instantiate<BoarKingBoss>();
			// 	SpawnBoss(boarBoss, "野猪王Boss", new Vector2(900, 400));
			// 	_bosses.Add(boarBoss);
			// }
			// else
			// {
			// 	GD.PrintErr("Failed to load BoarKingBoss scene!");
			// }

			// SpawnBoss("SearingExarch");
			// SpawnBoss("EaterOfWorlds");
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error spawning bosses: {e.Message}\n{e.StackTrace}");
		}
	}

	private void SpawnPlayer()
	{
		var playerSpawn = GetNode<Marker2D>("SpawnPoints/PlayerSpawn");
		
		if (_player != null)
		{
			GD.Print($"BattleMap found player at: {_player.GetPath()}");
			_player.GlobalPosition = playerSpawn.GlobalPosition;
			
			if (_battleUI != null)
			{
				// 连接玩家和战斗UI
				_battleUI.AttackPressed += _player.OnAttackPressed;
				_battleUI.SkillPressed += _player.OnSkillPressed;
			}
		}
	}

	protected void SpawnMonsters()
	{
		// 生成多个普通怪物
		var monsterSpawns = new List<(string type, Vector2 position)>
		{
			("Monster1", new Vector2(300, 200)),  // 左上
			("Monster1", new Vector2(300, 400)),  // 左下
			("Monster1", new Vector2(700, 200)),  // 右上
			("Monster1", new Vector2(700, 400)),  // 右下
			("Monster1", new Vector2(500, 150)),  // 上中
			("Monster1", new Vector2(500, 450)),  // 下中
		};

		foreach (var spawn in monsterSpawns)
		{
			SpawnMonsterAtPosition(spawn.type, spawn.position);
		}

		// 随机生成额外的怪物
		for (int i = 0; i < 3; i++)  // 随机生成3个额外的怪物
		{
			float x = (float)GD.RandRange(200, 800);
			float y = (float)GD.RandRange(150, 450);
			SpawnMonsterAtPosition("Monster1", new Vector2(x, y));
		}
	}

	protected void SpawnMonsterAtPosition(string monsterType, Vector2 position)
	{
		try
		{
			var monsterScene = GD.Load<PackedScene>($"res://scenes/monsters/{monsterType}.tscn");
			if (monsterScene != null)
			{
				var monster = monsterScene.Instantiate<Monster>();
				monster.GlobalPosition = position;
				_monsters.AddChild(monster);
				_activeMonsters.Add(monster);
				
				// 确保连接怪物信号
				monster.Died -= OnMonsterDied; // 先断开以防重复连接
				monster.Died += OnMonsterDied;
				
				GD.Print($"Spawned {monsterType} at position {position}, connected Died signal");
			}
			else
			{
				GD.PrintErr($"Failed to load monster scene: {monsterType}");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error spawning monster {monsterType}: {e.Message}");
		}
	}


	protected void SpawnRandomBoss()
	{
		string[] bosses = { "MandraBoss", "BoarKingBoss" };
		string randomBoss = bosses[GD.RandRange(0, bosses.Length - 1)];
		
		// 随机位置
		float x = (float)GD.RandRange(200, 800);
		float y = (float)GD.RandRange(150, 450);
		
		SpawnBoss(randomBoss, new Vector2(x, y));
	}
	// 添加一个方法来检查位置是否合适
	private bool IsValidSpawnPosition(Vector2 position)
	{
		// 检查与其他怪物的距离
		foreach (var monster in _activeMonsters)
		{
			if (monster.GlobalPosition.DistanceTo(position) < 50)  // 最小间距
			{
				return false;
			}
		}
		
		// 检查与玩家的距离
		if (_player != null && _player.GlobalPosition.DistanceTo(position) < 100)  // 与玩家保持距离
		{
			return false;
		}
		
		return true;
	}

	protected Vector2 GetRandomSpawnPosition()
	{
		for (int i = 0; i < 10; i++)  // 最多尝试10次
		{
			float x = (float)GD.RandRange(200, 800);
			float y = (float)GD.RandRange(150, 450);
			Vector2 position = new Vector2(x, y);
			
			if (IsValidSpawnPosition(position))
			{
				return position;
			}
		}
		
		// 如果找不到合适的位置，返回一个默认位置
		return new Vector2(500, 300);
	}

	private void OnMonsterDied(Enemy enemy)
	{
		// 类型转换
		if (enemy is Monster monster)
		{
			_activeMonsters.Remove(monster);
			GD.Print($"BattleMap: 怪物死亡，剩余怪物数量: {_activeMonsters.Count}");
			
			// 检查是否所有怪物都被击败
			CheckMonstersDefeated();
		}
	}

	public override void _Process(double delta)
	{
		// 更新战斗状态
		UpdateBattle(delta);
	}

	private void UpdateBattle(double delta)
	{
		// 创建列表副本进行遍历
		var monsters = _activeMonsters.ToList();
		foreach (var monster in monsters)
		{
			if (monster != null && IsInstanceValid(monster))
			{
				UpdateMonsterAI(monster, delta);
			}
		}
	}

	private void UpdateMonsterAI(Monster monster, double delta)
	{
		if (_player != null && monster != null)
		{
			monster.UpdateAI(_player, (float)delta);
		}
	}

	private void SpawnBoss(Enemy boss, string bossName, Vector2 position)
	{
		// 设置Boss位置
		boss.GlobalPosition = position;
		
		_monsters.AddChild(boss);
		_bosses.Add(boss);
		GD.Print($"{bossName} spawned at {position}");

		// 连接Boss信号
		if (boss is MandraBoss mandraBoss)
		{
			mandraBoss.BossDefeated += OnBossDefeated;
		}
		else if (boss is BoarKingBoss boarBoss)
		{
			boarBoss.BossDefeated += OnBossDefeated;
		}
	}

	protected virtual void OnBossDefeated()
	{
		// 检查是否所有Boss都被击败
		_bosses.RemoveAll(boss => !IsInstanceValid(boss));
		
		// if (_bosses.Count == 0 && _activeMonsters.Count == 0)
		// {
		// 	EmitSignal(SignalName.BattleCompleted);
		// }
	}

	// 生成BOSS
	public void SpawnBoss(string bossName, Vector2 position)
	{
		try
		{
			// 加载Boss场景
			var bossScene = ResourceLoader.Load<PackedScene>($"res://scenes/enemies/boss/{bossName}.tscn");
			if(bossScene != null)
			{
				var boss = bossScene.Instantiate<Enemy>();
				_monsters.AddChild(boss);
				_bosses.Add(boss);
				
				// 使用传入的position设置生成位置
				boss.GlobalPosition = position;
				
				GD.Print($"Spawned {bossName} at position {position}");
			}
			else
			{
				GD.PrintErr($"Failed to load {bossName} scene!");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error spawning boss {bossName}: {e.Message}\n{e.StackTrace}");
		}
	}

	// 添加新方法处理玩家死亡
	public virtual void OnPlayerDied()
	{
		GD.Print("Player died, ending battle...");
		
		// 清理所有Boss和怪物
		for (int i = _bosses.Count - 1; i >= 0; i--)
		{
			var boss = _bosses[i];
			if (IsInstanceValid(boss))
			{
				boss.QueueFree();
			}
		}
		_bosses.Clear();
		
		for (int i = _activeMonsters.Count - 1; i >= 0; i--)
		{
			var monster = _activeMonsters[i];
			if (IsInstanceValid(monster))
			{
				monster.QueueFree();
			}
		}
		_activeMonsters.Clear();
		
		// 发送战斗结束信号
		EmitSignal(SignalName.BattleCompleted);
		
		// 延迟清理场景，确保信号发送完成
		var timer = GetTree().CreateTimer(0.1f);
		timer.Connect("timeout", new Callable(this, nameof(CleanupScene)));
	}

	protected void CleanupScene()
	{
		QueueFree();
	}

	protected virtual void OnAllMonstersDefeated()
	{
		GD.Print("所有怪物已击败!");
		
		// 基类中的默认行为
		// 可以在子类中重写此方法
	}

	protected void CheckMonstersDefeated()
	{
		if (_activeMonsters.Count == 0)
		{
			GD.Print("所有怪物已被击败!");
			OnAllMonstersDefeated();
		}
	}
} 
