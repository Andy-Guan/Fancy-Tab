namespace FancyTab.Core.Models;

/// <summary>
/// 小节类
/// </summary>
public class Measure
{
    /// <summary>
    /// 小节编号
    /// </summary>
    public int Number { get; set; } = 1;

    /// <summary>
    /// 拍号分子 (每小节几拍)
    /// </summary>
    public int BeatsPerMeasure { get; set; } = 4;

    /// <summary>
    /// 拍号分母 (以几分音符为一拍)
    /// </summary>
    public int BeatUnit { get; set; } = 4;

    /// <summary>
    /// 小节内的音符列表
    /// </summary>
    public List<Note> Notes { get; set; } = new();

    /// <summary>
    /// 小节开始的和弦名称 (如 "Am", "G", "C")
    /// </summary>
    public string? ChordName { get; set; }

    /// <summary>
    /// 是否为重复开始
    /// </summary>
    public bool IsRepeatStart { get; set; } = false;

    /// <summary>
    /// 是否为重复结束
    /// </summary>
    public bool IsRepeatEnd { get; set; } = false;

    /// <summary>
    /// 重复次数
    /// </summary>
    public int RepeatCount { get; set; } = 2;

    /// <summary>
    /// 获取小节时值总长度 (以32分音符为单位)
    /// </summary>
    public int TotalTicks => BeatsPerMeasure * (32 / BeatUnit);

    /// <summary>
    /// 在指定位置获取指定弦的音符
    /// </summary>
    public Note? GetNoteAt(int position, int stringNumber)
    {
        return Notes.FirstOrDefault(n => n.Position == position && n.String == stringNumber);
    }

    /// <summary>
    /// 在指定位置获取所有音符 (和弦)
    /// </summary>
    public List<Note> GetNotesAtPosition(int position)
    {
        return Notes.Where(n => n.Position == position).ToList();
    }

    /// <summary>
    /// 添加音符
    /// </summary>
    public void AddNote(Note note)
    {
        // 移除同位置同弦的旧音符
        Notes.RemoveAll(n => n.Position == note.Position && n.String == note.String);
        Notes.Add(note);
        Notes = Notes.OrderBy(n => n.Position).ThenBy(n => n.String).ToList();
    }

    /// <summary>
    /// 删除音符
    /// </summary>
    public void RemoveNote(int position, int stringNumber)
    {
        Notes.RemoveAll(n => n.Position == position && n.String == stringNumber);
    }

    /// <summary>
    /// 克隆小节
    /// </summary>
    public Measure Clone()
    {
        return new Measure
        {
            Number = Number,
            BeatsPerMeasure = BeatsPerMeasure,
            BeatUnit = BeatUnit,
            Notes = Notes.Select(n => n.Clone()).ToList(),
            ChordName = ChordName,
            IsRepeatStart = IsRepeatStart,
            IsRepeatEnd = IsRepeatEnd,
            RepeatCount = RepeatCount
        };
    }
}
