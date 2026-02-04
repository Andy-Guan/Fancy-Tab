using System.Windows.Input;
using FancyTab.Models;

namespace FancyTab.Utils;

/// <summary>
/// 键盘输入处理器
/// </summary>
public class KeyboardHandler
{
    private string _pendingFretInput = "";
    private DateTime _lastKeyTime = DateTime.MinValue;
    private const int FretInputTimeout = 500; // 毫秒

    /// <summary>
    /// 处理按键，返回要执行的动作
    /// </summary>
    public KeyAction? HandleKey(Key key, ModifierKeys modifiers)
    {
        // 检查是否超时，重置输入缓冲
        if ((DateTime.Now - _lastKeyTime).TotalMilliseconds > FretInputTimeout)
        {
            _pendingFretInput = "";
        }
        _lastKeyTime = DateTime.Now;

        // 数字键 - 输入品数
        if (key >= Key.D0 && key <= Key.D9)
        {
            int digit = key - Key.D0;
            return HandleFretInput(digit);
        }
        if (key >= Key.NumPad0 && key <= Key.NumPad9)
        {
            int digit = key - Key.NumPad0;
            return HandleFretInput(digit);
        }

        // 方向键 - 移动光标
        switch (key)
        {
            case Key.Left:
                return new KeyAction(ActionType.MoveCursor, direction: Direction.Left);
            case Key.Right:
                return new KeyAction(ActionType.MoveCursor, direction: Direction.Right);
            case Key.Up:
                return new KeyAction(ActionType.MoveCursor, direction: Direction.Up);
            case Key.Down:
                return new KeyAction(ActionType.MoveCursor, direction: Direction.Down);
        }

        // 功能键
        switch (key)
        {
            case Key.Delete:
            case Key.Back:
                return new KeyAction(ActionType.DeleteNote);

            case Key.Enter:
                return new KeyAction(ActionType.MoveCursor, direction: Direction.Right);

            case Key.Space:
                return new KeyAction(ActionType.InsertRest);

            // 技巧键
            case Key.H:
                return new KeyAction(ActionType.SetTechnique, technique: Technique.HammerOn);
            case Key.P:
                return new KeyAction(ActionType.SetTechnique, technique: Technique.PullOff);
            case Key.S:
                if (modifiers == ModifierKeys.None)
                    return new KeyAction(ActionType.SetTechnique, technique: Technique.SlideUp);
                else if (modifiers == ModifierKeys.Shift)
                    return new KeyAction(ActionType.SetTechnique, technique: Technique.SlideDown);
                break;
            case Key.B:
                return new KeyAction(ActionType.SetTechnique, technique: Technique.Bend);
            case Key.V:
                return new KeyAction(ActionType.SetTechnique, technique: Technique.Vibrato);
            case Key.M:
                return new KeyAction(ActionType.SetTechnique, technique: Technique.Mute);
            case Key.T:
                return new KeyAction(ActionType.SetTechnique, technique: Technique.Tap);

            // 时值键
            case Key.W:
                return new KeyAction(ActionType.SetDuration, duration: NoteDuration.Whole);
            case Key.Q:
                if (modifiers == ModifierKeys.None)
                    return new KeyAction(ActionType.SetDuration, duration: NoteDuration.Quarter);
                break;
            case Key.E:
                if (modifiers == ModifierKeys.None)
                    return new KeyAction(ActionType.SetDuration, duration: NoteDuration.Eighth);
                break;

            // 泛音 (OemTilde 是 ~ 键)
            case Key.OemTilde:
                return new KeyAction(ActionType.SetTechnique, technique: Technique.Harmonic);

            // 滑音
            case Key.OemQuestion: // / 键
                return new KeyAction(ActionType.SetTechnique, technique: Technique.SlideUp);
            case Key.OemBackslash: // \ 键
                return new KeyAction(ActionType.SetTechnique, technique: Technique.SlideDown);

            // 小节操作
            case Key.Add:
            case Key.OemPlus:
                if (modifiers == ModifierKeys.Control)
                    return new KeyAction(ActionType.AddMeasure);
                break;
            case Key.Subtract:
            case Key.OemMinus:
                if (modifiers == ModifierKeys.Control)
                    return new KeyAction(ActionType.DeleteMeasure);
                break;

            // 附点
            case Key.OemPeriod:
                return new KeyAction(ActionType.ToggleDot);
        }

        return null;
    }

    private KeyAction HandleFretInput(int digit)
    {
        _pendingFretInput += digit.ToString();

        // 尝试解析品数
        if (int.TryParse(_pendingFretInput, out int fret))
        {
            // 如果品数合法且不太可能继续输入，立即执行
            if (fret > 24 || (_pendingFretInput.Length >= 2))
            {
                _pendingFretInput = "";
                if (fret > 24) fret = fret / 10; // 取第一位
                return new KeyAction(ActionType.InputFret, fret: Math.Min(fret, 24));
            }
            else if (fret >= 0 && fret <= 24)
            {
                // 单个数字，延迟确认
                return new KeyAction(ActionType.InputFret, fret: fret, isPending: fret < 10 && fret <= 2);
            }
        }

        _pendingFretInput = "";
        return null;
    }

    /// <summary>
    /// 确认待定的输入
    /// </summary>
    public void ConfirmPendingInput()
    {
        _pendingFretInput = "";
    }

    /// <summary>
    /// 清除待定输入
    /// </summary>
    public void ClearPending()
    {
        _pendingFretInput = "";
    }
}

/// <summary>
/// 键盘动作类型
/// </summary>
public enum ActionType
{
    InputFret,
    DeleteNote,
    MoveCursor,
    SetTechnique,
    SetDuration,
    InsertRest,
    AddMeasure,
    DeleteMeasure,
    ToggleDot
}

/// <summary>
/// 移动方向
/// </summary>
public enum Direction
{
    Left,
    Right,
    Up,
    Down
}

/// <summary>
/// 键盘动作
/// </summary>
public class KeyAction
{
    public ActionType Type { get; }
    public int? Fret { get; }
    public Direction? Direction { get; }
    public Technique? Technique { get; }
    public NoteDuration? Duration { get; }
    public bool IsPending { get; }

    public KeyAction(ActionType type, int? fret = null, Direction? direction = null,
        Technique? technique = null, NoteDuration? duration = null, bool isPending = false)
    {
        Type = type;
        Fret = fret;
        Direction = direction;
        Technique = technique;
        Duration = duration;
        IsPending = isPending;
    }
}
