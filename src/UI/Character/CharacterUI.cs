using Godot;
using System;

public partial class CharacterUI : Control
{
	private Label _healthValue;
	private Label _attackValue;
	private Label _defenseValue;
	private Button _closeButton;
	private GridContainer _inventoryGrid;
	private Panel _weaponSlot;
	private Panel _armorSlot;

	public override void _Ready()
	{
		// 获取节点引用
		_healthValue = GetNode<Label>("HBoxContainer/StatsPanel/VBoxContainer/GridContainer/HealthValue");
		_attackValue = GetNode<Label>("HBoxContainer/StatsPanel/VBoxContainer/GridContainer/AttackValue");
		_defenseValue = GetNode<Label>("HBoxContainer/StatsPanel/VBoxContainer/GridContainer/DefenseValue");
		_closeButton = GetNode<Button>("CloseButton");
		_inventoryGrid = GetNode<GridContainer>("HBoxContainer/InventoryPanel/VBoxContainer/GridContainer");
		_weaponSlot = GetNode<Panel>("HBoxContainer/EquipmentPanel/VBoxContainer/WeaponSlot");
		_armorSlot = GetNode<Panel>("HBoxContainer/EquipmentPanel/VBoxContainer/ArmorSlot");

		// 连接信号
		_closeButton.Pressed += OnCloseButtonPressed;

		// 初始化UI
		InitializeUI();

		// 设置标签翻译key
		GetNode<Label>("HBoxContainer/EquipmentPanel/VBoxContainer/Label").Set("TranslationKey", "ui_equipment");
		GetNode<Label>("HBoxContainer/EquipmentPanel/VBoxContainer/WeaponSlot/Label").Set("TranslationKey", "ui_weapon");
		GetNode<Label>("HBoxContainer/EquipmentPanel/VBoxContainer/ArmorSlot/Label").Set("TranslationKey", "ui_armor");
		
		GetNode<Label>("HBoxContainer/StatsPanel/VBoxContainer/GridContainer/HealthLabel").Set("TranslationKey", "ui_health");
		GetNode<Label>("HBoxContainer/StatsPanel/VBoxContainer/GridContainer/AttackLabel").Set("TranslationKey", "ui_attack_power");
		GetNode<Label>("HBoxContainer/StatsPanel/VBoxContainer/GridContainer/DefenseLabel").Set("TranslationKey", "ui_defense");
		
		GetNode<Label>("HBoxContainer/InventoryPanel/VBoxContainer/Label").Set("TranslationKey", "ui_inventory");
		
		_closeButton.Set("TranslationKey", "ui_close");

		// 更新翻译
		UpdateTranslations();
	}

	private void InitializeUI()
	{
		// 初始化背包格子
		InitializeInventorySlots();
		
		// 初始化装备槽
		InitializeEquipmentSlots();
		
		// 更新属性显示
		UpdateStats();
	}

	private void InitializeInventorySlots()
	{
		// 创建5x5的物品格子
		for (int i = 0; i < 25; i++)
		{
			var slot = new Panel();
			slot.CustomMinimumSize = new Vector2(64, 64);
			_inventoryGrid.AddChild(slot);
		}
	}

	private void InitializeEquipmentSlots()
	{
		// 设置装备槽可拖放
		_weaponSlot.GuiInput += OnWeaponSlotGuiInput;
		_armorSlot.GuiInput += OnArmorSlotGuiInput;
	}

	private void UpdateStats()
	{
		// TODO: 从角色数据获取实际属性值
		_healthValue.Text = "100";
		_attackValue.Text = "10";
		_defenseValue.Text = "5";
	}

	private void OnCloseButtonPressed()
	{
		Hide();
	}

	private void OnWeaponSlotGuiInput(InputEvent @event)
	{
		// TODO: 处理武器装备槽的输入事件
	}

	private void OnArmorSlotGuiInput(InputEvent @event)
	{
		// TODO: 处理防具装备槽的输入事件
	}

	private void UpdateTranslations()
	{
		GetTree().CallGroup("Translatable", "UpdateTranslation");
	}
} 
