using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyTab.Models;
using FancyTab.Services;
using FancyTab.Utils;

namespace FancyTab.ViewModels;

/// <summary>
/// 主视图模型
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly FileService _fileService = new();
    private readonly PdfExportService _pdfExportService = new();
    private readonly KeyboardHandler _keyboardHandler = new();

    [ObservableProperty]
    private Song _song = Song.CreateNew();

    [ObservableProperty]
    private int _currentMeasure = 0;

    [ObservableProperty]
    private int _currentPosition = 0;

    [ObservableProperty]
    private int _currentString = 1;

    [ObservableProperty]
    private NoteDuration _currentDuration = NoteDuration.Quarter;

    [ObservableProperty]
    private bool _showNoteNames = false;

    [ObservableProperty]
    private string _statusText = "";

    [ObservableProperty]
    private string _currentLanguage = "zh-CN";

    [ObservableProperty]
    private bool _isDotted = false;

    [ObservableProperty]
    private Technique _currentTechnique = Technique.None;

    [ObservableProperty]
    private bool _showHelpTip = true;

    public string WindowTitle => _fileService.HasUnsavedChanges 
        ? $"Fancy Tab - {Song.Title} *" 
        : $"Fancy Tab - {Song.Title}";

    public MainViewModel()
    {
        UpdateStatus();
    }

    #region 文件操作命令

    [RelayCommand]
    private void NewFile()
    {
        Song = _fileService.CreateNew();
        CurrentMeasure = 0;
        CurrentPosition = 0;
        CurrentString = 1;
        OnPropertyChanged(nameof(WindowTitle));
        UpdateStatus();
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var song = await _fileService.LoadAsync();
        if (song != null)
        {
            Song = song;
            CurrentMeasure = 0;
            CurrentPosition = 0;
            CurrentString = 1;
            OnPropertyChanged(nameof(WindowTitle));
            UpdateStatus();
        }
    }

    [RelayCommand]
    private async Task SaveFileAsync()
    {
        await _fileService.SaveAsync(Song);
        OnPropertyChanged(nameof(WindowTitle));
        UpdateStatus();
    }

    [RelayCommand]
    private async Task SaveFileAsAsync()
    {
        await _fileService.SaveAsAsync(Song);
        OnPropertyChanged(nameof(WindowTitle));
        UpdateStatus();
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        var success = await _pdfExportService.ExportAsync(Song);
        StatusText = success 
            ? LocalizationService.GetString("Status_PdfExported", "PDF exported successfully") 
            : LocalizationService.GetString("Status_PdfExportFailed", "PDF export failed");
    }

    #endregion

    #region 编辑操作

    public void HandleKeyDown(Key key, ModifierKeys modifiers)
    {
        var action = _keyboardHandler.HandleKey(key, modifiers);
        if (action == null) return;

        switch (action.Type)
        {
            case ActionType.InputFret:
                if (action.Fret.HasValue)
                {
                    InputFret(action.Fret.Value);
                }
                break;

            case ActionType.DeleteNote:
                DeleteCurrentNote();
                break;

            case ActionType.MoveCursor:
                if (action.Direction.HasValue)
                {
                    MoveCursor(action.Direction.Value);
                }
                break;

            case ActionType.SetTechnique:
                if (action.Technique.HasValue)
                {
                    ApplyTechnique(action.Technique.Value);
                }
                break;

            case ActionType.SetDuration:
                if (action.Duration.HasValue)
                {
                    CurrentDuration = action.Duration.Value;
                    UpdateStatus();
                }
                break;

            case ActionType.InsertRest:
                InsertRest();
                break;

            case ActionType.AddMeasure:
                AddMeasure();
                break;

            case ActionType.DeleteMeasure:
                DeleteCurrentMeasure();
                break;

            case ActionType.ToggleDot:
                IsDotted = !IsDotted;
                UpdateStatus();
                break;
        }
    }

    private void InputFret(int fret)
    {
        if (CurrentMeasure >= Song.Measures.Count) return;

        var measure = Song.Measures[CurrentMeasure];
        var note = new Note
        {
            String = CurrentString,
            Fret = fret,
            Duration = CurrentDuration,
            IsDotted = IsDotted,
            Technique = CurrentTechnique,
            Position = CurrentPosition
        };

        measure.AddNote(note);
        _fileService.HasUnsavedChanges = true;
        OnPropertyChanged(nameof(WindowTitle));

        // 自动移动到下一位置
        MoveToNextPosition();
        UpdateStatus();

        // 清除技巧状态
        CurrentTechnique = Technique.None;
    }

    private void DeleteCurrentNote()
    {
        if (CurrentMeasure >= Song.Measures.Count) return;

        var measure = Song.Measures[CurrentMeasure];
        measure.RemoveNote(CurrentPosition, CurrentString);
        _fileService.HasUnsavedChanges = true;
        OnPropertyChanged(nameof(WindowTitle));
        UpdateStatus();
    }

    private void MoveCursor(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                MoveToPreviousPosition();
                break;
            case Direction.Right:
                MoveToNextPosition();
                break;
            case Direction.Up:
                if (CurrentString > 1) CurrentString--;
                break;
            case Direction.Down:
                if (CurrentString < 6) CurrentString++;
                break;
        }
        UpdateStatus();
    }

    private void MoveToNextPosition()
    {
        if (Song.Measures.Count == 0) return;

        var measure = Song.Measures[CurrentMeasure];
        int step = NoteCalculator.GetTicksForDuration(CurrentDuration, IsDotted);
        CurrentPosition += step;

        if (CurrentPosition >= measure.TotalTicks)
        {
            CurrentPosition = 0;
            if (CurrentMeasure < Song.Measures.Count - 1)
            {
                CurrentMeasure++;
            }
            else
            {
                // 自动添加新小节
                Song.AddMeasure();
                CurrentMeasure++;
            }
        }
    }

    private void MoveToPreviousPosition()
    {
        if (Song.Measures.Count == 0) return;

        int step = NoteCalculator.GetTicksForDuration(CurrentDuration, IsDotted);
        CurrentPosition -= step;

        if (CurrentPosition < 0)
        {
            if (CurrentMeasure > 0)
            {
                CurrentMeasure--;
                var measure = Song.Measures[CurrentMeasure];
                CurrentPosition = measure.TotalTicks - step;
            }
            else
            {
                CurrentPosition = 0;
            }
        }
    }

    private void ApplyTechnique(Technique technique)
    {
        if (CurrentMeasure >= Song.Measures.Count) return;

        var measure = Song.Measures[CurrentMeasure];
        var existingNote = measure.GetNoteAt(CurrentPosition, CurrentString);

        if (existingNote != null)
        {
            // 切换技巧
            if (existingNote.Technique == technique)
            {
                existingNote.Technique = Technique.None;
            }
            else
            {
                existingNote.Technique = technique;
            }
            _fileService.HasUnsavedChanges = true;
            OnPropertyChanged(nameof(WindowTitle));
        }
        else
        {
            // 预设技巧用于下一个音符
            CurrentTechnique = technique;
        }

        UpdateStatus();
    }

    private void InsertRest()
    {
        if (CurrentMeasure >= Song.Measures.Count) return;

        var measure = Song.Measures[CurrentMeasure];
        var note = new Note
        {
            String = CurrentString,
            IsRest = true,
            Duration = CurrentDuration,
            Position = CurrentPosition
        };

        measure.AddNote(note);
        _fileService.HasUnsavedChanges = true;
        OnPropertyChanged(nameof(WindowTitle));
        MoveToNextPosition();
        UpdateStatus();
    }

    [RelayCommand]
    private void AddMeasure()
    {
        Song.AddMeasure();
        _fileService.HasUnsavedChanges = true;
        OnPropertyChanged(nameof(WindowTitle));
        UpdateStatus();
    }

    [RelayCommand]
    private void InsertMeasure()
    {
        Song.InsertMeasure(CurrentMeasure);
        _fileService.HasUnsavedChanges = true;
        OnPropertyChanged(nameof(WindowTitle));
        UpdateStatus();
    }

    private void DeleteCurrentMeasure()
    {
        if (Song.Measures.Count > 1)
        {
            Song.RemoveMeasure(CurrentMeasure);
            if (CurrentMeasure >= Song.Measures.Count)
            {
                CurrentMeasure = Song.Measures.Count - 1;
            }
            CurrentPosition = 0;
            _fileService.HasUnsavedChanges = true;
            OnPropertyChanged(nameof(WindowTitle));
            UpdateStatus();
        }
    }

    #endregion

    #region 设置操作

    [RelayCommand]
    private void SetDuration(NoteDuration duration)
    {
        CurrentDuration = duration;
        UpdateStatus();
    }

    [RelayCommand]
    private void ToggleNoteNames()
    {
        ShowNoteNames = !ShowNoteNames;
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        App.Localization.ToggleLanguage();
        CurrentLanguage = App.Localization.CurrentLanguage;
        UpdateStatus();
    }

    [RelayCommand]
    private void HideHelpTip()
    {
        ShowHelpTip = false;
    }

    [RelayCommand]
    private void SetTuning(string tuningName)
    {
        var tuning = Tuning.Presets.FirstOrDefault(t => t.Name == tuningName);
        if (tuning != null)
        {
            Song.Tuning = tuning.Clone();
            _fileService.HasUnsavedChanges = true;
            OnPropertyChanged(nameof(WindowTitle));
            UpdateStatus();
        }
    }

    #endregion

    #region 辅助方法

    private void UpdateStatus()
    {
        var durationName = NoteCalculator.GetDurationName(CurrentDuration, CurrentLanguage);
        var measureInfo = $"M:{CurrentMeasure + 1}/{Song.Measures.Count}";
        var posInfo = $"P:{CurrentPosition / 8 + 1}";
        var stringInfo = $"S:{CurrentString}";
        var dotInfo = IsDotted ? " [.]" : "";
        var techInfo = CurrentTechnique != Technique.None 
            ? $" [{NoteCalculator.GetTechniqueName(CurrentTechnique, CurrentLanguage)}]" 
            : "";

        StatusText = $"{measureInfo}  {posInfo}  {stringInfo}  |  {durationName}{dotInfo}{techInfo}";
    }

    public void NoteClicked(int measureIndex, int position, int stringNumber)
    {
        CurrentMeasure = measureIndex;
        CurrentPosition = position;
        CurrentString = stringNumber;
        UpdateStatus();
    }

    #endregion
}
