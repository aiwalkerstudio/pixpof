using Godot;
using System;
using System.Collections.Generic;
using Game.Enemies;

namespace Game.Enemies.Boss
{
    public partial class BoarKingBoss : Enemy
    {
        [Signal]
        public delegate void BossDefeatedEventHandler();

        private enum BossState
        {
            Idle,
            Charging,    // 蓄力
            Rushing,     // 冲锋
            Summoning,   // 召唤小怪
            Stunned      // 撞墙眩晕
        }

        [Export]
        public float MoveSpeed { get; set; } = 100.0f;
        
        [Export]
        public float RushSpeed { get; set; } = 400.0f;
        
        [Export]
        public float ChargeTime { get; set; } = 1.5f;
        
        [Export]
        public float StunTime { get; set; } = 2.0f;
        
        [Export]
        public float SummonCooldown { get; set; } = 15.0f;

        private BossState _currentState = BossState.Idle;
        private float _stateTimer = 0f;
        private float _summonTimer = 0f;
        private Game.Player _target;
        private Vector2 _rushDirection = Vector2.Zero;
        private List<Monster> _summonedMinions = new();

        public override void _Ready()
        {
            base._Ready();
            MaxHealth = 2000.0f;
            AttackDamage = 30.0f;
            CurrentHealth = MaxHealth;
            
            // 设置碰撞
            CollisionLayer = 4;  // 敌人层
            CollisionMask = 3;   // 与玩家(1)和墙(2)碰撞
        }

        public override void _PhysicsProcess(double delta)
        {
            UpdateAI((float)delta);
        }

        private void UpdateAI(float delta)
        {
            if (_target == null)
            {
                _target = GetNode<Game.Player>("/root/Main/Player");
                if (_target == null) return;
            }

            UpdateTimers(delta);
            
            switch (_currentState)
            {
                case BossState.Idle:
                    HandleIdleState();
                    break;
                case BossState.Charging:
                    HandleChargingState();
                    break;
                case BossState.Rushing:
                    HandleRushingState();
                    break;
                case BossState.Summoning:
                    HandleSummoningState();
                    break;
                case BossState.Stunned:
                    HandleStunnedState();
                    break;
            }
        }

        private void UpdateTimers(float delta)
        {
            _stateTimer -= delta;
            _summonTimer -= delta;

            if (_stateTimer <= 0)
            {
                switch (_currentState)
                {
                    case BossState.Charging:
                        StartRushing();
                        break;
                    case BossState.Stunned:
                        _currentState = BossState.Idle;
                        break;
                }
            }
        }

        private void HandleIdleState()
        {
            // 在空闲状态下缓慢移动
            Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
            Velocity = direction * MoveSpeed;
            MoveAndSlide();

            // 检查是否可以召唤小怪
            if (_summonTimer <= 0)
            {
                StartSummoning();
                return;
            }

            // 随机开始冲锋
            if (GD.Randf() < 0.02) // 2%的概率每帧
            {
                StartCharging();
            }
        }

        private void HandleChargingState()
        {
            // 蓄力时面向玩家
            _rushDirection = (_target.GlobalPosition - GlobalPosition).Normalized();
            // 可以添加蓄力动画或特效
        }

        private void HandleRushingState()
        {
            // 沿着固定方向冲锋
            Velocity = _rushDirection * RushSpeed;
            var collision = MoveAndSlide();

            // 检查是否撞墙
            if (collision)
            {
                StartStunned();
            }
        }

        private void HandleSummoningState()
        {
            // 召唤2-3个小野猪
            int count = GD.RandRange(2, 3);
            for (int i = 0; i < count; i++)
            {
                SpawnMinion();
            }
            
            _currentState = BossState.Idle;
            _summonTimer = SummonCooldown;
        }

        private void HandleStunnedState()
        {
            // 眩晕状态下不移动
            Velocity = Vector2.Zero;
            // 可以添加眩晕动画或特效
        }

        private void StartCharging()
        {
            _currentState = BossState.Charging;
            _stateTimer = ChargeTime;
            GD.Print("Boss开始蓄力!");
        }

        private void StartRushing()
        {
            _currentState = BossState.Rushing;
            GD.Print("Boss开始冲锋!");
        }

        private void StartStunned()
        {
            _currentState = BossState.Stunned;
            _stateTimer = StunTime;
            GD.Print("Boss撞墙眩晕!");
        }

        private void StartSummoning()
        {
            _currentState = BossState.Summoning;
            GD.Print("Boss召唤小野猪!");
        }

        private void SpawnMinion()
        {
            // 在Boss周围随机位置生成小野猪
            var offset = new Vector2(
                (float)GD.RandRange(-100, 100),
                (float)GD.RandRange(-100, 100)
            );
            
            // TODO: 实例化小野猪场景
            // var minionScene = GD.Load<PackedScene>("res://scenes/enemies/BoarMinion.tscn");
            // var minion = minionScene.Instantiate<Monster>();
            // minion.GlobalPosition = GlobalPosition + offset;
            // GetParent().AddChild(minion);
            // _summonedMinions.Add(minion);
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            
            // 在受伤时有概率提前结束当前状态
            if (GD.Randf() < 0.3f && _currentState != BossState.Stunned)
            {
                _currentState = BossState.Idle;
            }

            if (CurrentHealth <= 0)
            {
                EmitSignal(SignalName.BossDefeated);
                QueueFree();
            }
        }
    }
} 