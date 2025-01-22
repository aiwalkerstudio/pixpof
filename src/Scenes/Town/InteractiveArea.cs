using Godot;
using System;

public partial class InteractiveArea : Area2D
{
    [Signal]
    public delegate void InteractionStartedEventHandler();

    [Signal] 
    public delegate void InteractionEndedEventHandler();

    [Export]
    public string AreaName { get; set; } = "";

    [Export]
    public bool IsInteractable { get; set; } = true;

    private Sprite2D _highlightSprite;
    
    public override void _Ready()
    {
        // 添加碰撞形状
        var collisionShape = new CollisionShape2D();
        AddChild(collisionShape);
        
        // 添加高亮效果精灵
        _highlightSprite = new Sprite2D();
        AddChild(_highlightSprite);
        _highlightSprite.Visible = false;

        // 连接信号
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!IsInteractable) return;
        
        if (body.IsInGroup("Player"))
        {
            _highlightSprite.Visible = true;
            EmitSignal(SignalName.InteractionStarted);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (!IsInteractable) return;
        
        if (body.IsInGroup("Player"))
        {
            _highlightSprite.Visible = false;
            EmitSignal(SignalName.InteractionEnded);
        }
    }
} 