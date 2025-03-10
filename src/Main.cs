using Godot;
using System;
using Game;  // 添加这行来引用 Game 命名空间
using Game.Enemies; 
using Game.UI.Battle;  // 添加命名空间引用

public partial class Main : Node
{
	private Game.Player _player;  // 使用完整的命名空间路径
	private BattleUI _battleUI;
	private CharacterUI _characterUI;
	private Control _mainMenu;
	private Button _survivalButton;
	private Button _simulacrumButton;
	private Button _bossRushButton;
	private Button _languageButton;
	private bool _isEnglish = true;

	public override void _Ready()
	{
		// 获取主菜单引用
		_mainMenu = GetNode<Control>("UI/MainMenu");
		_survivalButton = GetNode<Button>("UI/MainMenu/VBoxContainer/SurvivalButton");
		_simulacrumButton = GetNode<Button>("UI/MainMenu/VBoxContainer/SimulacrumButton");
		_bossRushButton = GetNode<Button>("UI/MainMenu/VBoxContainer/BossRushButton");
		_languageButton = GetNode<Button>("UI/MainMenu/LanguageButton");

		// 连接信号
		_survivalButton.Pressed += OnSurvivalPressed;
		_simulacrumButton.Pressed += OnSimulacrumPressed;
		_bossRushButton.Pressed += OnBossRushPressed;
		_languageButton.Pressed += OnLanguagePressed;

		// 初始化UI文本
		UpdateUIText();
		
		// 初始化翻译系统
		var translationManager = new TranslationManager();
		AddChild(translationManager);
		
		// 创建玩家（先创建）
		// CreatePlayer();
		
		// 创建UI（后创建，并传入玩家引用）
		// CreateUI();
		
		// 加载战斗地图
		// LoadBattleMap();

		// 获取玩家实例
		_player = GetNode<Game.Player>("Player");  // 使用完整的命名空间路径
		if (_player == null)
		{
			GD.PrintErr("Player node not found!");
		}
	}

	private void CreateUI()
	{
		if (_battleUI != null) return; // 避免重复创建

		// 创建战斗UI
		var battleUIScene = GD.Load<PackedScene>("res://scenes/ui/battle/BattleUI.tscn");
		_battleUI = battleUIScene.Instantiate<BattleUI>();
		GetNode<CanvasLayer>("UI").AddChild(_battleUI);
		
		// 初始化战斗UI
		_battleUI.Initialize(_player);  // 传入玩家引用
		
		// 加载角色UI
		var characterUIScene = GD.Load<PackedScene>("res://scenes/ui/character/CharacterUI.tscn");
		if (characterUIScene != null)
		{
			_characterUI = characterUIScene.Instantiate<CharacterUI>();
			GetNode<CanvasLayer>("UI").AddChild(_characterUI);
			_characterUI.Hide();
			GD.Print("CharacterUI created successfully");
		}
		else
		{
			GD.PrintErr("Failed to load CharacterUI scene!");
		}
	}

	private void CreatePlayer()
	{
		var playerScene = GD.Load<PackedScene>("res://scenes/player/Player.tscn");
		_player = playerScene.Instantiate<Game.Player>();
		// 设置玩家名称以便调试
		_player.Name = "Player";
		AddChild(_player);
		
		// 调试输出
		GD.Print($"Player created at: {_player.GetPath()}");
	}

	private void LoadBattleMap()
	{
		var battleScene = GD.Load<PackedScene>("res://scenes/battle/BattleMap.tscn");
		var battleMap = battleScene.Instantiate<BattleMap>();
		AddChild(battleMap);
		
		// 初始化战斗地图
		battleMap.Initialize(_player, _battleUI);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_character"))
		{
			if (_characterUI.Visible)
			{
				_characterUI.Hide();
			}
			else
			{
				_characterUI.Show();
			}
		}
	}

	public override void _Process(double delta)
	{
		// 主场景的逻辑处理
	}

	private void OnSurvivalPressed()
	{
		GD.Print("Starting Survival Gauntlet...");
		CreateUI();  // 创建UI
		LoadBattleScene("SurvivalGauntlet");
	}

	private void OnSimulacrumPressed()
	{
		GD.Print("Starting Simulacrum Tower...");
		CreateUI();  // 创建UI
		LoadBattleScene("SimulacrumTower");
		_mainMenu.Hide();
	}

	private void OnBossRushPressed()
	{
		GD.Print("Starting Boss Rush...");
		CreateUI();  // 创建UI
		LoadBattleScene("BossRush");
		_mainMenu.Hide();
	}

	private void OnLanguagePressed()
	{
		_isEnglish = !_isEnglish;
		UpdateUIText();
	}

	private void UpdateUIText()
	{
		if (_isEnglish)
		{
			_survivalButton.Text = "Survival Gauntlet";
			_simulacrumButton.Text = "Simulacrum Tower";
			_bossRushButton.Text = "Boss Rush";
			_languageButton.Text = "Switch to Chinese";
		}
		else
		{
			_survivalButton.Text = "生存挑战";
			_simulacrumButton.Text = "模拟回廊";
			_bossRushButton.Text = "Boss连战";
			_languageButton.Text = "Switch to English";
		}
	}

	private void LoadBattleScene(string battleType)
	{
		GD.Print($"Loading battle scene: {battleType}");
		
		// 先隐藏主菜单
		_mainMenu.Hide();
		
		// 清理旧的战斗场景
		foreach (var child in GetChildren())
		{
			if (child is BattleMap)
			{
				child.QueueFree();
				GD.Print($"Removed old battle map: {child.Name}");
			}
		}
		
		// 创建新的战斗场景
		PackedScene battleScene = null;
		
		switch (battleType)
		{
			case "SurvivalGauntlet":
				battleScene = GD.Load<PackedScene>("res://scenes/battle/SurvivalGauntlet.tscn");
				break;
			case "SimulacrumTower":
				battleScene = GD.Load<PackedScene>("res://scenes/battle/SimulacrumTower.tscn");
				break;
			case "BossRush":
				battleScene = GD.Load<PackedScene>("res://scenes/battle/BossRush.tscn");
				break;
			default:
				GD.PrintErr($"Unknown battle type: {battleType}");
				return;
		}
		
		if (battleScene == null)
		{
			GD.PrintErr($"Failed to load battle scene for type: {battleType}");
			return;
		}
		
		// 实例化战斗场景
		var battleMap = battleScene.Instantiate<BattleMap>();
		AddChild(battleMap);
		
		// 确保玩家可见
		if (_player != null)
		{
			_player.Show();
			_player.Position = new Vector2(500, 300); // 设置玩家初始位置
		}
		else
		{
			GD.PrintErr("Player is null when loading battle scene!");
			//CreatePlayer(); // 尝试重新创建玩家
		}
		
		// 确保UI可见
		if (_battleUI != null)
		{
			_battleUI.Show();
		}
		else
		{
			GD.PrintErr("BattleUI is null when loading battle scene!");
			CreateUI(); // 尝试重新创建UI
		}
		
		// 初始化战斗场景
		battleMap.Initialize(_player, _battleUI);
		
		// 连接战斗完成信号
		battleMap.BattleCompleted += OnBattleCompleted;
		
		GD.Print($"Battle scene loaded: {battleType}");
	}

	private void OnBattleCompleted()
	{
		GD.Print("Battle completed, returning to main menu...");
		
		// 使用CallDeferred确保在当前帧结束后执行
		CallDeferred(nameof(ShowMainMenu));
	}

	private void ShowMainMenu()
	{
		GD.Print("Showing main menu...");
		
		// 清理所有战斗场景
		foreach (var child in GetChildren())
		{
			if (child is BattleMap)
			{
				child.QueueFree();
				GD.Print($"Removed battle map: {child.Name}");
			}
		}
		
		// 隐藏玩家和UI
		if (IsInstanceValid(_player))
		{
			_player.Hide();
		}
		
		if (IsInstanceValid(_battleUI))
		{
			_battleUI.Hide();
		}
		
		// 返回主菜单
		_mainMenu.Show();
		GD.Print("Main menu shown");
	}

	// 添加处理玩家死亡的方法
	public void OnPlayerDied()
	{
		GD.Print("Main received player death notification");
		
		// 查找当前的战斗场景
		foreach (var child in GetChildren())
		{
			if (child is BattleMap battleMap)
			{
				GD.Print($"Found battle map: {battleMap.Name}, notifying about player death");
				battleMap.OnPlayerDied();
				return;
			}
		}
		
		GD.PrintErr("No battle map found in Main after player death");
		
		// 如果找不到战斗场景，直接返回主菜单
		ShowMainMenu();
	}
} 
