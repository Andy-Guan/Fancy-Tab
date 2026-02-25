using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FancyTab.Core.Models;

namespace FancyTab.Avalonia.Controls;

/// <summary>
/// 六线谱画布控件
/// </summary>
public class TabCanvas : Control
{
    #region Styled Properties

    public static readonly StyledProperty<Song?> SongProperty =
        AvaloniaProperty.Register<TabCanvas, Song?>(nameof(Song));

    public static readonly StyledProperty<int> CurrentMeasureProperty =
        AvaloniaProperty.Register<TabCanvas, int>(nameof(CurrentMeasure));

    public static readonly StyledProperty<int> CurrentPositionProperty =
        AvaloniaProperty.Register<TabCanvas, int>(nameof(CurrentPosition));

    public static readonly StyledProperty<int> CurrentStringProperty =
        AvaloniaProperty.Register<TabCanvas, int>(nameof(CurrentString), 1);

    public static readonly StyledProperty<bool> ShowNoteNamesProperty =
        AvaloniaProperty.Register<TabCanvas, bool>(nameof(ShowNoteNames));

    public static readonly StyledProperty<int> MeasuresPerLineProperty =
        AvaloniaProperty.Register<TabCanvas, int>(nameof(MeasuresPerLine), 4);

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<TabCanvas, IBrush?>(nameof(Background), Brushes.White);

    public Song? Song
    {
        get => GetValue(SongProperty);
        set => SetValue(SongProperty, value);
    }

    public int CurrentMeasure
    {
        get => GetValue(CurrentMeasureProperty);
        set => SetValue(CurrentMeasureProperty, value);
    }

    public int CurrentPosition
    {
        get => GetValue(CurrentPositionProperty);
        set => SetValue(CurrentPositionProperty, value);
    }

    public int CurrentString
    {
        get => GetValue(CurrentStringProperty);
        set => SetValue(CurrentStringProperty, value);
    }

    public bool ShowNoteNames
    {
        get => GetValue(ShowNoteNamesProperty);
        set => SetValue(ShowNoteNamesProperty, value);
    }

    public int MeasuresPerLine
    {
        get => GetValue(MeasuresPerLineProperty);
        set => SetValue(MeasuresPerLineProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    #endregion

    #region Events

    public static readonly RoutedEvent<NoteClickedEventArgs> NoteClickedEvent =
        RoutedEvent.Register<TabCanvas, NoteClickedEventArgs>(nameof(NoteClicked), RoutingStrategies.Bubble);

    public event EventHandler<NoteClickedEventArgs>? NoteClicked
    {
        add => AddHandler(NoteClickedEvent, value);
        remove => RemoveHandler(NoteClickedEvent, value);
    }

    #endregion

    #region Drawing Parameters

    private const double StringSpacing = 20;
    private const double MeasureMargin = 20;
    private const double LineMargin = 40;
    private const double StringLabelWidth = 30;
    private const double LineHeight = StringSpacing * 7 + LineMargin + 30;

    private readonly Typeface _typeface = new("Consolas, Courier New, monospace");
    private readonly Typeface _boldTypeface = new("Consolas, Courier New, monospace", FontStyle.Normal, FontWeight.Bold);
    private readonly IPen _stringPen = new Pen(Brushes.Gray, 1);
    private readonly IPen _barLinePen = new Pen(Brushes.DarkGray, 2);
    private readonly IPen _cursorPen = new Pen(Brushes.DodgerBlue, 2);
    private readonly IPen _connectionPen = new Pen(new SolidColorBrush(Color.FromRgb(220, 80, 60)), 2);
    private readonly IBrush _noteBackground = new SolidColorBrush(Color.FromRgb(250, 250, 250));
    private readonly IBrush _noteForeground = Brushes.Black;
    private readonly IBrush _techniqueColor = new SolidColorBrush(Color.FromRgb(200, 60, 40));
    private readonly IBrush _techniqueBgColor = new SolidColorBrush(Color.FromRgb(255, 240, 235));
    private readonly IBrush _restColor = new SolidColorBrush(Color.FromRgb(100, 100, 100));
    private readonly IPen _durationStemPen = new Pen(Brushes.Black, 1.5);
    private readonly IPen _durationBeamPen = new Pen(Brushes.Black, 2);
    private readonly IBrush _noteNameColor = new SolidColorBrush(Color.FromRgb(100, 149, 237));

    #endregion

    static TabCanvas()
    {
        AffectsRender<TabCanvas>(SongProperty, CurrentMeasureProperty, CurrentPositionProperty,
            CurrentStringProperty, ShowNoteNamesProperty, MeasuresPerLineProperty, BackgroundProperty);
    }

    public TabCanvas()
    {
        Focusable = true;
        ClipToBounds = true;
    }

    public override void Render(DrawingContext dc)
    {
        base.Render(dc);

        dc.DrawRectangle(Background, null, new Rect(0, 0, Bounds.Width, Bounds.Height));

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
            "Press number keys to input fret...",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            14,
            Brushes.Gray);

        dc.DrawText(text, new Point(50, 50));
    }

    private void DrawTablature(DrawingContext dc)
    {
        double y = LineMargin;
        int measureIndex = 0;

        while (measureIndex < Song!.Measures.Count)
        {
            int measuresThisLine = Math.Min(MeasuresPerLine, Song.Measures.Count - measureIndex);
            double measureWidth = (Bounds.Width - StringLabelWidth - MeasureMargin * 2) / measuresThisLine;

            DrawStringLabels(dc, y);
            DrawStrings(dc, y, StringLabelWidth, Bounds.Width - MeasureMargin);

            double x = StringLabelWidth + MeasureMargin;
            for (int i = 0; i < measuresThisLine; i++)
            {
                var measure = Song.Measures[measureIndex + i];
                DrawMeasure(dc, measure, measureIndex + i, x, y, measureWidth);
                x += measureWidth;
            }

            measureIndex += measuresThisLine;
            y += LineHeight;

            if (y > Bounds.Height) break;
        }
    }

    private void DrawStringLabels(DrawingContext dc, double y)
    {
        string[] labels = { "e", "B", "G", "D", "A", "E" };

        for (int i = 0; i < 6; i++)
        {
            var text = new FormattedText(
                labels[i],
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                12,
                Brushes.DarkGray);

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
        dc.DrawLine(_barLinePen, new Point(x, y), new Point(x, y + 5 * StringSpacing));
        dc.DrawLine(_barLinePen, new Point(x + width, y), new Point(x + width, y + 5 * StringSpacing));

        var measureNumText = new FormattedText(
            measure.Number.ToString(),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            10,
            Brushes.Gray);
        dc.DrawText(measureNumText, new Point(x + 2, y - 15));

        if (!string.IsNullOrEmpty(measure.ChordName))
        {
            var chordText = new FormattedText(
                measure.ChordName,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _boldTypeface,
                14,
                Brushes.DarkBlue);
            dc.DrawText(chordText, new Point(x + 5, y - 35));
        }

        int positions = measure.TotalTicks / 8;
        double positionWidth = (width - 10) / Math.Max(positions, 1);

        // Draw connection arcs
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

                    DrawConnectionArc(dc, x1, x2, noteY);
                }
            }
        }

        // Draw notes
        foreach (var note in measure.Notes)
        {
            int displayPos = note.Position / 8;
            double noteX = x + 5 + displayPos * positionWidth;
            double noteY = y + (note.String - 1) * StringSpacing;

            DrawNote(dc, note, noteX, noteY);
        }

        // Draw rhythm notation
        DrawRhythmNotation(dc, measure, x, y, width, positionWidth);

        // Draw cursor
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
        double rhythmY = y + 5 * StringSpacing + 12;
        double stemHeight = 14;
        double beamSpacing = 5;

        var notesByPosition = measure.Notes
            .GroupBy(n => n.Position)
            .OrderBy(g => g.Key)
            .Select(g => new { Position = g.Key, Note = g.First() })
            .ToList();

        var beamGroups = new List<List<int>>();
        var currentGroup = new List<int>();

        for (int i = 0; i < notesByPosition.Count; i++)
        {
            var item = notesByPosition[i];
            int beamCount = GetBeamCount(item.Note.Duration);

            if (beamCount > 0)
            {
                if (currentGroup.Count == 0)
                {
                    currentGroup.Add(i);
                }
                else
                {
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
        if (currentGroup.Count > 0) beamGroups.Add(currentGroup);

        // Draw stems
        for (int i = 0; i < notesByPosition.Count; i++)
        {
            var item = notesByPosition[i];
            int displayPos = item.Position / 8;
            double noteX = x + 5 + displayPos * positionWidth;
            int beamCount = GetBeamCount(item.Note.Duration);

            if (beamCount >= 0)
            {
                dc.DrawLine(_durationStemPen,
                    new Point(noteX, rhythmY),
                    new Point(noteX, rhythmY + stemHeight));
            }
        }

        // Draw beams
        foreach (var group in beamGroups)
        {
            if (group.Count >= 2)
            {
                int firstIdx = group.First();
                int lastIdx = group.Last();
                double firstX = x + 5 + notesByPosition[firstIdx].Position / 8 * positionWidth;
                double lastX = x + 5 + notesByPosition[lastIdx].Position / 8 * positionWidth;

                int minBeams = group.Min(idx => GetBeamCount(notesByPosition[idx].Note.Duration));

                for (int b = 0; b < minBeams; b++)
                {
                    double beamY = rhythmY + stemHeight - 1 - b * beamSpacing;
                    dc.DrawLine(_durationBeamPen, new Point(firstX, beamY), new Point(lastX, beamY));
                }

                foreach (int idx in group)
                {
                    int beamCount = GetBeamCount(notesByPosition[idx].Note.Duration);
                    double noteX = x + 5 + notesByPosition[idx].Position / 8 * positionWidth;

                    for (int b = minBeams; b < beamCount; b++)
                    {
                        double beamY = rhythmY + stemHeight - 1 - b * beamSpacing;
                        dc.DrawLine(_durationBeamPen, new Point(noteX, beamY), new Point(noteX + 6, beamY));
                    }
                }
            }
            else if (group.Count == 1)
            {
                int idx = group[0];
                var item = notesByPosition[idx];
                double noteX = x + 5 + item.Position / 8 * positionWidth;
                int beamCount = GetBeamCount(item.Note.Duration);

                for (int b = 0; b < beamCount; b++)
                {
                    double beamY = rhythmY + stemHeight - 1 - b * beamSpacing;
                    dc.DrawLine(_durationBeamPen, new Point(noteX, beamY), new Point(noteX + 7, beamY + 3));
                }
            }
        }

        // Quarter note dots
        for (int i = 0; i < notesByPosition.Count; i++)
        {
            var item = notesByPosition[i];
            if (item.Note.Duration == NoteDuration.Quarter)
            {
                int displayPos = item.Position / 8;
                double noteX = x + 5 + displayPos * positionWidth;
                dc.DrawEllipse(Brushes.Black, null,
                    new Rect(noteX - 2, rhythmY + stemHeight + 1, 4, 4));
            }
        }
    }

    private static int GetBeamCount(NoteDuration duration)
    {
        return duration switch
        {
            NoteDuration.Whole => -1,
            NoteDuration.Half => -1,
            NoteDuration.Quarter => 0,
            NoteDuration.Eighth => 1,
            NoteDuration.Sixteenth => 2,
            NoteDuration.ThirtySecond => 3,
            _ => 0
        };
    }

    private void DrawConnectionArc(DrawingContext dc, double x1, double x2, double y)
    {
        double arcHeight = 12;

        var geometry = new PathGeometry { Figures = new PathFigures() };
        var figure = new PathFigure { StartPoint = new Point(x1, y - 5), IsClosed = false, Segments = new PathSegments() };

        figure.Segments.Add(new BezierSegment
        {
            Point1 = new Point(x1 + (x2 - x1) * 0.25, y - arcHeight),
            Point2 = new Point(x1 + (x2 - x1) * 0.75, y - arcHeight),
            Point3 = new Point(x2, y - 5)
        });

        geometry.Figures.Add(figure);
        dc.DrawGeometry(null, _connectionPen, geometry);
    }

    private void DrawNote(DrawingContext dc, Note note, double x, double y)
    {
        if (note.IsRest)
        {
            DrawRestSymbol(dc, x, y, note.Duration);
            return;
        }

        string displayText = note.Technique.HasFlag(Technique.Mute) ? "X" : note.Fret.ToString();
        bool hasTechnique = note.Technique != Technique.None;
        string techniqueText = GetTechniqueDisplayText(note.Technique);

        var textSize = MeasureText(displayText, 12);
        double bgWidth = Math.Max(textSize.Width + 6, 18);
        double bgHeight = 16;

        var bgBrush = hasTechnique ? _techniqueBgColor : _noteBackground;
        var bgRect = new Rect(x - bgWidth / 2, y - bgHeight / 2, bgWidth, bgHeight);

        if (hasTechnique)
        {
            var borderPen = new Pen(_techniqueColor, 1.5);
            dc.DrawRectangle(bgBrush, borderPen, bgRect, 3, 3);
        }
        else
        {
            dc.DrawRectangle(bgBrush, null, bgRect, 2, 2);
        }

        var fretText = new FormattedText(
            displayText,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            hasTechnique ? _boldTypeface : _typeface,
            12,
            hasTechnique ? _techniqueColor : _noteForeground);

        dc.DrawText(fretText, new Point(x - textSize.Width / 2, y - 7));

        if (!string.IsNullOrEmpty(techniqueText))
        {
            DrawTechniqueLabel(dc, x, y, techniqueText);
        }

        if (ShowNoteNames && Song != null && !note.IsRest)
        {
            string noteName = Song.Tuning.GetNoteName(note.String, note.Fret);
            var noteNameText = new FormattedText(
                noteName,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                9,
                _noteNameColor);

            dc.DrawText(noteNameText, new Point(x - 8, y + 10));
        }
    }

    private static string GetTechniqueDisplayText(Technique technique)
    {
        if (technique == Technique.None) return "";

        var parts = new List<string>();

        if (technique.HasFlag(Technique.HammerOn)) parts.Add("H");
        if (technique.HasFlag(Technique.PullOff)) parts.Add("P");
        if (technique.HasFlag(Technique.SlideUp)) parts.Add("S");
        if (technique.HasFlag(Technique.SlideDown)) parts.Add("S");
        if (technique.HasFlag(Technique.Bend)) parts.Add("B");
        if (technique.HasFlag(Technique.Vibrato)) parts.Add("~");
        if (technique.HasFlag(Technique.Harmonic)) parts.Add("<>");
        if (technique.HasFlag(Technique.PalmMute)) parts.Add("PM");
        if (technique.HasFlag(Technique.Tap)) parts.Add("T");
        if (technique.HasFlag(Technique.Trill)) parts.Add("tr");

        return string.Join("", parts);
    }

    private void DrawTechniqueLabel(DrawingContext dc, double x, double y, string text)
    {
        var labelText = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _boldTypeface,
            10,
            _techniqueColor);

        double labelX = x - labelText.Width / 2;
        double labelY = y - 20;

        var labelBg = new Rect(labelX - 2, labelY - 1, labelText.Width + 4, labelText.Height + 2);
        dc.DrawRectangle(Brushes.White, null, labelBg, 2, 2);

        dc.DrawText(labelText, new Point(labelX, labelY));
    }

    private void DrawRestSymbol(DrawingContext dc, double x, double y, NoteDuration duration)
    {
        string restSymbol = "-";

        var text = new FormattedText(
            restSymbol,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            14,
            _restColor);

        double bgWidth = Math.Max(text.Width + 6, 16);
        var bgRect = new Rect(x - bgWidth / 2, y - 8, bgWidth, 16);
        dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(245, 245, 245)),
            new Pen(_restColor, 0.5), bgRect, 2, 2);

        dc.DrawText(text, new Point(x - text.Width / 2, y - 8));
    }

    private Size MeasureText(string text, double fontSize)
    {
        var formattedText = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            fontSize,
            Brushes.Black);

        return new Size(formattedText.Width, formattedText.Height);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();

        var pos = e.GetPosition(this);
        var (measureIndex, position, stringNum) = HitTest(pos);

        if (measureIndex >= 0)
        {
            CurrentMeasure = measureIndex;
            CurrentPosition = position;
            CurrentString = stringNum;

            RaiseEvent(new NoteClickedEventArgs
            {
                RoutedEvent = NoteClickedEvent,
                Source = this,
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

        int line = (int)((point.Y - LineMargin + LineHeight / 2) / LineHeight);
        int measureStartIndex = line * MeasuresPerLine;

        if (measureStartIndex >= Song.Measures.Count)
            return (-1, 0, 1);

        int measuresThisLine = Math.Min(MeasuresPerLine, Song.Measures.Count - measureStartIndex);
        double measureWidth = (Bounds.Width - StringLabelWidth - MeasureMargin * 2) / measuresThisLine;

        double relativeX = point.X - StringLabelWidth - MeasureMargin;
        int measureInLine = (int)(relativeX / measureWidth);
        measureInLine = Math.Clamp(measureInLine, 0, measuresThisLine - 1);

        int measureIndex = measureStartIndex + measureInLine;

        var measure = Song.Measures[measureIndex];
        int positions = measure.TotalTicks / 8;
        double positionWidth = (measureWidth - 10) / Math.Max(positions, 1);
        double measureStartX = measureInLine * measureWidth;
        int position = (int)((relativeX - measureStartX - 5) / positionWidth) * 8;
        position = Math.Clamp(position, 0, measure.TotalTicks - 8);

        double lineY = LineMargin + line * LineHeight;
        int stringNum = (int)Math.Round((point.Y - lineY) / StringSpacing) + 1;
        stringNum = Math.Clamp(stringNum, 1, 6);

        return (measureIndex, position, stringNum);
    }
}

/// <summary>
/// Note clicked event args
/// </summary>
public class NoteClickedEventArgs : RoutedEventArgs
{
    public int MeasureIndex { get; set; }
    public int Position { get; set; }
    public int StringNumber { get; set; }
}
