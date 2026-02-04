using System.Windows;

namespace FancyTab.Services;

/// <summary>
/// 多语言服务
/// </summary>
public class LocalizationService
{
    private string _currentLanguage = "zh-CN";

    /// <summary>
    /// 当前语言
    /// </summary>
    public string CurrentLanguage => _currentLanguage;

    /// <summary>
    /// 语言改变事件
    /// </summary>
    public event EventHandler? LanguageChanged;

    /// <summary>
    /// 可用语言
    /// </summary>
    public static Dictionary<string, string> AvailableLanguages => new()
    {
        ["zh-CN"] = "简体中文",
        ["en-US"] = "English"
    };

    /// <summary>
    /// 设置语言
    /// </summary>
    public void SetLanguage(string languageCode)
    {
        if (_currentLanguage == languageCode) return;

        try
        {
            var dict = new ResourceDictionary();
            string resourcePath = languageCode switch
            {
                "zh-CN" => "Resources/Strings.zh-CN.xaml",
                "en-US" => "Resources/Strings.en-US.xaml",
                _ => "Resources/Strings.zh-CN.xaml"
            };

            dict.Source = new Uri(resourcePath, UriKind.Relative);

            // 移除旧的语言资源
            var oldDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("Strings.") == true);
            
            if (oldDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);
            }

            // 添加新的语言资源
            Application.Current.Resources.MergedDictionaries.Add(dict);

            _currentLanguage = languageCode;
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load language: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取本地化字符串
    /// </summary>
    public static string GetString(string key, string defaultValue = "")
    {
        try
        {
            var value = Application.Current.FindResource(key);
            return value?.ToString() ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 切换语言
    /// </summary>
    public void ToggleLanguage()
    {
        var languages = AvailableLanguages.Keys.ToList();
        int currentIndex = languages.IndexOf(_currentLanguage);
        int nextIndex = (currentIndex + 1) % languages.Count;
        SetLanguage(languages[nextIndex]);
    }
}
