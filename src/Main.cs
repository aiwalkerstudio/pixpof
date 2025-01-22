using Godot;
using System;

public partial class Main : Node
{
    private Player _player;
    private BattleUI _battleUI;
    private CharacterUI _characterUI;

    public override void _Ready()
    {
        // 创建UI
        CreateUI();
        
        // 创建玩家
        CreatePlayer();
        
        // 加载战斗地图
        LoadBattleMap();
    }

    private void CreateUI()
    {
        var uiNode = GetNode<CanvasLayer>("UI");
        
        // 加载战斗UI
        var battleUIScene = GD.Load<PackedScene>("res://scenes/ui/battle/BattleUI.tscn");
        _battleUI = battleUIScene.Instantiate<BattleUI>();
        uiNode.AddChild(_battleUI);
        
        // 加载角色UI
        var characterUIScene = GD.Load<PackedScene>("res://scenes/ui/character/CharacterUI.tscn");
        _characterUI = characterUIScene.Instantiate<CharacterUI>();
        uiNode.AddChild(_characterUI);
        _characterUI.Hide();
    }

    private void CreatePlayer()
    {
        var playerScene = GD.Load<PackedScene>("res://scenes/player/Player.tscn");
        _player = playerScene.Instantiate<Player>();
        AddChild(_player);
    }

    private void LoadBattleMap()
    {
        var battleScene = GD.Load<PackedScene>("res://scenes/battle/BattleMap.tscn");
        var battleMap = battleScene.Instantiate<BattleMap>();
        AddChild(battleMap);
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
} 