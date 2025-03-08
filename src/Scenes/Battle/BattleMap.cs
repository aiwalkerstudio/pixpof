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
	private Node2D _monsters;
	private List<Monster> _activeMonsters = new();
	private Game.Player _player;  // 使用完整的命名空间路径
	private BattleUI _battleUI;
	private List<Enemy> _bosses = new();  // 改为Boss列表

	// 添加BOSS预制体引用
	[Export] 
	private PackedScene EaterOfWorldsScene;
	
	[Export]
	private PackedScene SearingExarchScene;

	// BOSS生成点
	[Export] 
	private Node2D BossSpawnPoint;

	[Signal]
	public delegate void BattleCompletedEventHandler();

	public override void _Ready()
	{
		_monsters = GetNode<Node2D>("Monsters");
		
		// 启用Y-sort，使得物体根据Y坐标自动排序
		_monsters.YSortEnabled = true;
	}

	// 新增：由Main调用的初始化方法
	public void Initialize(Game.Player player, BattleUI battleUI)  // 修改参数类型
	{
		_player = player;
		_battleUI = battleUI;

		// 初始化战斗场景
		InitializeBattle();
	}

	private void InitializeBattle()
	{
		try
		{
			// 生成玩家
			SpawnPlayer();
			
			// 生成怪物
			SpawnMonsters();

			// 加载曼陀罗Boss场景
			var mandraBossScene = GD.Load<PackedScene>("res://scenes/enemies/boss/MandraBoss.tscn");
			if (mandraBossScene != null)
			{
				var mandraBoss = mandraBossScene.Instantiate<MandraBoss>();
				SpawnBoss(mandraBoss, "曼陀罗Boss", new Vector2(700, 200));
				_bosses.Add(mandraBoss);
			}
			else
			{
				GD.PrintErr("Failed to load MandraBoss scene!");
			}

			// 加载野猪王Boss场景
			var boarBossScene = GD.Load<PackedScene>("res://scenes/enemies/boss/BoarKingBoss.tscn");
			if (boarBossScene != null)
			{
				var boarBoss = boarBossScene.Instantiate<BoarKingBoss>();
				SpawnBoss(boarBoss, "野猪王Boss", new Vector2(900, 400));
				_bosses.Add(boarBoss);
			}
			else
			{
				GD.PrintErr("Failed to load BoarKingBoss scene!");
			}

			SpawnBoss("SearingExarch");
			SpawnBoss("EaterOfWorlds");
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

	private void SpawnMonsters()
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

	private void SpawnMonsterAtPosition(string monsterType, Vector2 position)
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
				
				// 连接怪物信号
				monster.Died += OnMonsterDied;
				
				GD.Print($"Spawned {monsterType} at position {position}");
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

	// 获取随机生成位置
	private Vector2 GetRandomSpawnPosition()
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
			
			// 检查战斗是否结束
			if (_activeMonsters.Count == 0)
			{
				EmitSignal(SignalName.BattleCompleted);
			}
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

	private void OnBossDefeated()
	{
		// 检查是否所有Boss都被击败
		_bosses.RemoveAll(boss => !IsInstanceValid(boss));
		
		if (_bosses.Count == 0 && _activeMonsters.Count == 0)
		{
			EmitSignal(SignalName.BattleCompleted);
		}
	}

	// 生成BOSS
	public void SpawnBoss(string bossName)
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
				
				// 设置生成位置
				if(BossSpawnPoint != null)
				{
					boss.GlobalPosition = BossSpawnPoint.GlobalPosition;
				}
				else
				{
					GD.PrintErr($"BossSpawnPoint is null when spawning {bossName}");
				}
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
} 
