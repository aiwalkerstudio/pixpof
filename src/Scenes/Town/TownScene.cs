using Godot;
using System;

public partial class TownScene : Node2D
{
	// 场景内的主要节点引用
	private Node2D _ground;
	private Node2D _interactiveAreas;
	private Node2D _npcs;
	private Control _quickAccessBar;
	private Control _dialogueSystem;
	private Control _characterUI;
	private Control _battleUI;

	public override void _Ready()
	{
		// 获取节点引用
		_ground = GetNode<Node2D>("Ground");
		_interactiveAreas = GetNode<Node2D>("InteractiveAreas");
		_npcs = GetNode<Node2D>("NPCs");
		_quickAccessBar = GetNode<Control>("UI/QuickAccessBar");
		_dialogueSystem = GetNode<Control>("UI/DialogueSystem");

		// 初始化场景
		InitializeScene();

		// 添加玩家
		var playerScene = GD.Load<PackedScene>("res://scenes/player/Player.tscn");
		var player = playerScene.Instantiate<CharacterBody2D>();
		player.Position = new Vector2(100, 100); // 设置初始位置
		AddChild(player);

		// 加载角色界面
		var characterScene = GD.Load<PackedScene>("res://scenes/ui/character/CharacterUI.tscn");
		_characterUI = characterScene.Instantiate<Control>();
		GetNode<CanvasLayer>("UI").AddChild(_characterUI);
		_characterUI.Hide();

		// 加载战斗UI
		var battleScene = GD.Load<PackedScene>("res://scenes/ui/battle/BattleUI.tscn");
		_battleUI = battleScene.Instantiate<Control>();
		GetNode<CanvasLayer>("UI").AddChild(_battleUI);

		// 连接战斗UI信号
		if (_battleUI is BattleUI battleUI)
		{
			battleUI.AttackPressed += OnPlayerAttack;
			battleUI.SkillPressed += OnPlayerSkill;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_character")) // 需要在项目设置中添加快捷键映射
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

	private void InitializeScene()
	{
		// 初始化交互区域
		SetupInteractiveAreas();
		
		// 初始化NPC
		SetupNPCs();
		
		// 初始化UI
		SetupUI();
	}

	private void SetupInteractiveAreas()
	{
		// TODO: 添加传送点、商人摊位等交互区域
	}

	private void SetupNPCs()
	{
		// TODO: 添加NPC
	}

	private void SetupUI()
	{
		// TODO: 设置快捷访问栏
	}

	private void OnPlayerAttack()
	{
		// TODO: 处理玩家攻击
		GD.Print("Player Attack!");
	}

	private void OnPlayerSkill(int skillIndex)
	{
		// TODO: 处理玩家技能
		GD.Print($"Player Use Skill {skillIndex}!");
	}

	private void OnBattlePortalEntered()
	{
		// 切换到战斗场景
		var battleScene = GD.Load<PackedScene>("res://scenes/battle/BattleMap.tscn");
		var battleMap = battleScene.Instantiate<BattleMap>();
		
		// 连接战斗完成信号
		battleMap.BattleCompleted += OnBattleCompleted;
		
		// 替换当前场景
		GetTree().Root.AddChild(battleMap);
		QueueFree();
	}

	private void OnBattleCompleted()
	{
		// 返回主城
		var townScene = GD.Load<PackedScene>("res://scenes/town/TownScene.tscn");
		var town = townScene.Instantiate<TownScene>();
		
		GetTree().Root.AddChild(town);
		QueueFree();
	}
} 
