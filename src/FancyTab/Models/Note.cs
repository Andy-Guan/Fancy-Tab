namespace FancyTab.Models;

/// <summary>
/// 音符时值枚举
/// </summary>
public enum NoteDuration
{
    Whole = 1,          // 全音符
    Half = 2,           // 二分音符
    Quarter = 4,        // 四分音符
    Eighth = 8,         // 八分音符
    Sixteenth = 16,     // 十六分音符
    ThirtySecond = 32   // 三十二分音符
}

/// <summary>
/// 吉他技巧枚举
/// </summary>
[Flags]
public enum Technique
{
    None = 0,
    HammerOn = 1,       // 击弦 h
    PullOff = 2,        // 勾弦 p
    SlideUp = 4,        // 上滑音 /
    SlideDown = 8,      // 下滑音 \
    Bend = 16,          // 推弦 b
    Release = 32,       // 释放推弦 r
    Vibrato = 64,       // 揉弦 ~
    Harmonic = 128,     // 自然泛音 <>
    PinchHarmonic = 256,// 人工泛音
    Mute = 512,         // 闷音 x
    PalmMute = 1024,    // 手掌闷音 PM
    Tap = 2048,         // 点弦 t
    Trill = 4096,       // 颤音 tr
    LetRing = 8192      // 延音
}

/// <summary>
/// 音符类
/// </summary>
public class Note
{
    /// <summary>
    /// 弦号 (1-6, 1为最细弦e)
    /// </summary>
    public int String { get; set; } = 1;

    /// <summary>
    /// 品数 (0-24, 0为空弦)
    /// </summary>
    public int Fret { get; set; } = 0;

    /// <summary>
    /// 时值
    /// </summary>
    public NoteDuration Duration { get; set; } = NoteDuration.Quarter;

    /// <summary>
    /// 是否为附点音符
    /// </summary>
    public bool IsDotted { get; set; } = false;

    /// <summary>
    /// 是否为三连音的一部分
    /// </summary>
    public bool IsTriplet { get; set; } = false;

    /// <summary>
    /// 技巧
    /// </summary>
    public Technique Technique { get; set; } = Technique.None;

    /// <summary>
    /// 在小节内的位置 (以最小单位计算)
    /// </summary>
    public int Position { get; set; } = 0;

    /// <summary>
    /// 是否为休止符
    /// </summary>
    public bool IsRest { get; set; } = false;

    /// <summary>
    /// 推弦音高 (半音数)
    /// </summary>
    public double BendAmount { get; set; } = 0;

    /// <summary>
    /// 连接到的下一个音符 (用于连音线)
    /// </summary>
    public bool TiedToNext { get; set; } = false;

    /// <summary>
    /// 克隆音符
    /// </summary>
    public Note Clone()
    {
        return new Note
        {
            String = String,
            Fret = Fret,
            Duration = Duration,
            IsDotted = IsDotted,
            IsTriplet = IsTriplet,
            Technique = Technique,
            Position = Position,
            IsRest = IsRest,
            BendAmount = BendAmount,
            TiedToNext = TiedToNext
        };
    }
}
