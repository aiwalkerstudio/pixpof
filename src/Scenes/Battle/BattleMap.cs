using Godot;
using System;
using System.Collections.Generic;

public partial class BattleMap : Node2D
{
	private Node2D _monsters;
	private List<Monster> _activeMonsters = new();
	private Player _player;
	private BattleUI _battleUI;

	[Signal]
	public delegate void BattleCompletedEventHandler();

	public override void _Ready()
	{
		_monsters = GetNode<Node2D>("Monsters");
		
		// 启用Y-sort，使得物体根据Y坐标自动排序
		_monsters.YSortEnabled = true;
	}

	// 新增：由Main调用的初始化方法
	public void Initialize(Player player, BattleUI battleUI)
	{
		_player = player;
		_battleUI = battleUI;

		// 初始化战斗场景
		InitializeBattle();
	}

	private void InitializeBattle()
	{
		// 生成玩家
		SpawnPlayer();
		
		// 生成怪物
		SpawnMonsters();
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
		// 生成普通怪物
		SpawnMonster("Monster1", "SpawnPoints/MonsterSpawn1");
		SpawnMonster("Monster1", "SpawnPoints/MonsterSpawn2");
		
		// TODO: 根据战斗进度生成BOSS
	}

	private void SpawnMonster(string monsterType, string spawnPointPath)
	{
		var spawnPoint = GetNode<Marker2D>(spawnPointPath);
		var monsterScene = GD.Load<PackedScene>($"res://scenes/monsters/{monsterType}.tscn");
		var monster = monsterScene.Instantiate<Monster>();
		
		monster.GlobalPosition = spawnPoint.GlobalPosition;
		_monsters.AddChild(monster);
		_activeMonsters.Add(monster);
		
		// 连接怪物信号
		monster.Died += OnMonsterDied;
	}

	private void OnMonsterDied(Monster monster)
	{
		_activeMonsters.Remove(monster);
		
		// 检查战斗是否结束
		if (_activeMonsters.Count == 0)
		{
			EmitSignal(SignalName.BattleCompleted);
		}
	}

	public override void _Process(double delta)
	{
		// 更新战斗状态
		UpdateBattle(delta);
	}

	private void UpdateBattle(double delta)
	{
		// 更新怪物AI
		foreach (var monster in _activeMonsters)
		{
			UpdateMonsterAI(monster, delta);
		}
	}

	private void UpdateMonsterAI(Monster monster, double delta)
	{
		// TODO: 实现怪物AI逻辑
	}
} 
