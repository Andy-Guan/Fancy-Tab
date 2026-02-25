using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using FancyTab.Core.Models;

namespace FancyTab.Core.Services;

/// <summary>
/// 跨平台字体解析器
/// </summary>
public class CrossPlatformFontResolver : IFontResolver
{
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        var style = (isBold ? "Bold" : "") + (isItalic ? "Italic" : "");
        if (string.IsNullOrEmpty(style)) style = "Regular";
        return new FontResolverInfo($"{familyName}#{style}");
    }

    public byte[]? GetFont(string faceName)
    {
        var parts = faceName.Split('#');
        var familyName = parts[0];
        var style = parts.Length > 1 ? parts[1] : "Regular";

        // 尝试从多个位置加载字体
        var fontPath = GetFontPath(familyName, style);
        if (fontPath != null && File.Exists(fontPath))
        {
            return File.ReadAllBytes(fontPath);
        }
        
        return null;
    }

    private static string? GetFontPath(string familyName, string style)
    {
        // Windows 字体路径
        var windowsFontsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
        
        // Linux 字体路径
        var linuxFontPaths = new[]
        {
            "/usr/share/fonts",
            "/usr/local/share/fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts")
        };
        
        // macOS 字体路径
        var macFontPaths = new[]
        {
            "/System/Library/Fonts",
            "/Library/Fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts")
        };

        var fontFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Arial#Regular"] = "arial.ttf",
            ["Arial#Bold"] = "arialbd.ttf",
            ["Arial#Italic"] = "ariali.ttf",
            ["Arial#BoldItalic"] = "arialbi.ttf",
            ["Consolas#Regular"] = "consola.ttf",
            ["Consolas#Bold"] = "consolab.ttf",
            ["Courier New#Regular"] = "cour.ttf",
            ["Courier New#Bold"] = "courbd.ttf",
        };

        var key = $"{familyName}#{style}";
        if (!fontFiles.TryGetValue(key, out var fileName))
        {
            key = $"{familyName}#Regular";
            if (!fontFiles.TryGetValue(key, out fileName))
            {
                fileName = "arial.ttf"; // 默认字体
            }
        }

        // 尝试 Windows
        if (Directory.Exists(windowsFontsFolder))
        {
            var path = Path.Combine(windowsFontsFolder, fileName);
            if (File.Exists(path)) return path;
        }

        // 尝试 Linux
        foreach (var basePath in linuxFontPaths.Where(Directory.Exists))
        {
            var path = FindFontFile(basePath, fileName);
            if (path != null) return path;
        }

        // 尝试 macOS
        foreach (var basePath in macFontPaths.Where(Directory.Exists))
        {
            var path = FindFontFile(basePath, fileName);
            if (path != null) return path;
        }

        return null;
    }

    private static string? FindFontFile(string basePath, string fileName)
    {
        try
        {
            var files = Directory.GetFiles(basePath, fileName, SearchOption.AllDirectories);
            return files.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// PDF导出服务
/// </summary>
public class PdfExportService : IPdfExportService
{
    private readonly IFileDialogService _dialogService;
    
    // 页面设置
    private const double PageWidth = 595;   // A4宽度 (点)
    private const double PageHeight = 842;  // A4高度 (点)
    private const double Margin = 50;
    private const double ContentWidth = PageWidth - 2 * Margin;

    // 六线谱设置
    private const double StringSpacing = 12;
    private const double LineSpacing = 30;
    private const double RhythmAreaHeight = 25;
    private const double TabLineHeight = StringSpacing * 5 + LineSpacing + RhythmAreaHeight;

    // 字体
    private XFont? _titleFont;
    private XFont? _artistFont;
    private XFont? _headerFont;
    private XFont? _noteFont;
    private XFont? _noteBoldFont;
    private XFont? _techniqueFont;
    private XFont? _chordFont;
    private XFont? _stringLabelFont;

    // 颜色
    private readonly XColor _stringColor = XColors.Gray;
    private readonly XColor _barLineColor = XColors.DarkGray;
    private readonly XColor _techniqueColor = XColor.FromArgb(255, 200, 60, 40);
    private readonly XColor _chordColor = XColors.DarkBlue;
    private readonly XColor _connectionColor = XColor.FromArgb(255, 220, 80, 60);

    private static bool _fontResolverInitialized;

    public PdfExportService(IFileDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    private void EnsureFontsInitialized()
    {
        if (!_fontResolverInitialized)
        {
            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new CrossPlatformFontResolver();
            }
            _fontResolverInitialized = true;
        }

        _titleFont ??= new XFont("Arial", 18, XFontStyleEx.Bold);
        _artistFont ??= new XFont("Arial", 12, XFontStyleEx.Regular);
        _headerFont ??= new XFont("Arial", 10, XFontStyleEx.Regular);
        _noteFont ??= new XFont("Arial", 10, XFontStyleEx.Regular);
        _noteBoldFont ??= new XFont("Arial", 10, XFontStyleEx.Bold);
        _techniqueFont ??= new XFont("Arial", 8, XFontStyleEx.Bold);
        _chordFont ??= new XFont("Arial", 11, XFontStyleEx.Bold);
        _stringLabelFont ??= new XFont("Arial", 9, XFontStyleEx.Regular);
    }

    public async Task<bool> ExportAsync(Song song, string? filePath = null)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = await _dialogService.ShowSaveFileDialogAsync(
                $"{song.Title}.pdf",
                "PDF Files (*.pdf)|*.pdf");

            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
        }

        try
        {
            await Task.Run(() => GeneratePdf(song, filePath));
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PDF export error: {ex.Message}");
            return false;
        }
    }

    private void GeneratePdf(Song song, string filePath)
    {
        EnsureFontsInitialized();

        using var document = new PdfDocument();
        document.Info.Title = song.Title;
        document.Info.Author = song.Artist;

        var page = document.AddPage();
        page.Width = XUnit.FromPoint(PageWidth);
        page.Height = XUnit.FromPoint(PageHeight);

        using var gfx = XGraphics.FromPdfPage(page);

        double y = Margin;

        // 绘制标题
        y = DrawHeader(gfx, song, y);

        // 绘制调弦信息
        y = DrawTuningInfo(gfx, song, y);

        // 绘制和弦图
        if (song.Chords.Count > 0)
        {
            y = DrawChordDiagrams(gfx, song, y);
        }

        // 绘制六线谱
        DrawTablature(gfx, song, y, document);

        document.Save(filePath);
    }

    private double DrawHeader(XGraphics gfx, Song song, double y)
    {
        gfx.DrawString(song.Title, _titleFont!, XBrushes.Black,
            new XRect(Margin, y, ContentWidth, 30), XStringFormats.TopCenter);
        y += 30;

        if (!string.IsNullOrEmpty(song.Artist))
        {
            gfx.DrawString(song.Artist, _artistFont!, XBrushes.DarkGray,
                new XRect(Margin, y, ContentWidth, 20), XStringFormats.TopCenter);
            y += 20;
        }

        string infoText = $"Tempo: {song.Tempo} BPM";
        if (song.Capo > 0)
        {
            infoText += $"  |  Capo: {song.Capo}";
        }
        gfx.DrawString(infoText, _headerFont!, XBrushes.Gray,
            new XRect(Margin, y, ContentWidth, 15), XStringFormats.TopCenter);
        y += 25;

        return y;
    }

    private double DrawTuningInfo(XGraphics gfx, Song song, double y)
    {
        string tuningText = $"Tuning: {string.Join(" ", song.Tuning.StringNames)}";
        gfx.DrawString(tuningText, _headerFont!, XBrushes.Gray,
            new XRect(Margin, y, ContentWidth, 15), XStringFormats.TopLeft);
        y += 20;

        return y;
    }

    private double DrawChordDiagrams(XGraphics gfx, Song song, double y)
    {
        const double chordDiagramWidth = 60;
        const double chordDiagramHeight = 70;
        double x = Margin;

        foreach (var chord in song.Chords)
        {
            if (x + chordDiagramWidth > PageWidth - Margin)
            {
                x = Margin;
                y += chordDiagramHeight + 10;
            }

            DrawChordDiagram(gfx, chord, x, y, chordDiagramWidth, chordDiagramHeight);
            x += chordDiagramWidth + 10;
        }

        y += chordDiagramHeight + 20;
        return y;
    }

    private void DrawChordDiagram(XGraphics gfx, Chord chord, double x, double y, double width, double height)
    {
        gfx.DrawString(chord.Name, _chordFont!, XBrushes.Black,
            new XRect(x, y, width, 15), XStringFormats.TopCenter);
        y += 15;

        double fretboardHeight = height - 20;
        double stringSpacingLocal = (width - 10) / 5;
        double fretSpacing = fretboardHeight / 4;

        var pen = new XPen(XColors.Black, 0.5);
        for (int i = 0; i <= 4; i++)
        {
            double fy = y + i * fretSpacing;
            gfx.DrawLine(pen, x + 5, fy, x + width - 5, fy);
        }

        for (int i = 0; i < 6; i++)
        {
            double fx = x + 5 + i * stringSpacingLocal;
            gfx.DrawLine(pen, fx, y, fx, y + fretboardHeight);
        }

        for (int i = 0; i < 6; i++)
        {
            int fret = chord.Fingering[i];
            double fx = x + 5 + (5 - i) * stringSpacingLocal;

            if (fret == -1)
            {
                gfx.DrawString("x", _stringLabelFont!, XBrushes.Black,
                    new XRect(fx - 4, y - 12, 8, 10), XStringFormats.Center);
            }
            else if (fret == 0)
            {
                gfx.DrawString("o", _stringLabelFont!, XBrushes.Black,
                    new XRect(fx - 4, y - 12, 8, 10), XStringFormats.Center);
            }
            else
            {
                double fy = y + (fret - chord.BaseFret + 0.5) * fretSpacing;
                gfx.DrawEllipse(XBrushes.Black, fx - 4, fy - 4, 8, 8);
            }
        }
    }

    private void DrawTablature(XGraphics gfx, Song song, double y, PdfDocument document)
    {
        int measuresPerLine = 4;
        double measureWidth = (ContentWidth - 30) / measuresPerLine;
        int measureIndex = 0;

        while (measureIndex < song.Measures.Count)
        {
            if (y + TabLineHeight > PageHeight - Margin)
            {
                var page = document.AddPage();
                page.Width = XUnit.FromPoint(PageWidth);
                page.Height = XUnit.FromPoint(PageHeight);
                gfx = XGraphics.FromPdfPage(page);
                y = Margin;
            }

            int measuresThisLine = Math.Min(measuresPerLine, song.Measures.Count - measureIndex);
            double actualMeasureWidth = (ContentWidth - 30) / measuresThisLine;

            DrawStringLabels(gfx, y);
            DrawStrings(gfx, y);

            double x = Margin + 25;
            for (int i = 0; i < measuresThisLine; i++)
            {
                var measure = song.Measures[measureIndex + i];
                DrawMeasure(gfx, song, measure, x, y, actualMeasureWidth);
                x += actualMeasureWidth;
            }

            measureIndex += measuresThisLine;
            y += TabLineHeight;
        }
    }

    private void DrawStringLabels(XGraphics gfx, double y)
    {
        string[] labels = { "e", "B", "G", "D", "A", "E" };
        for (int i = 0; i < 6; i++)
        {
            double stringY = y + i * StringSpacing;
            gfx.DrawString(labels[i], _stringLabelFont!, XBrushes.DarkGray,
                new XPoint(Margin + 5, stringY + 3));
        }
    }

    private void DrawStrings(XGraphics gfx, double y)
    {
        var pen = new XPen(_stringColor, 0.5);
        for (int i = 0; i < 6; i++)
        {
            double stringY = y + i * StringSpacing;
            gfx.DrawLine(pen, Margin + 25, stringY, PageWidth - Margin, stringY);
        }
    }

    private void DrawMeasure(XGraphics gfx, Song song, Measure measure, double x, double y, double width)
    {
        var barPen = new XPen(_barLineColor, 1);

        gfx.DrawLine(barPen, x, y, x, y + 5 * StringSpacing);
        gfx.DrawLine(barPen, x + width, y, x + width, y + 5 * StringSpacing);

        if (!string.IsNullOrEmpty(measure.ChordName))
        {
            gfx.DrawString(measure.ChordName, _chordFont!, new XSolidBrush(_chordColor),
                new XPoint(x + 5, y - 12));
        }

        int positions = measure.TotalTicks / 8;
        double positionWidth = (width - 10) / Math.Max(positions, 1);

        // 绘制连接弧线
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
                    double x1 = x + 5 + displayPos1 * positionWidth + 6;
                    double x2 = x + 5 + displayPos2 * positionWidth - 6;
                    double noteY = y + (currentNote.String - 1) * StringSpacing;

                    DrawConnectionArc(gfx, x1, x2, noteY);
                }
            }
        }

        // 绘制音符
        foreach (var note in measure.Notes)
        {
            int displayPos = note.Position / 8;
            double noteX = x + 5 + displayPos * positionWidth;
            double noteY = y + (note.String - 1) * StringSpacing;

            DrawNote(gfx, note, noteX, noteY);
        }

        // 绘制节奏符号
        DrawRhythmNotation(gfx, measure, x, y, width, positionWidth);
    }

    private void DrawConnectionArc(XGraphics gfx, double x1, double x2, double y)
    {
        var pen = new XPen(_connectionColor, 1.2);
        double arcHeight = 8;

        var path = new XGraphicsPath();
        path.AddBezier(
            new XPoint(x1, y - 4),
            new XPoint(x1 + (x2 - x1) * 0.25, y - arcHeight),
            new XPoint(x1 + (x2 - x1) * 0.75, y - arcHeight),
            new XPoint(x2, y - 4));

        gfx.DrawPath(pen, path);
    }

    private void DrawRhythmNotation(XGraphics gfx, Measure measure, double x, double y, double width, double positionWidth)
    {
        double rhythmY = y + 5 * StringSpacing + 8;
        double stemHeight = 10;
        double beamSpacing = 3;

        var stemPen = new XPen(XColors.Black, 1);
        var beamPen = new XPen(XColors.Black, 1.5);

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

        for (int i = 0; i < notesByPosition.Count; i++)
        {
            var item = notesByPosition[i];
            int displayPos = item.Position / 8;
            double noteX = x + 5 + displayPos * positionWidth;
            int beamCount = GetBeamCount(item.Note.Duration);

            if (beamCount >= 0)
            {
                gfx.DrawLine(stemPen, noteX, rhythmY, noteX, rhythmY + stemHeight);
            }
        }

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
                    gfx.DrawLine(beamPen, firstX, beamY, lastX, beamY);
                }

                foreach (int idx in group)
                {
                    int beamCount = GetBeamCount(notesByPosition[idx].Note.Duration);
                    double noteX = x + 5 + notesByPosition[idx].Position / 8 * positionWidth;

                    for (int b = minBeams; b < beamCount; b++)
                    {
                        double beamY = rhythmY + stemHeight - 1 - b * beamSpacing;
                        gfx.DrawLine(beamPen, noteX, beamY, noteX + 4, beamY);
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
                    gfx.DrawLine(beamPen, noteX, beamY, noteX + 5, beamY + 2);
                }
            }
        }

        for (int i = 0; i < notesByPosition.Count; i++)
        {
            var item = notesByPosition[i];
            if (item.Note.Duration == NoteDuration.Quarter)
            {
                int displayPos = item.Position / 8;
                double noteX = x + 5 + displayPos * positionWidth;
                gfx.DrawEllipse(XBrushes.Black, noteX - 1.5, rhythmY + stemHeight + 2, 3, 3);
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

    private void DrawNote(XGraphics gfx, Note note, double x, double y)
    {
        bool hasTechnique = note.Technique != Technique.None;

        if (note.IsRest)
        {
            var restBrush = new XSolidBrush(XColor.FromArgb(255, 100, 100, 100));
            gfx.DrawRectangle(new XPen(XColors.Gray, 0.5), XBrushes.WhiteSmoke, x - 6, y - 5, 12, 10);
            gfx.DrawString("—", _noteFont!, restBrush, new XRect(x - 6, y - 5, 12, 10), XStringFormats.Center);
            return;
        }

        string fretText = note.Technique.HasFlag(Technique.Mute) ? "X" : note.Fret.ToString();
        string techniqueLabel = GetTechniqueLabel(note.Technique);

        var size = gfx.MeasureString(fretText, _noteFont!);
        double bgWidth = Math.Max(size.Width + 4, 12);

        if (hasTechnique)
        {
            var techBrush = new XSolidBrush(XColor.FromArgb(255, 255, 240, 235));
            var techPen = new XPen(_techniqueColor, 1);
            gfx.DrawRectangle(techPen, techBrush, x - bgWidth / 2, y - 5, bgWidth, 10);
        }
        else
        {
            gfx.DrawRectangle(XBrushes.White, x - bgWidth / 2, y - 5, bgWidth, 10);
        }

        var textBrush = hasTechnique ? new XSolidBrush(_techniqueColor) : XBrushes.Black;
        var font = hasTechnique ? _noteBoldFont! : _noteFont!;
        gfx.DrawString(fretText, font, textBrush, new XRect(x - bgWidth / 2, y - 5, bgWidth, 10), XStringFormats.Center);

        if (!string.IsNullOrEmpty(techniqueLabel))
        {
            gfx.DrawString(techniqueLabel, _techniqueFont!, new XSolidBrush(_techniqueColor),
                new XPoint(x - 4, y - 10));
        }
    }

    private static string GetTechniqueLabel(Technique technique)
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

        return string.Join("", parts);
    }
}
