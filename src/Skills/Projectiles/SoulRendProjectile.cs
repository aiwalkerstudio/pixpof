using Godot;
using System.Collections.Generic;

public class SoulRendProjectile : Area2D
{
    public float Speed { get; set; }
    public float Damage { get; set; }
    public float DotDamage { get; set; }
    public float LifeLeechPercent { get; set; }
    public float TrackingRange { get; set; }
    public float AreaDamageRadius { get; set; }
    public Vector2 Direction { get; set; }
    public Node Source { get; set; }
    
    private HashSet<Node2D> _hitTargets = new HashSet<Node2D>();
    private float _lifetime = 3.0f;
    
    public override void _Ready()
    {
        // 设置碰撞检测
        CollisionLayer = 4; // 投射物层
        CollisionMask = 2;  // 敌人层
        
        // 添加碰撞形状
        var shape = new CircleShape2D();
        shape.Radius = 8f;
        var collision = new CollisionShape2D();
        collision.Shape = shape;
        AddChild(collision);
        
        // 添加视觉效果
        var sprite = new ColorRect();
        sprite.Color = new Color(0.8f, 0.2f, 0.8f); // 紫色
        sprite.Size = new Vector2(16, 16);
        sprite.Position = new Vector2(-8, -8);
        AddChild(sprite);
        
        // 连接信号
        BodyEntered += OnBodyEntered;
    }
    
    public override void _Process(double delta)
    {
        _lifetime -= (float)delta;
        if (_lifetime <= 0)
        {
            QueueFree();
            return;
        }
        
        // 追踪逻辑
        var nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {
            var targetDir = (nearestEnemy.GlobalPosition - GlobalPosition).Normalized();
            Direction = Direction.Lerp(targetDir, 0.1f);
        }
        
        // 移动
        Position += Direction * Speed * (float)delta;
        
        // 范围伤害
        ApplyAreaDamage();
    }
    
    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy && !_hitTargets.Contains(body))
        {
            _hitTargets.Add(body);
            
            // 直接伤害
            enemy.TakeDamage(Damage);
            
            // 持续伤害
            enemy.ApplyDotDamage(DotDamage, 3.0f);
            
            // 吸血效果
            if (Source is Player player)
            {
                player.AddEnergyShield(Damage * LifeLeechPercent);
            }
        }
    }
    
    private Node2D FindNearestEnemy()
    {
        Node2D nearest = null;
        float nearestDist = TrackingRange;
        
        foreach (var body in GetOverlappingBodies())
        {
            if (body is Enemy enemy && !_hitTargets.Contains(enemy))
            {
                float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (dist < nearestDist)
                {
                    nearest = enemy;
                    nearestDist = dist;
                }
            }
        }
        
        return nearest;
    }
    
    private void ApplyAreaDamage()
    {
        foreach (var body in GetOverlappingBodies())
        {
            if (body is Enemy enemy && !_hitTargets.Contains(enemy))
            {
                float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (dist <= AreaDamageRadius)
                {
                    enemy.TakeDamage(DotDamage * (float)GetProcessDeltaTime());
                }
            }
        }
    }
} 