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

	public override void _Ready()
	{
		// 初始化翻译系统
		var translationManager = new TranslationManager();
		AddChild(translationManager);
		
		// 创建玩家（先创建）
		CreatePlayer();
		
		// 创建UI（后创建，并传入玩家引用）
		CreateUI();
		
		// 加载战斗地图
		LoadBattleMap();

		// 获取玩家实例
		_player = GetNode<Game.Player>("Player");  // 使用完整的命名空间路径
		if (_player == null)
		{
			GD.PrintErr("Player node not found!");
		}
	}

	private void CreateUI()
	{
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
} 
