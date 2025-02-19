using Godot;
using System;

public partial class BattleUI : Control
{
	private ProgressBar _healthBar;
	private ProgressBar _manaBar;
	private HBoxContainer _buffContainer;
	private Control _skillBar;
	private Button _attackButton;
	private Button _skill1Button;
	private Button _skill2Button;
	private Control _damageContainer;
	private Panel _minimap;
	private SubViewport _minimapViewport;

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
		_damageContainer = GetNode<Control>("DamageContainer");
		_minimap = GetNode<Panel>("Minimap");
		_minimapViewport = GetNode<SubViewport>("Minimap/MinimapViewport");

		// 连接信号
		_attackButton.Pressed += OnAttackPressed;
		_skill1Button.Pressed += () => OnSkillPressed(0);
		_skill2Button.Pressed += () => OnSkillPressed(1);

		// 设置按钮翻译key
		_attackButton.AddToGroup("Translatable");
		_attackButton.Set("TranslationKey", "ui_attack");
		
		_skill1Button.AddToGroup("Translatable");
		_skill1Button.Set("TranslationKey", "ui_skill1");
		
		_skill2Button.AddToGroup("Translatable");
		_skill2Button.Set("TranslationKey", "ui_skill2");

		// 初始化UI
		InitializeUI();

		// 更新翻译
		UpdateTranslations();
	}

	private void InitializeUI()
	{
		// 初始化血条
		UpdateHealth(100, 100);

		// 初始化技能按钮
		UpdateSkillButtons();

		// 初始化小地图
		InitializeMinimap();
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

	private void OnAttackPressed()
	{
		EmitSignal(SignalName.AttackPressed);
	}

	private void OnSkillPressed(int skillIndex)
	{
		EmitSignal(SignalName.SkillPressed, skillIndex);
	}

	private void UpdateTranslations()
	{
		GetTree().CallGroup("Translatable", "UpdateTranslation");
	}
} 
