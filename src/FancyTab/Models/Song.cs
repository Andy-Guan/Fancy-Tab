using System.Text.Json;
using System.Text.Json.Serialization;

namespace FancyTab.Models;

/// <summary>
/// 歌曲类 - 顶层数据模型
/// </summary>
public class Song
{
    /// <summary>
    /// 文件格式版本
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// 歌曲标题
    /// </summary>
    public string Title { get; set; } = "Untitled";

    /// <summary>
    /// 艺术家/作曲者
    /// </summary>
    public string Artist { get; set; } = "";

    /// <summary>
    /// 专辑
    /// </summary>
    public string Album { get; set; } = "";

    /// <summary>
    /// 速度 (BPM)
    /// </summary>
    public int Tempo { get; set; } = 120;

    /// <summary>
    /// 调弦设置
    /// </summary>
    public Tuning Tuning { get; set; } = Tuning.Standard;

    /// <summary>
    /// 变调夹位置 (0表示不使用)
    /// </summary>
    public int Capo { get; set; } = 0;

    /// <summary>
    /// 小节列表
    /// </summary>
    public List<Measure> Measures { get; set; } = new();

    /// <summary>
    /// 使用的和弦列表
    /// </summary>
    public List<Chord> Chords { get; set; } = new();

    /// <summary>
    /// 备注
    /// </summary>
    public string Notes { get; set; } = "";

    /// <summary>
    /// 创建日期
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 修改日期
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建新歌曲
    /// </summary>
    public static Song CreateNew(string title = "Untitled", int initialMeasures = 4)
    {
        var song = new Song
        {
            Title = title,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now
        };

        // 创建初始小节
        for (int i = 0; i < initialMeasures; i++)
        {
            song.Measures.Add(new Measure { Number = i + 1 });
        }

        return song;
    }

    /// <summary>
    /// 添加小节
    /// </summary>
    public Measure AddMeasure()
    {
        var lastMeasure = Measures.LastOrDefault();
        var newMeasure = new Measure
        {
            Number = Measures.Count + 1,
            BeatsPerMeasure = lastMeasure?.BeatsPerMeasure ?? 4,
            BeatUnit = lastMeasure?.BeatUnit ?? 4
        };
        Measures.Add(newMeasure);
        ModifiedAt = DateTime.Now;
        return newMeasure;
    }

    /// <summary>
    /// 插入小节
    /// </summary>
    public Measure InsertMeasure(int index)
    {
        var refMeasure = Measures.ElementAtOrDefault(index) ?? Measures.LastOrDefault();
        var newMeasure = new Measure
        {
            Number = index + 1,
            BeatsPerMeasure = refMeasure?.BeatsPerMeasure ?? 4,
            BeatUnit = refMeasure?.BeatUnit ?? 4
        };
        Measures.Insert(index, newMeasure);
        
        // 重新编号
        for (int i = 0; i < Measures.Count; i++)
        {
            Measures[i].Number = i + 1;
        }
        
        ModifiedAt = DateTime.Now;
        return newMeasure;
    }

    /// <summary>
    /// 删除小节
    /// </summary>
    public void RemoveMeasure(int index)
    {
        if (index >= 0 && index < Measures.Count && Measures.Count > 1)
        {
            Measures.RemoveAt(index);
            
            // 重新编号
            for (int i = 0; i < Measures.Count; i++)
            {
                Measures[i].Number = i + 1;
            }
            
            ModifiedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// 序列化为JSON
    /// </summary>
    public string ToJson(bool indented = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        return JsonSerializer.Serialize(this, options);
    }

    /// <summary>
    /// 从JSON反序列化
    /// </summary>
    public static Song? FromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<Song>(json, options);
    }

    /// <summary>
    /// 克隆歌曲
    /// </summary>
    public Song Clone()
    {
        var json = ToJson();
        return FromJson(json) ?? new Song();
    }
}
