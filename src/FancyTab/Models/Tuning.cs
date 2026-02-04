namespace FancyTab.Models;

/// <summary>
/// 调弦设置类
/// </summary>
public class Tuning
{
    /// <summary>
    /// 调弦名称
    /// </summary>
    public string Name { get; set; } = "Standard";

    /// <summary>
    /// 六根弦的音高 (MIDI音符号, 从1弦到6弦)
    /// 标准调弦: E4(64), B3(59), G3(55), D3(50), A2(45), E2(40)
    /// </summary>
    public int[] StringPitches { get; set; } = { 64, 59, 55, 50, 45, 40 };

    /// <summary>
    /// 六根弦的音名
    /// </summary>
    public string[] StringNames => StringPitches.Select(MidiToNoteName).ToArray();

    /// <summary>
    /// 预设调弦
    /// </summary>
    public static Tuning Standard => new()
    {
        Name = "Standard",
        StringPitches = new[] { 64, 59, 55, 50, 45, 40 }
    };

    public static Tuning DropD => new()
    {
        Name = "Drop D",
        StringPitches = new[] { 64, 59, 55, 50, 45, 38 }
    };

    public static Tuning HalfStepDown => new()
    {
        Name = "Half Step Down",
        StringPitches = new[] { 63, 58, 54, 49, 44, 39 }
    };

    public static Tuning FullStepDown => new()
    {
        Name = "Full Step Down",
        StringPitches = new[] { 62, 57, 53, 48, 43, 38 }
    };

    public static Tuning OpenG => new()
    {
        Name = "Open G",
        StringPitches = new[] { 62, 59, 55, 50, 43, 38 }
    };

    public static Tuning OpenD => new()
    {
        Name = "Open D",
        StringPitches = new[] { 62, 57, 54, 50, 45, 38 }
    };

    public static Tuning DADGAD => new()
    {
        Name = "DADGAD",
        StringPitches = new[] { 62, 57, 55, 50, 45, 38 }
    };

    /// <summary>
    /// 所有预设调弦
    /// </summary>
    public static List<Tuning> Presets => new()
    {
        Standard, DropD, HalfStepDown, FullStepDown, OpenG, OpenD, DADGAD
    };

    /// <summary>
    /// 获取指定弦和品的实际音高 (MIDI音符号)
    /// </summary>
    public int GetPitch(int stringNumber, int fret)
    {
        if (stringNumber < 1 || stringNumber > 6) return 0;
        return StringPitches[stringNumber - 1] + fret;
    }

    /// <summary>
    /// 获取指定弦和品的音名
    /// </summary>
    public string GetNoteName(int stringNumber, int fret)
    {
        return MidiToNoteName(GetPitch(stringNumber, fret));
    }

    /// <summary>
    /// MIDI音符号转音名
    /// </summary>
    public static string MidiToNoteName(int midi)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int octave = (midi / 12) - 1;
        int noteIndex = midi % 12;
        return $"{noteNames[noteIndex]}{octave}";
    }

    /// <summary>
    /// 音名转MIDI音符号
    /// </summary>
    public static int NoteNameToMidi(string noteName)
    {
        var noteMap = new Dictionary<string, int>
        {
            {"C", 0}, {"C#", 1}, {"Db", 1}, {"D", 2}, {"D#", 3}, {"Eb", 3},
            {"E", 4}, {"F", 5}, {"F#", 6}, {"Gb", 6}, {"G", 7}, {"G#", 8},
            {"Ab", 8}, {"A", 9}, {"A#", 10}, {"Bb", 10}, {"B", 11}
        };

        // 解析音名和八度
        int octaveIndex = noteName.Length - 1;
        while (octaveIndex > 0 && char.IsDigit(noteName[octaveIndex - 1])) octaveIndex--;
        
        string note = noteName[..octaveIndex];
        if (!int.TryParse(noteName[octaveIndex..], out int octave)) octave = 4;

        if (noteMap.TryGetValue(note, out int noteValue))
        {
            return (octave + 1) * 12 + noteValue;
        }
        return 60; // 默认返回C4
    }

    /// <summary>
    /// 克隆调弦设置
    /// </summary>
    public Tuning Clone()
    {
        return new Tuning
        {
            Name = Name,
            StringPitches = (int[])StringPitches.Clone()
        };
    }
}
