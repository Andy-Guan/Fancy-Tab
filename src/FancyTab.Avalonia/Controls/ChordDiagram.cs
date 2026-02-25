using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FancyTab.Core.Models;

namespace FancyTab.Avalonia.Controls;

/// <summary>
/// 和弦图控件
/// </summary>
public class ChordDiagram : Control
{
    #region 样式属性

    public static readonly StyledProperty<Chord?> ChordProperty =
        AvaloniaProperty.Register<ChordDiagram, Chord?>(nameof(Chord));

    public static readonly StyledProperty<int> FretCountProperty =
        AvaloniaProperty.Register<ChordDiagram, int>(nameof(FretCount), 4);

    public Chord? Chord
    {
        get => GetValue(ChordProperty);
        set => SetValue(ChordProperty, value);
    }

    public int FretCount
    {
        get => GetValue(FretCountProperty);
        set => SetValue(FretCountProperty, value);
    }

    #endregion

    private readonly Typeface _typeface = new("Arial");
    private readonly IPen _framePen;
    private readonly IPen _stringPen;
    private readonly IPen _fretPen;
    private readonly IPen _barrePen;
    private readonly IPen _openStringPen;

    static ChordDiagram()
    {
        AffectsRender<ChordDiagram>(ChordProperty, FretCountProperty);
    }

    public ChordDiagram()
    {
        MinWidth = 80;
        MinHeight = 100;

        // 创建不可变画笔
        _framePen = new Pen(Brushes.Black, 1.5).ToImmutable();
        _stringPen = new Pen(Brushes.Black, 1).ToImmutable();
        _fretPen = new Pen(Brushes.Black, 1).ToImmutable();
        _barrePen = new Pen(Brushes.Black, 8).ToImmutable();
        _openStringPen = new Pen(Brushes.Black, 1.5).ToImmutable();
    }

    public override void Render(DrawingContext dc)
    {
        base.Render(dc);

        if (Chord == null) return;

        double width = Bounds.Width;
        double height = Bounds.Height;

        // 布局参数
        double titleHeight = 20;
        double topMargin = 15;
        double margin = 10;
        double diagramWidth = width - margin * 2;
        double diagramHeight = height - titleHeight - topMargin - margin;
        double stringSpacing = diagramWidth / 5;
        double fretSpacing = diagramHeight / FretCount;

        // 绘制和弦名称
        var nameText = new FormattedText(
            Chord.Name,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(_typeface.FontFamily, FontStyle.Normal, FontWeight.Bold),
            14,
            Brushes.Black);

        dc.DrawText(nameText, new Point((width - nameText.Width) / 2, 2));

        double startX = margin;
        double startY = titleHeight + topMargin;

        // 绘制横线（品）
        for (int i = 0; i <= FretCount; i++)
        {
            double y = startY + i * fretSpacing;
            var pen = i == 0 ? _framePen : _fretPen;
            dc.DrawLine(pen, new Point(startX, y), new Point(startX + diagramWidth, y));
        }

        // 绘制竖线（弦）
        for (int i = 0; i < 6; i++)
        {
            double x = startX + i * stringSpacing;
            dc.DrawLine(_stringPen, new Point(x, startY), new Point(x, startY + diagramHeight));
        }

        // 绘制起始品位
        if (Chord.BaseFret > 1)
        {
            var fretText = new FormattedText(
                Chord.BaseFret.ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                10,
                Brushes.Black);

            dc.DrawText(fretText, new Point(startX + diagramWidth + 3, startY + fretSpacing / 2 - 5));
        }

        // 绘制横按
        if (Chord.HasBarre)
        {
            int barreStart = Chord.BarreStrings.Start;
            int barreEnd = Chord.BarreStrings.End;
            double barreY = startY + fretSpacing / 2;
            double barreStartX = startX + (6 - barreEnd) * stringSpacing;
            double barreEndX = startX + (6 - barreStart) * stringSpacing;

            dc.DrawLine(_barrePen, 
                new Point(barreStartX, barreY), 
                new Point(barreEndX, barreY));
        }

        // 绘制按法
        for (int i = 0; i < 6; i++)
        {
            int fret = Chord.Fingering[i];
            double x = startX + (5 - i) * stringSpacing;

            if (fret == -1)
            {
                // X 表示不弹
                var xText = new FormattedText(
                    "x",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    _typeface,
                    12,
                    Brushes.Black);

                dc.DrawText(xText, new Point(x - xText.Width / 2, startY - 14));
            }
            else if (fret == 0)
            {
                // O 表示空弦
                dc.DrawEllipse(null, _openStringPen, 
                    new Point(x, startY - 8), 4, 4);
            }
            else if (!Chord.HasBarre || fret != Chord.BaseFret)
            {
                // 绘制实心圆表示按弦位置
                double y = startY + (fret - Chord.BaseFret + 0.5) * fretSpacing;
                dc.DrawEllipse(Brushes.Black, null, new Point(x, y), 6, 6);
            }
        }
    }
}
