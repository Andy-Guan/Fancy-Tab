using FancyTab.Core.Models;

namespace FancyTab.Core.Utils;

/// <summary>
/// 音符计算工具类
/// </summary>
public static class NoteCalculator
{
    /// <summary>
    /// 音名数组
    /// </summary>
    private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    private static readonly string[] NoteNamesFlat = { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" };

    /// <summary>
    /// 获取音符的MIDI编号
    /// </summary>
    public static int GetMidiNote(Tuning tuning, int stringNumber, int fret)
    {
        return tuning.GetPitch(stringNumber, fret);
    }

    /// <summary>
    /// 获取音符名称
    /// </summary>
    public static string GetNoteName(int midiNote, bool useFlat = false)
    {
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        var names = useFlat ? NoteNamesFlat : NoteNames;
        return $"{names[noteIndex]}{octave}";
    }

    /// <summary>
    /// 获取音符名称（不含八度）
    /// </summary>
    public static string GetNoteNameWithoutOctave(int midiNote, bool useFlat = false)
    {
        int noteIndex = midiNote % 12;
        var names = useFlat ? NoteNamesFlat : NoteNames;
        return names[noteIndex];
    }

    /// <summary>
    /// 获取音符的频率 (Hz)
    /// </summary>
    public static double GetFrequency(int midiNote)
    {
        return 440.0 * Math.Pow(2.0, (midiNote - 69) / 12.0);
    }

    /// <summary>
    /// 计算两个音符之间的半音数
    /// </summary>
    public static int GetInterval(int midiNote1, int midiNote2)
    {
        return Math.Abs(midiNote2 - midiNote1);
    }

    /// <summary>
    /// 获取音符时值对应的32分音符数量
    /// </summary>
    public static int GetTicksForDuration(NoteDuration duration, bool isDotted = false, bool isTriplet = false)
    {
        int baseTicks = duration switch
        {
            NoteDuration.Whole => 32,
            NoteDuration.Half => 16,
            NoteDuration.Quarter => 8,
            NoteDuration.Eighth => 4,
            NoteDuration.Sixteenth => 2,
            NoteDuration.ThirtySecond => 1,
            _ => 8
        };

        if (isDotted)
        {
            baseTicks = baseTicks + baseTicks / 2;
        }

        if (isTriplet)
        {
            baseTicks = baseTicks * 2 / 3;
        }

        return Math.Max(baseTicks, 1);
    }

    /// <summary>
    /// 获取时值的显示名称
    /// </summary>
    public static string GetDurationName(NoteDuration duration, string language = "zh-CN")
    {
        if (language.StartsWith("zh"))
        {
            return duration switch
            {
                NoteDuration.Whole => "全音符",
                NoteDuration.Half => "二分音符",
                NoteDuration.Quarter => "四分音符",
                NoteDuration.Eighth => "八分音符",
                NoteDuration.Sixteenth => "十六分音符",
                NoteDuration.ThirtySecond => "三十二分音符",
                _ => "四分音符"
            };
        }
        else
        {
            return duration switch
            {
                NoteDuration.Whole => "Whole",
                NoteDuration.Half => "Half",
                NoteDuration.Quarter => "Quarter",
                NoteDuration.Eighth => "Eighth",
                NoteDuration.Sixteenth => "Sixteenth",
                NoteDuration.ThirtySecond => "32nd",
                _ => "Quarter"
            };
        }
    }

    /// <summary>
    /// 获取技巧的显示符号
    /// </summary>
    public static string GetTechniqueSymbol(Technique technique)
    {
        if (technique.HasFlag(Technique.HammerOn)) return "h";
        if (technique.HasFlag(Technique.PullOff)) return "p";
        if (technique.HasFlag(Technique.SlideUp)) return "/";
        if (technique.HasFlag(Technique.SlideDown)) return "\\";
        if (technique.HasFlag(Technique.Bend)) return "b";
        if (technique.HasFlag(Technique.Release)) return "r";
        if (technique.HasFlag(Technique.Vibrato)) return "~";
        if (technique.HasFlag(Technique.Harmonic)) return "<>";
        if (technique.HasFlag(Technique.Mute)) return "x";
        if (technique.HasFlag(Technique.PalmMute)) return "PM";
        if (technique.HasFlag(Technique.Tap)) return "t";
        if (technique.HasFlag(Technique.Trill)) return "tr";
        return "";
    }

    /// <summary>
    /// 获取技巧的显示名称
    /// </summary>
    public static string GetTechniqueName(Technique technique, string language = "zh-CN")
    {
        if (language.StartsWith("zh"))
        {
            if (technique.HasFlag(Technique.HammerOn)) return "击弦";
            if (technique.HasFlag(Technique.PullOff)) return "勾弦";
            if (technique.HasFlag(Technique.SlideUp)) return "上滑音";
            if (technique.HasFlag(Technique.SlideDown)) return "下滑音";
            if (technique.HasFlag(Technique.Bend)) return "推弦";
            if (technique.HasFlag(Technique.Release)) return "释放";
            if (technique.HasFlag(Technique.Vibrato)) return "揉弦";
            if (technique.HasFlag(Technique.Harmonic)) return "泛音";
            if (technique.HasFlag(Technique.Mute)) return "闷音";
            if (technique.HasFlag(Technique.PalmMute)) return "手掌闷音";
            if (technique.HasFlag(Technique.Tap)) return "点弦";
            if (technique.HasFlag(Technique.Trill)) return "颤音";
            if (technique.HasFlag(Technique.LetRing)) return "延音";
            return "";
        }
        else
        {
            if (technique.HasFlag(Technique.HammerOn)) return "Hammer-on";
            if (technique.HasFlag(Technique.PullOff)) return "Pull-off";
            if (technique.HasFlag(Technique.SlideUp)) return "Slide Up";
            if (technique.HasFlag(Technique.SlideDown)) return "Slide Down";
            if (technique.HasFlag(Technique.Bend)) return "Bend";
            if (technique.HasFlag(Technique.Release)) return "Release";
            if (technique.HasFlag(Technique.Vibrato)) return "Vibrato";
            if (technique.HasFlag(Technique.Harmonic)) return "Harmonic";
            if (technique.HasFlag(Technique.Mute)) return "Mute";
            if (technique.HasFlag(Technique.PalmMute)) return "Palm Mute";
            if (technique.HasFlag(Technique.Tap)) return "Tap";
            if (technique.HasFlag(Technique.Trill)) return "Trill";
            if (technique.HasFlag(Technique.LetRing)) return "Let Ring";
            return "";
        }
    }
}
