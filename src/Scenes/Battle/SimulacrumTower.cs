using Godot;
using System.Collections.Generic;
using Game.Enemies;
using Game.Enemies.Boss;

public partial class SimulacrumTower : BattleMap
{
	private int _currentFloor = 0;
	private int _maxFloors = 10;
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

	// 添加标志位防止重复进入下一层
	private bool _isTransitioningToNextFloor = false;

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
		GD.Print($"开始加载第 {_currentFloor + 1} 层");
		
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
		
		// 如果已经在过渡到下一层，直接返回
		if (_isTransitioningToNextFloor)
		{
			GD.Print("已经在过渡到下一层，忽略重复调用");
			return;
		}
		
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
		
		// 设置过渡标志
		_isTransitioningToNextFloor = true;
		
		// 否则进入下一层
		GD.Print("进入下一层");
		
		// 使用CallDeferred延迟到下一帧执行，避免在当前帧中修改场景树
		CallDeferred(nameof(DelayedStartNextFloor));
	}

	// 添加延迟启动下一层的方法
	private void DelayedStartNextFloor()
	{
		// 创建一次性计时器
		var timer = GetTree().CreateTimer(2.0);
		timer.Timeout += () => {
			StartNextFloor();
			// 重置过渡标志
			_isTransitioningToNextFloor = false;
		};
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

	protected override void OnBossDefeated()
	{
		base.OnBossDefeated();
		
		GD.Print($"Boss被击败! 当前层数: {_currentFloor}/{_maxFloors}, 剩余Boss数: {_bosses.Count}");
		
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
		
		// 如果没有Boss了，检查是否可以进入下一层
		if (_bosses.Count == 0 && _activeMonsters.Count == 0)
		{
			GD.Print("所有Boss和怪物都被击败，准备进入下一层");
			
			// 检查是否已经完成所有层级
			if (_currentFloor >= _maxFloors)
			{
				GD.Print("已完成所有层级，发送战斗完成信号");
				Victory();
				return;
			}
			
			// 如果已经在过渡到下一层，直接返回
			if (_isTransitioningToNextFloor)
			{
				GD.Print("已经在过渡到下一层，忽略重复调用");
				return;
			}
			
			// 设置过渡标志
			_isTransitioningToNextFloor = true;
			
			// 进入下一层
			GD.Print("进入下一层");
			CallDeferred(nameof(DelayedStartNextFloor));
		}
	}

	protected new void SpawnRandomBoss()
	{
		string[] bosses = { "MandraBoss", "BoarKingBoss" };
		string randomBoss = bosses[GD.RandRange(0, bosses.Length - 1)];
		
		// 随机位置
		float x = (float)GD.RandRange(400, 600);
		float y = (float)GD.RandRange(250, 350);
		Vector2 position = new Vector2(x, y);
		
		GD.Print($"尝试生成Boss: {randomBoss} 在位置 {position}");
		
		try
		{
			// 加载Boss场景
			var bossScene = ResourceLoader.Load<PackedScene>($"res://scenes/enemies/boss/{randomBoss}.tscn");
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
				
				GD.Print($"成功生成Boss: {randomBoss} 在位置 {position}");
			}
			else
			{
				GD.PrintErr($"Failed to load {randomBoss} scene!");
			}
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Error spawning boss {randomBoss}: {e.Message}\n{e.StackTrace}");
		}
	}

	// 添加OnBossDied方法处理Boss的Died信号
	private void OnBossDied(Enemy enemy)
	{
		GD.Print($"收到Boss的Died信号: {enemy.Name}, ID: {enemy.GetInstanceId()}");
		
		// 从_bosses列表中移除
		_bosses.Remove(enemy);
		
		GD.Print($"Boss已从列表中移除，剩余Boss数: {_bosses.Count}");
		
		// 检查是否可以进入下一层
		if (_bosses.Count == 0 && _activeMonsters.Count == 0)
		{
			GD.Print("所有Boss和怪物都被击败，准备进入下一层");
			
			// 检查是否已经完成所有层级
			if (_currentFloor >= _maxFloors)
			{
				GD.Print("已完成所有层级，发送战斗完成信号");
				Victory();
				return;
			}
			
			// 如果已经在过渡到下一层，直接返回
			if (_isTransitioningToNextFloor)
			{
				GD.Print("已经在过渡到下一层，忽略重复调用");
				return;
			}
			
			// 设置过渡标志
			_isTransitioningToNextFloor = true;
			
			// 进入下一层
			GD.Print("进入下一层");
			CallDeferred(nameof(DelayedStartNextFloor));
		}
	}
} 
