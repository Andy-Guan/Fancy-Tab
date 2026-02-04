using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using FancyTab.Models;
using Microsoft.Win32;

namespace FancyTab.Services;

/// <summary>
/// 文件服务 - 处理文件的保存和加载
/// </summary>
public class FileService
{
    /// <summary>
    /// JSON文件扩展名
    /// </summary>
    public const string JsonExtension = ".json";

    /// <summary>
    /// 自定义格式扩展名
    /// </summary>
    public const string GtabExtension = ".gtab";

    /// <summary>
    /// 文件过滤器
    /// </summary>
    public static string FileFilter => "Guitar Tab Files (*.gtab)|*.gtab|JSON Files (*.json)|*.json|All Files (*.*)|*.*";

    /// <summary>
    /// 当前文件路径
    /// </summary>
    public string? CurrentFilePath { get; private set; }

    /// <summary>
    /// 是否有未保存的更改
    /// </summary>
    public bool HasUnsavedChanges { get; set; }

    /// <summary>
    /// 保存歌曲到文件
    /// </summary>
    public async Task<bool> SaveAsync(Song song, string? filePath = null)
    {
        filePath ??= CurrentFilePath;

        if (string.IsNullOrEmpty(filePath))
        {
            return await SaveAsAsync(song);
        }

        try
        {
            song.ModifiedAt = DateTime.Now;
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == GtabExtension)
            {
                await SaveGtabAsync(song, filePath);
            }
            else
            {
                await SaveJsonAsync(song, filePath);
            }

            CurrentFilePath = filePath;
            HasUnsavedChanges = false;
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 另存为
    /// </summary>
    public async Task<bool> SaveAsAsync(Song song)
    {
        var dialog = new SaveFileDialog
        {
            Filter = FileFilter,
            DefaultExt = GtabExtension,
            FileName = SanitizeFileName(song.Title),
            AddExtension = true
        };

        if (dialog.ShowDialog() == true)
        {
            return await SaveAsync(song, dialog.FileName);
        }

        return false;
    }

    /// <summary>
    /// 加载歌曲
    /// </summary>
    public async Task<Song?> LoadAsync(string? filePath = null)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            var dialog = new OpenFileDialog
            {
                Filter = FileFilter,
                DefaultExt = GtabExtension
            };

            if (dialog.ShowDialog() != true)
            {
                return null;
            }

            filePath = dialog.FileName;
        }

        try
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            Song? song;

            if (extension == GtabExtension)
            {
                song = await LoadGtabAsync(filePath);
            }
            else
            {
                song = await LoadJsonAsync(filePath);
            }

            if (song != null)
            {
                CurrentFilePath = filePath;
                HasUnsavedChanges = false;
            }

            return song;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 创建新文件
    /// </summary>
    public Song CreateNew(string title = "Untitled")
    {
        CurrentFilePath = null;
        HasUnsavedChanges = false;
        return Song.CreateNew(title);
    }

    #region JSON格式

    private static async Task SaveJsonAsync(Song song, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string json = JsonSerializer.Serialize(song, options);
        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
    }

    private static async Task<Song?> LoadJsonAsync(string filePath)
    {
        string json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<Song>(json, options);
    }

    #endregion

    #region GTAB格式 (压缩的JSON)

    private static async Task SaveGtabAsync(Song song, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string json = JsonSerializer.Serialize(song, options);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        await using var fileStream = File.Create(filePath);
        await using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        await gzipStream.WriteAsync(jsonBytes);
    }

    private static async Task<Song?> LoadGtabAsync(string filePath)
    {
        await using var fileStream = File.OpenRead(filePath);
        await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream, Encoding.UTF8);

        string json = await reader.ReadToEndAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<Song>(json, options);
    }

    #endregion

    /// <summary>
    /// 清理文件名
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(fileName);
        foreach (char c in invalidChars)
        {
            sb.Replace(c, '_');
        }
        return sb.ToString();
    }
}
