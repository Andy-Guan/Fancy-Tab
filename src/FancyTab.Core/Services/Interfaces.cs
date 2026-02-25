using FancyTab.Core.Models;

namespace FancyTab.Core.Services;

/// <summary>
/// 文件对话框服务接口
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// 显示保存文件对话框
    /// </summary>
    Task<string?> ShowSaveFileDialogAsync(string defaultFileName, string filter);
    
    /// <summary>
    /// 显示打开文件对话框
    /// </summary>
    Task<string?> ShowOpenFileDialogAsync(string filter);
}

/// <summary>
/// 本地化服务接口
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// 当前语言
    /// </summary>
    string CurrentLanguage { get; }
    
    /// <summary>
    /// 语言改变事件
    /// </summary>
    event EventHandler? LanguageChanged;
    
    /// <summary>
    /// 设置语言
    /// </summary>
    void SetLanguage(string languageCode);
    
    /// <summary>
    /// 切换语言
    /// </summary>
    void ToggleLanguage();
    
    /// <summary>
    /// 获取本地化字符串
    /// </summary>
    string GetString(string key, string defaultValue = "");
}

/// <summary>
/// PDF导出服务接口
/// </summary>
public interface IPdfExportService
{
    /// <summary>
    /// 导出为PDF
    /// </summary>
    Task<bool> ExportAsync(Song song, string? filePath = null);
}
