using Godot;
using System;
using Game;  // 添加这行来引用 Game 命名空间

public partial class Main : Node
{
	private Game.Player _player;  // 使用完整的命名空间路径
	private BattleUI _battleUI;
	private CharacterUI _characterUI;
	private Monster _monster;

	public override void _Ready()
	{
		// 初始化翻译系统
		var translationManager = new TranslationManager();
		AddChild(translationManager);
		
		// 创建UI（最顶层）
		CreateUI();
		
		// 创建玩家（中间层）
		CreatePlayer();
		
		// 加载战斗地图（最底层）并初始化
		LoadBattleMap();

		// 获取玩家和怪物实例
		_player = GetNode<Game.Player>("Player");  // 使用完整的命名空间路径
		_monster = GetNode<Monster>("Monster");

		if (_player == null)
		{
			GD.PrintErr("Player node not found!");
		}

		if (_monster == null)
		{
			GD.PrintErr("Monster node not found!");
		}
	}

	private void CreateUI()
	{
		// 确保UI层存在
		var uiNode = GetNode<CanvasLayer>("UI");
		if (uiNode == null)
		{
			GD.PrintErr("UI CanvasLayer not found! Creating one...");
			uiNode = new CanvasLayer();
			uiNode.Name = "UI";
			AddChild(uiNode);
		}
		
		try
		{
			// 加载战斗UI
			var battleUIScene = GD.Load<PackedScene>("res://scenes/ui/battle/BattleUI.tscn");
			if (battleUIScene != null)
			{
				_battleUI = battleUIScene.Instantiate<BattleUI>();
				uiNode.AddChild(_battleUI);
				GD.Print("BattleUI created successfully");
			}
			else
			{
				GD.PrintErr("Failed to load BattleUI scene!");
			}
			
			// 加载角色UI
			var characterUIScene = GD.Load<PackedScene>("res://scenes/ui/character/CharacterUI.tscn");
			if (characterUIScene != null)
			{
				_characterUI = characterUIScene.Instantiate<CharacterUI>();
				uiNode.AddChild(_characterUI);
				_characterUI.Hide();
				GD.Print("CharacterUI created successfully");
			}
			else
			{
				GD.PrintErr("Failed to load CharacterUI scene!");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error creating UI: {e.Message}\n{e.StackTrace}");
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
} 
