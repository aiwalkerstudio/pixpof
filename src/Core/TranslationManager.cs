using Godot;
using System;
using System.Collections.Generic;

public partial class TranslationManager : Node
{
	private static TranslationManager _instance;
	public static TranslationManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new TranslationManager();
			}
			return _instance;
		}
	}

	private Dictionary<string, Dictionary<string, string>> _translations = new();
	private string _currentLocale = "en_US";

	public override void _Ready()
	{
		// 设置单例实例
		_instance = this;
		
		// 加载翻译文件
		LoadTranslations();
		
		// 设置初始语言
		SetLocale(_currentLocale);
	}

	public void LoadTranslations()
	{
		// 加载UI翻译
		var uiTranslation = LoadTranslationFile("res://assets/i18n/translations/source/ui.csv");
		_translations["ui"] = uiTranslation;
		
		// TODO: 加载其他翻译文件
		// _translations["items"] = LoadTranslationFile("res://assets/i18n/translations/source/items.csv");
		// _translations["skills"] = LoadTranslationFile("res://assets/i18n/translations/source/skills.csv");
		// _translations["dialogs"] = LoadTranslationFile("res://assets/i18n/translations/source/dialogs.csv");
	}

	private Dictionary<string, string> LoadTranslationFile(string path)
	{
		var translations = new Dictionary<string, string>();
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		
		if (file == null)
		{
			GD.PrintErr($"Failed to load translation file: {path}");
			return translations;
		}

		// 读取CSV头部
		var headers = file.GetCsvLine();
		var localeIndex = Array.IndexOf(headers, _currentLocale);
		
		if (localeIndex == -1)
		{
			GD.PrintErr($"Locale {_currentLocale} not found in translation file");
			return translations;
		}

		// 读取翻译内容
		while (!file.EofReached())
		{
			var line = file.GetCsvLine();
			if (line != null && line.Length > localeIndex)
			{
				translations[line[0]] = line[localeIndex];
			}
		}

		return translations;
	}

	public void SetLocale(string locale)
	{
		_currentLocale = locale;
		LoadTranslations();
		
		// 通知所有UI更新文本
		GetTree().CallGroup("Translatable", "UpdateTranslation");
	}

	public string GetTranslation(string category, string key)
	{
		if (_translations.ContainsKey(category) && _translations[category].ContainsKey(key))
		{
			return _translations[category][key];
		}
		return key;
	}
} 
