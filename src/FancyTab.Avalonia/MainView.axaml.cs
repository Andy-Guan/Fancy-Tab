using Avalonia.Controls;
using FancyTab.Avalonia.Services;
using FancyTab.Core.ViewModels;

namespace FancyTab.Avalonia;

public partial class MainView : UserControl
{
    private MainViewModel _viewModel = null!;

    public MainView()
    {
        InitializeComponent();
        
        // 创建服务和 ViewModel
        // 注意：在 Android 上，文件对话框服务需要特殊处理
        var fileDialogService = new AvaloniaFileDialogService();
        _viewModel = new MainViewModel(fileDialogService, App.Localization);
        DataContext = _viewModel;
        
        // 注册 TabCanvas 事件
        TabEditor.NoteClicked += TabEditor_NoteClicked;
    }

    private void TabEditor_NoteClicked(object? sender, Controls.NoteClickedEventArgs e)
    {
        _viewModel.NoteClicked(e.MeasureIndex, e.Position, e.StringNumber);
        TabEditor.InvalidateVisual();
    }
}
