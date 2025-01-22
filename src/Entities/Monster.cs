using Godot;
using System;

public partial class Monster : CharacterBody2D
{
    [Export]
    public float MaxHealth { get; set; } = 100;

    [Export]
    public float MoveSpeed { get; set; } = 100;

    [Export]
    public float AttackDamage { get; set; } = 10;

    [Export]
    public float AttackRange { get; set; } = 50;

    [Export]
    public float DetectionRange { get; set; } = 200;

    [Export]
    public float AttackCooldown { get; set; } = 1.0f; // 攻击冷却时间

    private float _currentHealth;
    private Player _target;
    private State _currentState = State.Idle;

    private float _attackTimer = 0.0f; // 攻击计时器
    private bool _canAttack = true; // 是否可以攻击
    private Area2D _attackArea; // 攻击判定区域

    [Signal]
    public delegate void DiedEventHandler(Monster monster);

    [Signal]
    public delegate void HealthChangedEventHandler(float currentHealth, float maxHealth);

    private enum State
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
        
        // 寻找玩家目标
        _target = GetPlayer();

        // 创建攻击判定区域
        CreateAttackArea();
    }

    private Player GetPlayer()
    {
        // 首先尝试在当前场景树中查找
        var player = GetTree().GetFirstNodeInGroup("Player") as Player;
        if (player != null)
        {
            GD.Print($"Found player at: {player.GetPath()}");
            return player;
        }

        // 如果找不到，尝试不同的路径
        var paths = new string[]
        {
            "/root/Main/Player",
            "../../Player",
            "../../../Player"
        };

        foreach (var path in paths)
        {
            try
            {
                player = GetNode<Player>(path);
                if (player != null)
                {
                    GD.Print($"Found player at path: {path}");
                    return player;
                }
            }
            catch (Exception)
            {
                GD.Print($"Could not find player at path: {path}");
            }
        }

        GD.PrintErr("Player not found in any location!");
        return null;
    }

    private void CreateAttackArea()
    {
        _attackArea = new Area2D();
        _attackArea.CollisionLayer = 0;
        _attackArea.CollisionMask = 1; // 只检测玩家层

        var shape = new CollisionShape2D();
        var circle = new CircleShape2D();
        circle.Radius = AttackRange;
        shape.Shape = circle;
        
        _attackArea.AddChild(shape);
        AddChild(_attackArea);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_currentState == State.Dead) return;

        // 更新攻击冷却
        if (!_canAttack)
        {
            _attackTimer += (float)delta;
            if (_attackTimer >= AttackCooldown)
            {
                _canAttack = true;
                _attackTimer = 0;
            }
        }

        UpdateState();
        UpdateBehavior(delta);
    }

    private void UpdateState()
    {
        if (_target == null) return;

        var distanceToTarget = GlobalPosition.DistanceTo(_target.GlobalPosition);

        if (distanceToTarget <= AttackRange && _canAttack)
        {
            _currentState = State.Attack;
        }
        else if (distanceToTarget <= DetectionRange)
        {
            _currentState = State.Chase;
        }
        else
        {
            _currentState = State.Idle;
        }
    }

    private void UpdateBehavior(double delta)
    {
        switch (_currentState)
        {
            case State.Idle:
                // 待机状态 - 可以添加随机徘徊
                Velocity = Vector2.Zero;
                break;

            case State.Chase:
                // 追击玩家 - 添加预测和避障
                ChaseTarget(delta);
                break;

            case State.Attack:
                // 攻击玩家
                Attack();
                break;
        }

        MoveAndSlide();
    }

    private void ChaseTarget(double delta)
    {
        if (_target == null) return;

        // 计算到目标的方向
        var direction = (_target.GlobalPosition - GlobalPosition).Normalized();
        
        // 检查前方是否有障碍物
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            GlobalPosition,
            GlobalPosition + direction * DetectionRange,
            CollisionMask
        );
        var result = spaceState.IntersectRay(query);

        // 如果有障碍物，尝试绕过
        if (result.Count > 0)
        {
            // 简单的避障 - 可以改进为更复杂的寻路
            var normal = result["normal"].AsVector2();
            direction = (direction + normal).Normalized();
        }

        Velocity = direction * MoveSpeed;
    }

    private void Attack()
    {
        if (!_canAttack) return;

        // 获取攻击范围内的物体
        var overlappingBodies = _attackArea.GetOverlappingBodies();
        foreach (var body in overlappingBodies)
        {
            if (body is Player player)
            {
                // 对玩家造成伤害
                DealDamageToPlayer(player);
                break;
            }
        }

        // 开始攻击冷却
        _canAttack = false;
        _attackTimer = 0;

        // 播放攻击动画
        PlayAttackAnimation();
    }

    private void DealDamageToPlayer(Player player)
    {
        // TODO: 实现玩家受伤逻辑
        GD.Print($"Monster deals {AttackDamage} damage to player");

        // 创建伤害数字
        ShowDamageNumber(AttackDamage, player.GlobalPosition);
    }

    private void ShowDamageNumber(float damage, Vector2 position)
    {
        // 获取战斗UI来显示伤害数字
        var battleUI = GetNode<BattleUI>("/root/Main/UI/BattleUI");
        if (battleUI != null)
        {
            battleUI.ShowDamage(position, damage);
        }
    }

    private void PlayAttackAnimation()
    {
        // 创建简单的攻击动画效果
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 0, 0), 0.1f);
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1), 0.1f);
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);

        // 播放受击动画
        PlayHitAnimation();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void PlayHitAnimation()
    {
        // 创建简单的受击动画效果
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0.5f), 0.1f);
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1.0f), 0.1f);
    }

    private void Die()
    {
        _currentState = State.Dead;

        // 播放死亡动画
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
        tween.TweenCallback(Callable.From(() => {
            EmitSignal(SignalName.Died, this);
            QueueFree();
        }));
    }
} 