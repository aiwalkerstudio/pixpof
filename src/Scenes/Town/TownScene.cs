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
} 