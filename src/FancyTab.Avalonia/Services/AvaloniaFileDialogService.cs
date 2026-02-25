using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FancyTab.Core.Services;

namespace FancyTab.Avalonia.Services;

/// <summary>
/// Avalonia 文件对话框服务实现
/// </summary>
public class AvaloniaFileDialogService : IFileDialogService
{
    private readonly Window? _parentWindow;

    public AvaloniaFileDialogService(Window? parentWindow = null)
    {
        _parentWindow = parentWindow;
    }

    public async Task<string?> ShowSaveFileDialogAsync(string defaultFileName, string filter)
    {
        var window = _parentWindow ?? GetMainWindow();
        if (window == null) return null;

        var storageProvider = window.StorageProvider;
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save File",
            SuggestedFileName = defaultFileName,
            FileTypeChoices = ParseFilter(filter)
        });

        return file?.Path.LocalPath;
    }

    public async Task<string?> ShowOpenFileDialogAsync(string filter)
    {
        var window = _parentWindow ?? GetMainWindow();
        if (window == null) return null;

        var storageProvider = window.StorageProvider;
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false,
            FileTypeFilter = ParseFilter(filter)
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is 
            global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    private static FilePickerFileType[] ParseFilter(string filter)
    {
        // 解析 "Description (*.ext)|*.ext" 格式
        var parts = filter.Split('|');
        var types = new List<FilePickerFileType>();

        for (int i = 0; i < parts.Length - 1; i += 2)
        {
            var name = parts[i].Trim();
            var patterns = parts[i + 1].Split(';')
                .Select(p => p.Trim())
                .ToArray();

            types.Add(new FilePickerFileType(name)
            {
                Patterns = patterns
            });
        }

        return types.ToArray();
    }
}
