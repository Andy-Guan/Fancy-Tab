using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FancyTab.Avalonia.Services;
using FancyTab.Core.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace FancyTab.Avalonia;

public partial class MainWindow : Window
{
    private MainViewModel _viewModel = null!;

    public MainWindow()
    {
        InitializeComponent();
        
        // 创建服务和 ViewModel
        var fileDialogService = new AvaloniaFileDialogService(this);
        _viewModel = new MainViewModel(fileDialogService, App.Localization);
        DataContext = _viewModel;
        
        // 注册事件
        KeyDown += Window_KeyDown;
        Closing += Window_Closing;
        
        // 注册 TabCanvas 事件
        TabEditor.NoteClicked += TabEditor_NoteClicked;
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        // 让 ViewModel 处理键盘输入
        _viewModel.HandleKeyDown(e.Key, e.KeyModifiers);
        
        // 触发重绘
        TabEditor.InvalidateVisual();
    }

    private async void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        // 如果有未保存的更改，提示用户
        // 可以根据需要实现保存提示
    }

    private void TabEditor_NoteClicked(object? sender, Controls.NoteClickedEventArgs e)
    {
        _viewModel.NoteClicked(e.MeasureIndex, e.Position, e.StringNumber);
        TabEditor.InvalidateVisual();
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void Shortcuts_Click(object? sender, RoutedEventArgs e)
    {
        var shortcuts = @"Keyboard Shortcuts

Navigation:
  Arrow Keys - Move cursor
  0-9 - Input fret number
  Delete - Delete note
  Space - Insert rest

Techniques:
  H - Hammer-on
  P - Pull-off
  B - Bend
  V - Vibrato
  M - Mute
  S - Slide up
  Shift+S - Slide down

Duration:
  W - Whole note
  Q - Quarter note
  E - Eighth note
  . - Toggle dotted

File:
  Ctrl+N - New file
  Ctrl+O - Open file
  Ctrl+S - Save file
  Ctrl+E - Export PDF";

        var box = MessageBoxManager.GetMessageBoxStandard(
            App.Localization.GetString("Shortcuts_Title", "Keyboard Shortcuts"),
            shortcuts,
            ButtonEnum.Ok);
        await box.ShowAsync();
    }

    private async void About_Click(object? sender, RoutedEventArgs e)
    {
        var about = $@"{App.Localization.GetString("About_Title", "About Fancy Tab")}

{App.Localization.GetString("About_Version", "Version 1.0.0")}

{App.Localization.GetString("About_Description", "A clean guitar tablature editor")}

Built with Avalonia UI
Cross-platform: Windows, macOS, Linux, Android";

        var box = MessageBoxManager.GetMessageBoxStandard(
            App.Localization.GetString("About_Title", "About"),
            about,
            ButtonEnum.Ok);
        await box.ShowAsync();
    }
}
