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

    private float _currentHealth;
    private Player _target;
    private State _currentState = State.Idle;

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
        _target = GetNode<Player>("/root/Main/Player");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_currentState == State.Dead) return;

        UpdateState();
        UpdateBehavior(delta);
    }

    private void UpdateState()
    {
        if (_target == null) return;

        var distanceToTarget = GlobalPosition.DistanceTo(_target.GlobalPosition);

        if (distanceToTarget <= AttackRange)
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
                // 待机状态
                Velocity = Vector2.Zero;
                break;

            case State.Chase:
                // 追击玩家
                var direction = (_target.GlobalPosition - GlobalPosition).Normalized();
                Velocity = direction * MoveSpeed;
                break;

            case State.Attack:
                // 攻击玩家
                Attack();
                break;
        }

        MoveAndSlide();
    }

    private void Attack()
    {
        // TODO: 实现攻击逻辑
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _currentState = State.Dead;
        EmitSignal(SignalName.Died, this);
        QueueFree();
    }
} 