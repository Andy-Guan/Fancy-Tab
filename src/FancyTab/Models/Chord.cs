namespace FancyTab.Models;

/// <summary>
/// 和弦类
/// </summary>
public class Chord
{
    /// <summary>
    /// 和弦名称 (如 "Am", "G", "Cmaj7")
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 根音
    /// </summary>
    public string Root { get; set; } = "C";

    /// <summary>
    /// 和弦类型 (major, minor, 7, maj7, m7, dim, aug, sus2, sus4等)
    /// </summary>
    public string Type { get; set; } = "major";

    /// <summary>
    /// 六根弦的按法 (-1表示不弹, 0表示空弦, 1-24表示品数)
    /// </summary>
    public int[] Fingering { get; set; } = { -1, -1, -1, -1, -1, -1 };

    /// <summary>
    /// 起始品位 (用于高把位和弦)
    /// </summary>
    public int BaseFret { get; set; } = 1;

    /// <summary>
    /// 是否使用横按
    /// </summary>
    public bool HasBarre { get; set; } = false;

    /// <summary>
    /// 横按的弦范围 (起始弦, 结束弦)
    /// </summary>
    public (int Start, int End) BarreStrings { get; set; } = (1, 6);

    /// <summary>
    /// 常用和弦库
    /// </summary>
    public static Dictionary<string, Chord> CommonChords => new()
    {
        // 大三和弦
        ["C"] = new Chord { Name = "C", Root = "C", Type = "major", Fingering = new[] { 0, 1, 0, 2, 3, -1 } },
        ["D"] = new Chord { Name = "D", Root = "D", Type = "major", Fingering = new[] { 2, 3, 2, 0, -1, -1 } },
        ["E"] = new Chord { Name = "E", Root = "E", Type = "major", Fingering = new[] { 0, 0, 1, 2, 2, 0 } },
        ["F"] = new Chord { Name = "F", Root = "F", Type = "major", Fingering = new[] { 1, 1, 2, 3, 3, 1 }, HasBarre = true, BarreStrings = (1, 6) },
        ["G"] = new Chord { Name = "G", Root = "G", Type = "major", Fingering = new[] { 3, 0, 0, 0, 2, 3 } },
        ["A"] = new Chord { Name = "A", Root = "A", Type = "major", Fingering = new[] { 0, 2, 2, 2, 0, -1 } },
        ["B"] = new Chord { Name = "B", Root = "B", Type = "major", Fingering = new[] { 2, 4, 4, 4, 2, -1 }, BaseFret = 2, HasBarre = true },

        // 小三和弦
        ["Am"] = new Chord { Name = "Am", Root = "A", Type = "minor", Fingering = new[] { 0, 1, 2, 2, 0, -1 } },
        ["Bm"] = new Chord { Name = "Bm", Root = "B", Type = "minor", Fingering = new[] { 2, 3, 4, 4, 2, -1 }, BaseFret = 2, HasBarre = true },
        ["Cm"] = new Chord { Name = "Cm", Root = "C", Type = "minor", Fingering = new[] { 3, 4, 5, 5, 3, -1 }, BaseFret = 3, HasBarre = true },
        ["Dm"] = new Chord { Name = "Dm", Root = "D", Type = "minor", Fingering = new[] { 1, 3, 2, 0, -1, -1 } },
        ["Em"] = new Chord { Name = "Em", Root = "E", Type = "minor", Fingering = new[] { 0, 0, 0, 2, 2, 0 } },
        ["Fm"] = new Chord { Name = "Fm", Root = "F", Type = "minor", Fingering = new[] { 1, 1, 1, 3, 3, 1 }, HasBarre = true },
        ["Gm"] = new Chord { Name = "Gm", Root = "G", Type = "minor", Fingering = new[] { 3, 3, 3, 5, 5, 3 }, BaseFret = 3, HasBarre = true },

        // 七和弦
        ["C7"] = new Chord { Name = "C7", Root = "C", Type = "7", Fingering = new[] { 0, 1, 3, 2, 3, -1 } },
        ["D7"] = new Chord { Name = "D7", Root = "D", Type = "7", Fingering = new[] { 2, 1, 2, 0, -1, -1 } },
        ["E7"] = new Chord { Name = "E7", Root = "E", Type = "7", Fingering = new[] { 0, 0, 1, 0, 2, 0 } },
        ["G7"] = new Chord { Name = "G7", Root = "G", Type = "7", Fingering = new[] { 1, 0, 0, 0, 2, 3 } },
        ["A7"] = new Chord { Name = "A7", Root = "A", Type = "7", Fingering = new[] { 0, 2, 0, 2, 0, -1 } },

        // 大七和弦
        ["Cmaj7"] = new Chord { Name = "Cmaj7", Root = "C", Type = "maj7", Fingering = new[] { 0, 0, 0, 2, 3, -1 } },
        ["Fmaj7"] = new Chord { Name = "Fmaj7", Root = "F", Type = "maj7", Fingering = new[] { 0, 1, 2, 2, -1, -1 } },
        ["Gmaj7"] = new Chord { Name = "Gmaj7", Root = "G", Type = "maj7", Fingering = new[] { 2, 0, 0, 0, 2, 3 } },

        // 小七和弦
        ["Am7"] = new Chord { Name = "Am7", Root = "A", Type = "m7", Fingering = new[] { 0, 1, 0, 2, 0, -1 } },
        ["Dm7"] = new Chord { Name = "Dm7", Root = "D", Type = "m7", Fingering = new[] { 1, 1, 2, 0, -1, -1 } },
        ["Em7"] = new Chord { Name = "Em7", Root = "E", Type = "m7", Fingering = new[] { 0, 0, 0, 0, 2, 0 } },

        // 挂留和弦
        ["Dsus2"] = new Chord { Name = "Dsus2", Root = "D", Type = "sus2", Fingering = new[] { 0, 3, 2, 0, -1, -1 } },
        ["Dsus4"] = new Chord { Name = "Dsus4", Root = "D", Type = "sus4", Fingering = new[] { 3, 3, 2, 0, -1, -1 } },
        ["Asus2"] = new Chord { Name = "Asus2", Root = "A", Type = "sus2", Fingering = new[] { 0, 0, 2, 2, 0, -1 } },
        ["Asus4"] = new Chord { Name = "Asus4", Root = "A", Type = "sus4", Fingering = new[] { 0, 3, 2, 2, 0, -1 } },
    };

    /// <summary>
    /// 根据名称获取和弦
    /// </summary>
    public static Chord? GetByName(string name)
    {
        return CommonChords.TryGetValue(name, out var chord) ? chord.Clone() : null;
    }

    /// <summary>
    /// 克隆和弦
    /// </summary>
    public Chord Clone()
    {
        return new Chord
        {
            Name = Name,
            Root = Root,
            Type = Type,
            Fingering = (int[])Fingering.Clone(),
            BaseFret = BaseFret,
            HasBarre = HasBarre,
            BarreStrings = BarreStrings
        };
    }
}
