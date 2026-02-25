using System;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using FancyTab.Core.Services;

namespace FancyTab.Avalonia.Services;

/// <summary>
/// Avalonia 本地化服务实现
/// </summary>
public class AvaloniaLocalizationService : ILocalizationService
{
    private string _currentLanguage = "zh-CN";
    private ResourceInclude? _currentResourceDict;

    public string CurrentLanguage => _currentLanguage;

    public event EventHandler? LanguageChanged;

    public static Dictionary<string, string> AvailableLanguages => new()
    {
        ["zh-CN"] = "简体中文",
        ["en-US"] = "English"
    };

    public void SetLanguage(string languageCode)
    {
        if (_currentLanguage == languageCode) return;

        try
        {
            var app = Application.Current;
            if (app == null) return;

            // 移除旧的语言资源
            if (_currentResourceDict != null)
            {
                app.Resources.MergedDictionaries.Remove(_currentResourceDict);
            }

            // 创建新的资源引用
            string resourcePath = languageCode switch
            {
                "zh-CN" => "avares://FancyTab.Avalonia/Resources/Strings.zh-CN.axaml",
                "en-US" => "avares://FancyTab.Avalonia/Resources/Strings.en-US.axaml",
                _ => "avares://FancyTab.Avalonia/Resources/Strings.zh-CN.axaml"
            };

            _currentResourceDict = new ResourceInclude(new Uri(resourcePath))
            {
                Source = new Uri(resourcePath)
            };

            app.Resources.MergedDictionaries.Add(_currentResourceDict);

            _currentLanguage = languageCode;
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load language: {ex.Message}");
        }
    }

    public string GetString(string key, string defaultValue = "")
    {
        try
        {
            var app = Application.Current;
            if (app != null && app.TryGetResource(key, app.ActualThemeVariant, out var value) && value is string str)
            {
                return str;
            }
        }
        catch
        {
            // 忽略错误
        }

        return defaultValue;
    }

    public void ToggleLanguage()
    {
        var languages = AvailableLanguages.Keys.ToList();
        int currentIndex = languages.IndexOf(_currentLanguage);
        int nextIndex = (currentIndex + 1) % languages.Count;
        SetLanguage(languages[nextIndex]);
    }
}
