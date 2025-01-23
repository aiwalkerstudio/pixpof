using Godot;
using System;

public partial class TranslatableLabel : Label
{
	[Export]
	public string TranslationCategory { get; set; } = "ui";
	
	[Export]
	public string TranslationKey { get; set; }

	public override void _Ready()
	{
		AddToGroup("Translatable");
		UpdateTranslation();
	}

	public virtual void UpdateTranslation()
	{
		if (!string.IsNullOrEmpty(TranslationKey))
		{
			Text = TranslationManager.Instance.GetTranslation(TranslationCategory, TranslationKey);
		}
	}
}

public partial class TranslatableButton : Button
{
	[Export]
	public string TranslationCategory { get; set; } = "ui";
	
	[Export]
	public string TranslationKey { get; set; }

	public override void _Ready()
	{
		AddToGroup("Translatable");
		UpdateTranslation();
	}

	public virtual void UpdateTranslation()
	{
		if (!string.IsNullOrEmpty(TranslationKey))
		{
			Text = TranslationManager.Instance.GetTranslation(TranslationCategory, TranslationKey);
		}
	}
} 
