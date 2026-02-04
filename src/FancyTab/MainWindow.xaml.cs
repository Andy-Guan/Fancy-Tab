using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using FancyTab.Controls;
using FancyTab.ViewModels;

namespace FancyTab;

/// <summary>
/// 主窗口
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        TabEditor.Focus();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // 忽略已处理的按键和系统按键
        if (e.Handled) return;

        // F1 显示快捷键帮助
        if (e.Key == Key.F1)
        {
            Shortcuts_Click(sender, e);
            e.Handled = true;
            return;
        }

        // 让ViewModel处理按键
        ViewModel.HandleKeyDown(e.Key, Keyboard.Modifiers);

        // 标记某些按键为已处理，避免默认行为
        if (e.Key >= Key.D0 && e.Key <= Key.D9 ||
            e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 ||
            e.Key == Key.Left || e.Key == Key.Right ||
            e.Key == Key.Up || e.Key == Key.Down ||
            e.Key == Key.Delete || e.Key == Key.Back ||
            e.Key == Key.Space ||
            (e.Key >= Key.A && e.Key <= Key.Z && Keyboard.Modifiers == ModifierKeys.None))
        {
            e.Handled = true;
        }

        // 刷新画布
        TabEditor.InvalidateVisual();
    }

    private void TabEditor_NoteClicked(object sender, NoteClickedEventArgs e)
    {
        ViewModel.NoteClicked(e.MeasureIndex, e.Position, e.StringNumber);
        TabEditor.InvalidateVisual();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // 这里可以添加未保存提示
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Shortcuts_Click(object sender, RoutedEventArgs e)
    {
        var message = @"键盘快捷键 / Keyboard Shortcuts

【导航 Navigation】
方向键 Arrow Keys - 移动光标 Move cursor
0-9 - 输入品数 Input fret
Delete - 删除音符 Delete note
Enter - 下一位置 Next position

【时值 Duration】
W - 全音符 Whole
Q - 四分音符 Quarter  
E - 八分音符 Eighth
. - 附点 Dotted

【技巧 Techniques】
H - 击弦 Hammer-on
P - 勾弦 Pull-off
B - 推弦 Bend
V - 揉弦 Vibrato
M - 闷音 Mute
S - 滑音 Slide (Shift+S下滑)
~ - 泛音 Harmonic

【文件 File】
Ctrl+N - 新建 New
Ctrl+O - 打开 Open
Ctrl+S - 保存 Save
Ctrl+E - 导出PDF Export PDF";

        MessageBox.Show(message, "快捷键 / Shortcuts", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var message = @"Fancy Tab
Version 1.0.0

吉他六线谱编辑器
Guitar Tablature Editor

简洁 · 高效 · 专业
Clean · Efficient · Professional";

        MessageBox.Show(message, "关于 / About", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
