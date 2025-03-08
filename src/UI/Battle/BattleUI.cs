using Godot;
using System;

namespace Game.UI.Battle
{
	public partial class BattleUI : Control
	{
		private ProgressBar _healthBar;
		private ProgressBar _manaBar;
		private HBoxContainer _buffContainer;
		private Control _skillBar;
		private Button _attackButton;
		private Button _skill1Button;
		private Button _skill2Button;
		private Button _skill3Button;
		private Control _damageContainer;
		private Panel _minimap;
		private SubViewport _minimapViewport;
		private Label _goldLabel;
		private Game.Player _player;

		[Signal]
		public delegate void AttackPressedEventHandler();

		[Signal]
		public delegate void SkillPressedEventHandler(int skillIndex);

		public override void _Ready()
		{
			// 获取节点引用
			_healthBar = GetNode<ProgressBar>("StatusBar/HealthBar");
			_manaBar = GetNode<ProgressBar>("StatusBar/ManaBar");
			_buffContainer = GetNode<HBoxContainer>("StatusBar/BuffContainer");
			_skillBar = GetNode<Control>("SkillBar");
			_attackButton = GetNode<Button>("SkillBar/HBoxContainer/AttackButton");
			_skill1Button = GetNode<Button>("SkillBar/HBoxContainer/Skill1Button");
			_skill2Button = GetNode<Button>("SkillBar/HBoxContainer/Skill2Button");
			_skill3Button = GetNode<Button>("SkillBar/HBoxContainer/Skill3Button");
			_damageContainer = GetNode<Control>("DamageContainer");
			_minimap = GetNode<Panel>("Minimap");
			_minimapViewport = GetNode<SubViewport>("Minimap/MinimapViewport");
			_goldLabel = GetNode<Label>("StatusBar/GoldLabel");

			// 连接按钮信号
			_attackButton.Pressed += OnAttackButtonPressed;
			_skill1Button.Pressed += OnSkill1ButtonPressed;
			_skill2Button.Pressed += OnSkill2ButtonPressed;
			_skill3Button.Pressed += OnSkill3ButtonPressed;

			// 设置按钮翻译key
			_attackButton.AddToGroup("Translatable");
			_attackButton.Set("TranslationKey", "ui_attack");
			
			_skill1Button.AddToGroup("Translatable");
			_skill1Button.Set("TranslationKey", "ui_skill1");
			
			_skill2Button.AddToGroup("Translatable");
			_skill2Button.Set("TranslationKey", "ui_skill2");

			_skill3Button.AddToGroup("Translatable");
			_skill3Button.Set("TranslationKey", "ui_skill3");

			// 初始化UI
			UpdateUI();
		}

		// 新增：初始化方法，由Main调用
		public void Initialize(Game.Player player)
		{
			_player = player;
			if (_player != null)
			{
				// 连接玩家信号
				_player.GoldChanged += OnGoldChanged;
				// 立即更新显示
				OnGoldChanged(_player.Gold);
			}
		}

		private void UpdateUI()
		{
			if (_player != null)
			{
				// 更新UI显示
				_healthBar.Value = _player.CurrentHealth;
				_healthBar.MaxValue = _player.MaxHealth;
				_manaBar.Value = _player.CurrentMana;
				_manaBar.MaxValue = _player.MaxMana;
				OnGoldChanged(_player.Gold);
			}
		}

		public void UpdateHealth(float current, float max)
		{
			_healthBar.MaxValue = max;
			_healthBar.Value = current;
		}

		public void UpdateMana(float current, float max)
		{
			_manaBar.MaxValue = max;
			_manaBar.Value = current;
		}

		public void ShowDamage(Vector2 position, float amount)
		{
			var damageLabel = new Label();
			damageLabel.Text = amount.ToString("F0");
			damageLabel.Position = position;
			
			_damageContainer.AddChild(damageLabel);

			// 创建动画
			var tween = CreateTween();
			tween.TweenProperty(damageLabel, "position", position + Vector2.Up * 50, 0.5f);
			tween.Parallel().TweenProperty(damageLabel, "modulate:a", 0.0f, 0.5f);
			tween.TweenCallback(Callable.From(() => damageLabel.QueueFree()));
		}

		public void AddBuff(string buffName, float duration)
		{
			var buffIcon = new TextureRect();
			// TODO: 设置buff图标
			_buffContainer.AddChild(buffIcon);

			// 创建持续时间计时器
			var timer = new Timer();
			timer.WaitTime = duration;
			timer.OneShot = true;
			timer.Timeout += () =>
			{
				buffIcon.QueueFree();
				timer.QueueFree();
			};
			AddChild(timer);
			timer.Start();
		}

		private void UpdateSkillButtons()
		{
			// TODO: 根据玩家技能更新按钮状态
		}

		private void InitializeMinimap()
		{
			// TODO: 初始化小地图显示
		}

		private void OnAttackButtonPressed()
		{
			EmitSignal(SignalName.AttackPressed);
		}

		private void OnSkill1ButtonPressed()
		{
			EmitSignal(SignalName.SkillPressed, 0);
		}

		private void OnSkill2ButtonPressed()
		{
			EmitSignal(SignalName.SkillPressed, 1);
		}

		private void OnSkill3ButtonPressed()
		{
			EmitSignal(SignalName.SkillPressed, 2);
		}


		private void UpdateTranslations()
		{
			GetTree().CallGroup("Translatable", "UpdateTranslation");
		}

		private void OnGoldChanged(int newAmount)
		{
			GD.Print($"before  BattleUI updated gold display: {newAmount}");
			if (_goldLabel != null)
			{
				_goldLabel.Text = $"Coins: {newAmount}";
				GD.Print($"BattleUI updated gold display: {newAmount}");
			}
		}
	}
} 
