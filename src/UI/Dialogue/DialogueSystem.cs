using Godot;
using System;

public partial class DialogueSystem : Control
{
    private Label _dialogueText;
    private Button _continueButton;
    
    public override void _Ready()
    {
        // 获取UI组件引用
        _dialogueText = GetNode<Label>("DialogueText");
        _continueButton = GetNode<Button>("ContinueButton");
        
        // 初始状态为隐藏
        Visible = false;
        
        // 连接按钮信号
        _continueButton.Pressed += OnContinuePressed;
    }

    public void ShowDialogue(string text)
    {
        _dialogueText.Text = text;
        Visible = true;
    }

    public void HideDialogue()
    {
        Visible = false;
    }

    private void OnContinuePressed()
    {
        HideDialogue();
    }
} 