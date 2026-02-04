using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FancyTab.Models;

namespace FancyTab.Controls;

/// <summary>
/// 六线谱画布控件
/// </summary>
public class TabCanvas : Control
{
    #region 依赖属性

    public static readonly DependencyProperty SongProperty =
        DependencyProperty.Register(nameof(Song), typeof(Song), typeof(TabCanvas),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnSongChanged));

    public static readonly DependencyProperty CurrentMeasureProperty =
        DependencyProperty.Register(nameof(CurrentMeasure), typeof(int), typeof(TabCanvas),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CurrentPositionProperty =
        DependencyProperty.Register(nameof(CurrentPosition), typeof(int), typeof(TabCanvas),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CurrentStringProperty =
        DependencyProperty.Register(nameof(CurrentString), typeof(int), typeof(TabCanvas),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowNoteNamesProperty =
        DependencyProperty.Register(nameof(ShowNoteNames), typeof(bool), typeof(TabCanvas),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MeasuresPerLineProperty =
        DependencyProperty.Register(nameof(MeasuresPerLine), typeof(int), typeof(TabCanvas),
            new FrameworkPropertyMetadata(4, FrameworkPropertyMetadataOptions.AffectsRender));

    public Song? Song
    {
        get => (Song?)GetValue(SongProperty);
        set => SetValue(SongProperty, value);
    }

    public int CurrentMeasure
    {
        get => (int)GetValue(CurrentMeasureProperty);
        set => SetValue(CurrentMeasureProperty, value);
    }

    public int CurrentPosition
    {
        get => (int)GetValue(CurrentPositionProperty);
        set => SetValue(CurrentPositionProperty, value);
    }

    public int CurrentString
    {
        get => (int)GetValue(CurrentStringProperty);
        set => SetValue(CurrentStringProperty, value);
    }

    public bool ShowNoteNames
    {
        get => (bool)GetValue(ShowNoteNamesProperty);
        set => SetValue(ShowNoteNamesProperty, value);
    }

    public int MeasuresPerLine
    {
        get => (int)GetValue(MeasuresPerLineProperty);
        set => SetValue(MeasuresPerLineProperty, value);
    }

    #endregion

    #region 事件

    public static readonly RoutedEvent NoteClickedEvent =
        EventManager.RegisterRoutedEvent(nameof(NoteClicked), RoutingStrategy.Bubble,
            typeof(EventHandler<NoteClickedEventArgs>), typeof(TabCanvas));

    public event EventHandler<NoteClickedEventArgs> NoteClicked
    {
        add => AddHandler(NoteClickedEvent, value);
        remove => RemoveHandler(NoteClickedEvent, value);
    }

    #endregion

    #region 绘制参数

    private const double StringSpacing = 20;        // 弦间距
    private const double NoteSpacing = 40;          // 音符间距
    private const double MeasureMargin = 20;        // 小节边距
    private const double LineMargin = 40;           // 行边距
    private const double StringLabelWidth = 30;     // 弦名标签宽度
    private const double LineHeight = StringSpacing * 7 + LineMargin + 30; // 行高度（增加节奏符号空间）

    private readonly Typeface _typeface = new("Consolas");
    private readonly Typeface _techniqueBoldTypeface = new(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
    private readonly Pen _stringPen = new(Brushes.Gray, 1);
    private readonly Pen _barLinePen = new(Brushes.DarkGray, 2);
    private readonly Pen _cursorPen = new(Brushes.DodgerBlue, 2);
    private readonly Pen _connectionPen = new(new SolidColorBrush(Color.FromRgb(220, 80, 60)), 2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
    private readonly Brush _noteBackground = new SolidColorBrush(Color.FromRgb(250, 250, 250));
    private readonly Brush _noteForeground = Brushes.Black;
    private readonly Brush _techniqueColor = new SolidColorBrush(Color.FromRgb(200, 60, 40));
    private readonly Brush _techniqueBgColor = new SolidColorBrush(Color.FromRgb(255, 240, 235));
    private readonly Brush _restColor = new SolidColorBrush(Color.FromRgb(100, 100, 100));
    private readonly Pen _durationStemPen = new(Brushes.Black, 1.5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
    private readonly Pen _durationBeamPen = new(Brushes.Black, 2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
    private readonly Brush _noteNameColor = new SolidColorBrush(Color.FromRgb(100, 149, 237));

    #endregion

    static TabCanvas()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TabCanvas),
            new FrameworkPropertyMetadata(typeof(TabCanvas)));
    }

    public TabCanvas()
    {
        Focusable = true;
        Background = Brushes.White;
        ClipToBounds = true;
    }

    private static void OnSongChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabCanvas canvas)
        {
            canvas.InvalidateVisual();
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // 绘制背景
        dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));

        if (Song == null || Song.Measures.Count == 0)
        {
            DrawEmptyMessage(dc);
            return;
        }

        DrawTablature(dc);
    }

    private void DrawEmptyMessage(DrawingContext dc)
    {
        var text = new FormattedText(
            "按数字键开始输入品数...",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            14,
            Brushes.Gray,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(text, new Point(50, 50));
    }

    private void DrawTablature(DrawingContext dc)
    {
        double y = LineMargin;
        int measureIndex = 0;

        while (measureIndex < Song!.Measures.Count)
        {
            // 计算本行能放多少小节
            int measuresThisLine = Math.Min(MeasuresPerLine, Song.Measures.Count - measureIndex);
            double measureWidth = (ActualWidth - StringLabelWidth - MeasureMargin * 2) / measuresThisLine;

            // 绘制弦名标签
            DrawStringLabels(dc, y);

            // 绘制六根弦
            DrawStrings(dc, y, StringLabelWidth, ActualWidth - MeasureMargin);

            // 绘制小节
            double x = StringLabelWidth + MeasureMargin;
            for (int i = 0; i < measuresThisLine; i++)
            {
                var measure = Song.Measures[measureIndex + i];
                DrawMeasure(dc, measure, measureIndex + i, x, y, measureWidth);
                x += measureWidth;
            }

            measureIndex += measuresThisLine;
            y += LineHeight;

            // 如果超出可视区域，停止绘制
            if (y > ActualHeight) break;
        }
    }

    private void DrawStringLabels(DrawingContext dc, double y)
    {
        var tuning = Song?.Tuning ?? Tuning.Standard;
        string[] labels = { "e", "B", "G", "D", "A", "E" };

        for (int i = 0; i < 6; i++)
        {
            string label = labels[i];
            var text = new FormattedText(
                label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                12,
                Brushes.DarkGray,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(text, new Point(10, y + i * StringSpacing - 6));
        }
    }

    private void DrawStrings(DrawingContext dc, double y, double startX, double endX)
    {
        for (int i = 0; i < 6; i++)
        {
            double stringY = y + i * StringSpacing;
            dc.DrawLine(_stringPen, new Point(startX, stringY), new Point(endX, stringY));
        }
    }

    private void DrawMeasure(DrawingContext dc, Measure measure, int measureIndex, double x, double y, double width)
    {
        // 绘制小节线
        dc.DrawLine(_barLinePen, new Point(x, y), new Point(x, y + 5 * StringSpacing));
        dc.DrawLine(_barLinePen, new Point(x + width, y), new Point(x + width, y + 5 * StringSpacing));

        // 绘制小节号
        var measureNumText = new FormattedText(
            measure.Number.ToString(),
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            10,
            Brushes.Gray,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
        dc.DrawText(measureNumText, new Point(x + 2, y - 15));

        // 绘制和弦名称
        if (!string.IsNullOrEmpty(measure.ChordName))
        {
            var chordText = new FormattedText(
                measure.ChordName,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                14,
                Brushes.DarkBlue,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(chordText, new Point(x + 5, y - 35));
        }

        // 计算每个位置的宽度
        int positions = measure.TotalTicks / 8; // 以8分音符为基本单位显示
        double positionWidth = (width - 10) / Math.Max(positions, 1);

        // 按弦分组绘制连接线
        var notesByString = measure.Notes
            .OrderBy(n => n.Position)
            .GroupBy(n => n.String);

        foreach (var stringGroup in notesByString)
        {
            var notes = stringGroup.ToList();
            for (int i = 0; i < notes.Count - 1; i++)
            {
                var currentNote = notes[i];
                var nextNote = notes[i + 1];

                // 检查是否需要绘制连接线（击弦、勾弦、滑音等连接技巧）
                bool needConnection = nextNote.Technique.HasFlag(Technique.HammerOn) ||
                                      nextNote.Technique.HasFlag(Technique.PullOff) ||
                                      nextNote.Technique.HasFlag(Technique.SlideUp) ||
                                      nextNote.Technique.HasFlag(Technique.SlideDown) ||
                                      currentNote.TiedToNext;

                if (needConnection && !currentNote.IsRest && !nextNote.IsRest)
                {
                    int displayPos1 = currentNote.Position / 8;
                    int displayPos2 = nextNote.Position / 8;
                    double x1 = x + 5 + displayPos1 * positionWidth + 10;
                    double x2 = x + 5 + displayPos2 * positionWidth - 10;
                    double noteY = y + (currentNote.String - 1) * StringSpacing;

                    // 绘制弧形连接线
                    DrawConnectionArc(dc, x1, x2, noteY);
                }
            }
        }

        // 绘制音符
        foreach (var note in measure.Notes)
        {
            int displayPos = note.Position / 8;
            double noteX = x + 5 + displayPos * positionWidth;
            double noteY = y + (note.String - 1) * StringSpacing;

            DrawNote(dc, note, noteX, noteY, measureIndex, note.Position);
        }

        // 绘制节奏符号（在六线谱下方）
        DrawRhythmNotation(dc, measure, x, y, width, positionWidth);

        // 绘制光标
        if (measureIndex == CurrentMeasure)
        {
            int cursorDisplayPos = CurrentPosition / 8;
            double cursorX = x + 5 + cursorDisplayPos * positionWidth;
            double cursorY = y + (CurrentString - 1) * StringSpacing;

            dc.DrawRectangle(null, _cursorPen,
                new Rect(cursorX - 8, cursorY - 8, 16, 16));
        }
    }

    private void DrawRhythmNotation(DrawingContext dc, Measure measure, double x, double y, double width, double positionWidth)
    {
        // 节奏符号绘制在第6弦下方
        double rhythmY = y + 5 * StringSpacing + 12;
        double stemHeight = 14;
        double beamSpacing = 5;

        // 按位置分组音符，收集每个位置的时值
        var notesByPosition = measure.Notes
            .GroupBy(n => n.Position)
            .OrderBy(g => g.Key)
            .Select(g => new { Position = g.Key, Note = g.First() })
            .ToList();

        // 分组连续的可连杠音符
        var beamGroups = new List<List<int>>(); // 存储索引
        var currentGroup = new List<int>();

        for (int i = 0; i < notesByPosition.Count; i++)
        {
            var item = notesByPosition[i];
            int beamCount = GetBeamCount(item.Note.Duration);

            if (beamCount > 0) // 八分音符及更短
            {
                if (currentGroup.Count == 0)
                {
                    currentGroup.Add(i);
                }
                else
                {
                    // 检查是否可以与前一个连杠（位置间距不超过1个八分音符位置）
                    int prevPos = notesByPosition[currentGroup.Last()].Position;
                    int posDiff = (item.Position - prevPos) / 8;
                    if (posDiff <= 1)
                    {
                        currentGroup.Add(i);
                    }
                    else
                    {
                        beamGroups.Add(currentGroup);
                        currentGroup = new List<int> { i };
                    }
                }
            }
            else
            {
                if (currentGroup.Count > 0)
                {
                    beamGroups.Add(currentGroup);
                    currentGroup = new List<int>();
                }
            }
        }
        if (currentGroup.Count > 0)
        {
            beamGroups.Add(currentGroup);
        }

        // 绘制所有音符的符干
        for (int i = 0; i < notesByPosition.Count; i++)
        {
            var item = notesByPosition[i];
            int displayPos = item.Position / 8;
            double noteX = x + 5 + displayPos * positionWidth;
            int beamCount = GetBeamCount(item.Note.Duration);

            if (beamCount >= 0) // 四分及更短都画符干
            {
                dc.DrawLine(_durationStemPen,
                    new Point(noteX, rhythmY),
                    new Point(noteX, rhythmY + stemHeight));
            }
        }

        // 绘制连杠组
        foreach (var group in beamGroups)
        {
            if (group.Count >= 2)
            {
                // 多个音符连杠
                int firstIdx = group.First();
                int lastIdx = group.Last();
                double firstX = x + 5 + notesByPosition[firstIdx].Position / 8 * positionWidth;
                double lastX = x + 5 + notesByPosition[lastIdx].Position / 8 * positionWidth;

                // 找出组内最少的连杠数（决定公共连杠数量）
                int minBeams = group.Min(idx => GetBeamCount(notesByPosition[idx].Note.Duration));

                // 绘制公共连杠
                for (int b = 0; b < minBeams; b++)
                {
                    double beamY = rhythmY + stemHeight - 1 - b * beamSpacing;
                    dc.DrawLine(_durationBeamPen,
                        new Point(firstX, beamY),
                        new Point(lastX, beamY));
                }

                // 绘制额外的局部连杠（如十六分音符的第二条杠）
                foreach (int idx in group)
                {
                    int beamCount = GetBeamCount(notesByPosition[idx].Note.Duration);
                    double noteX = x + 5 + notesByPosition[idx].Position / 8 * positionWidth;

                    for (int b = minBeams; b < beamCount; b++)
                    {
                        double beamY = rhythmY + stemHeight - 1 - b * beamSpacing;
                        // 短横杠向右延伸
                        dc.DrawLine(_durationBeamPen,
                            new Point(noteX, beamY),
                            new Point(noteX + 6, beamY));
                    }
                }
            }
            else if (group.Count == 1)
            {
                // 单独的音符画小旗
                int idx = group[0];
                var item = notesByPosition[idx];
                double noteX = x + 5 + item.Position / 8 * positionWidth;
                int beamCount = GetBeamCount(item.Note.Duration);

                for (int b = 0; b < beamCount; b++)
                {
                    double beamY = rhythmY + stemHeight - 1 - b * beamSpacing;
                    // 斜向下的小旗
                    dc.DrawLine(_durationBeamPen,
                        new Point(noteX, beamY),
                        new Point(noteX + 7, beamY + 3));
                }
            }
        }

        // 绘制四分音符的点（区分于无符号）
        for (int i = 0; i < notesByPosition.Count; i++)
        {
            var item = notesByPosition[i];
            if (item.Note.Duration == NoteDuration.Quarter)
            {
                int displayPos = item.Position / 8;
                double noteX = x + 5 + displayPos * positionWidth;
                // 在符干底部画一个小圆点
                dc.DrawEllipse(Brushes.Black, null, 
                    new Point(noteX, rhythmY + stemHeight + 3), 2, 2);
            }
        }
    }

    private int GetBeamCount(NoteDuration duration)
    {
        return duration switch
        {
            NoteDuration.Whole => -1,      // 不画
            NoteDuration.Half => -1,       // 不画
            NoteDuration.Quarter => 0,     // 只画竖线
            NoteDuration.Eighth => 1,      // 1条横杠
            NoteDuration.Sixteenth => 2,   // 2条横杠
            NoteDuration.ThirtySecond => 3, // 3条横杠
            _ => 0
        };
    }

    private void DrawConnectionArc(DrawingContext dc, double x1, double x2, double y)
    {
        // 绘制优美的弧形连接线
        double midX = (x1 + x2) / 2;
        double arcHeight = 12; // 弧线高度

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = new Point(x1, y - 5) };
        
        // 使用贝塞尔曲线绘制平滑的弧线
        var bezierSegment = new BezierSegment(
            new Point(x1 + (x2 - x1) * 0.25, y - arcHeight),
            new Point(x1 + (x2 - x1) * 0.75, y - arcHeight),
            new Point(x2, y - 5),
            true);
        
        pathFigure.Segments.Add(bezierSegment);
        pathGeometry.Figures.Add(pathFigure);

        dc.DrawGeometry(null, _connectionPen, pathGeometry);
    }

    private void DrawNote(DrawingContext dc, Note note, double x, double y, int measureIndex, int position)
    {
        // 处理休止符
        if (note.IsRest)
        {
            DrawRestSymbol(dc, x, y, note.Duration);
            return;
        }

        string fretText = note.Fret.ToString();
        bool hasTechnique = note.Technique != Technique.None;

        // 获取技巧显示文本
        string techniqueText = GetTechniqueDisplayText(note.Technique);

        // 确定显示文本
        string displayText;
        if (note.Technique.HasFlag(Technique.Mute))
        {
            displayText = "X";
        }
        else
        {
            displayText = fretText;
        }

        // 计算文本尺寸
        var textSize = MeasureText(displayText, 12);
        double bgWidth = Math.Max(textSize.Width + 6, 18);
        double bgHeight = 16;

        // 绘制背景 - 技巧音符使用不同背景
        var bgBrush = hasTechnique ? _techniqueBgColor : _noteBackground;
        var bgRect = new Rect(x - bgWidth / 2, y - bgHeight / 2, bgWidth, bgHeight);
        
        // 技巧音符绘制圆角边框
        if (hasTechnique)
        {
            var borderPen = new Pen(_techniqueColor, 1.5);
            dc.DrawRoundedRectangle(bgBrush, borderPen, bgRect, 3, 3);
        }
        else
        {
            dc.DrawRoundedRectangle(bgBrush, null, bgRect, 2, 2);
        }

        // 绘制品数文本
        var fretTextFormatted = new FormattedText(
            displayText,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            hasTechnique ? _techniqueBoldTypeface : _typeface,
            12,
            hasTechnique ? _techniqueColor : _noteForeground,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(fretTextFormatted, new Point(x - textSize.Width / 2, y - 7));

        // 绘制技巧标记（在音符上方或下方）
        if (!string.IsNullOrEmpty(techniqueText))
        {
            DrawTechniqueLabel(dc, x, y, techniqueText);
        }

        // 绘制音名
        if (ShowNoteNames && Song != null && !note.IsRest)
        {
            string noteName = Song.Tuning.GetNoteName(note.String, note.Fret);
            var noteNameText = new FormattedText(
                noteName,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                9,
                _noteNameColor,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(noteNameText, new Point(x - 8, y + 10));
        }
    }

    private string GetTechniqueDisplayText(Technique technique)
    {
        if (technique == Technique.None) return "";
        
        var parts = new List<string>();
        
        if (technique.HasFlag(Technique.HammerOn)) parts.Add("H");
        if (technique.HasFlag(Technique.PullOff)) parts.Add("P");
        if (technique.HasFlag(Technique.SlideUp)) parts.Add("S↑");
        if (technique.HasFlag(Technique.SlideDown)) parts.Add("S↓");
        if (technique.HasFlag(Technique.Bend)) parts.Add("B");
        if (technique.HasFlag(Technique.Release)) parts.Add("R");
        if (technique.HasFlag(Technique.Vibrato)) parts.Add("~");
        if (technique.HasFlag(Technique.Harmonic)) parts.Add("◇");
        if (technique.HasFlag(Technique.PinchHarmonic)) parts.Add("◆");
        if (technique.HasFlag(Technique.PalmMute)) parts.Add("PM");
        if (technique.HasFlag(Technique.Tap)) parts.Add("T");
        if (technique.HasFlag(Technique.Trill)) parts.Add("tr");
        if (technique.HasFlag(Technique.LetRing)) parts.Add("LR");
        // Mute 已在显示文本中处理为X，不需要额外标记
        
        return string.Join("", parts);
    }

    private void DrawTechniqueLabel(DrawingContext dc, double x, double y, string text)
    {
        var labelText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _techniqueBoldTypeface,
            10,
            _techniqueColor,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        double labelX = x - labelText.Width / 2;
        double labelY = y - 20; // 在音符上方

        // 绘制标签背景
        var labelBg = new Rect(labelX - 2, labelY - 1, labelText.Width + 4, labelText.Height + 2);
        dc.DrawRoundedRectangle(Brushes.White, null, labelBg, 2, 2);

        dc.DrawText(labelText, new Point(labelX, labelY));
    }

    private void DrawRestSymbol(DrawingContext dc, double x, double y, NoteDuration duration)
    {
        // 绘制休止符 - 使用更专业的外观
        string restSymbol = duration switch
        {
            NoteDuration.Whole => "𝄻",      // 全休止符
            NoteDuration.Half => "𝄼",       // 二分休止符
            NoteDuration.Quarter => "𝄽",    // 四分休止符
            NoteDuration.Eighth => "𝄾",     // 八分休止符
            NoteDuration.Sixteenth => "𝄿",  // 十六分休止符
            _ => "-"
        };

        // 如果系统不支持音乐符号，使用简单的表示
        var testText = new FormattedText(
            restSymbol,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            14,
            _restColor,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        // 检测是否能正确显示音乐符号
        if (testText.Width < 3 || restSymbol == "-")
        {
            // 使用备用显示方式
            restSymbol = "—";
        }

        var text = new FormattedText(
            restSymbol,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            14,
            _restColor,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        // 绘制背景
        double bgWidth = Math.Max(text.Width + 6, 16);
        var bgRect = new Rect(x - bgWidth / 2, y - 8, bgWidth, 16);
        dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(245, 245, 245)), 
            new Pen(_restColor, 0.5), bgRect, 2, 2);

        dc.DrawText(text, new Point(x - text.Width / 2, y - 8));
    }

    private Size MeasureText(string text, double fontSize)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            fontSize,
            Brushes.Black,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        return new Size(formattedText.Width, formattedText.Height);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();

        var pos = e.GetPosition(this);
        var (measureIndex, position, stringNum) = HitTest(pos);

        if (measureIndex >= 0)
        {
            CurrentMeasure = measureIndex;
            CurrentPosition = position;
            CurrentString = stringNum;

            RaiseEvent(new NoteClickedEventArgs(NoteClickedEvent, this)
            {
                MeasureIndex = measureIndex,
                Position = position,
                StringNumber = stringNum
            });
        }
    }

    private (int MeasureIndex, int Position, int StringNumber) HitTest(Point point)
    {
        if (Song == null || Song.Measures.Count == 0)
            return (-1, 0, 1);

        // 计算点击位置对应的行
        int line = (int)((point.Y - LineMargin + LineHeight / 2) / LineHeight);
        int measureStartIndex = line * MeasuresPerLine;

        if (measureStartIndex >= Song.Measures.Count)
            return (-1, 0, 1);

        // 计算本行的小节数
        int measuresThisLine = Math.Min(MeasuresPerLine, Song.Measures.Count - measureStartIndex);
        double measureWidth = (ActualWidth - StringLabelWidth - MeasureMargin * 2) / measuresThisLine;

        // 计算点击的小节
        double relativeX = point.X - StringLabelWidth - MeasureMargin;
        int measureInLine = (int)(relativeX / measureWidth);
        measureInLine = Math.Clamp(measureInLine, 0, measuresThisLine - 1);

        int measureIndex = measureStartIndex + measureInLine;

        // 计算点击的位置
        var measure = Song.Measures[measureIndex];
        int positions = measure.TotalTicks / 8;
        double positionWidth = (measureWidth - 10) / Math.Max(positions, 1);
        double measureStartX = measureInLine * measureWidth;
        int position = (int)((relativeX - measureStartX - 5) / positionWidth) * 8;
        position = Math.Clamp(position, 0, measure.TotalTicks - 8);

        // 计算点击的弦
        double lineY = LineMargin + line * LineHeight;
        int stringNum = (int)Math.Round((point.Y - lineY) / StringSpacing) + 1;
        stringNum = Math.Clamp(stringNum, 1, 6);

        return (measureIndex, position, stringNum);
    }
}

/// <summary>
/// 音符点击事件参数
/// </summary>
public class NoteClickedEventArgs : RoutedEventArgs
{
    public int MeasureIndex { get; set; }
    public int Position { get; set; }
    public int StringNumber { get; set; }

    public NoteClickedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }
}
