# Fancy Tab

简洁高效的吉他六线谱编辑器 / A Clean & Efficient Guitar Tablature Editor

---

## 功能特点

- **六线谱编辑** - 直观的可视化编辑界面
- **键盘快捷输入** - 数字键快速输入品数
- **多种调弦** - Standard、Drop D、Open G 等 7 种预设
- **吉他技巧** - 击弦(H)、勾弦(P)、滑音(S)、推弦(B)、揉弦(V)、闷音(M)等
- **节奏显示** - 音符下方显示时值符号（符干+连杠）
- **PDF导出** - 专业级乐谱导出
- **中英双语** - 界面语言可切换

---

## 快速开始

### 环境要求

- Windows 10/11 (64位)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### 运行方法

```bash
# 克隆仓库
git clone https://github.com/yourusername/FancyTab.git
cd FancyTab

# 运行
dotnet run --project src/FancyTab/FancyTab.csproj
```

或者双击 `FancyTab.bat`（需要先安装 .NET 8 SDK）

---

## 快捷键

| 按键 | 功能 |
|------|------|
| 0-9 | 输入品数 |
| 方向键 | 移动光标 |
| Delete | 删除音符 |
| Space | 插入休止符 |
| W/H/Q/8/16 | 时值选择 |
| H | 击弦 Hammer-on |
| P | 勾弦 Pull-off |
| S / Shift+S | 滑音 Slide |
| B | 推弦 Bend |
| V | 揉弦 Vibrato |
| M | 闷音 Mute |
| Ctrl+N/O/S | 新建/打开/保存 |
| Ctrl+E | 导出PDF |
| F1 | 快捷键帮助 |

---

## 项目结构

```
Fancy Tab/
├── src/FancyTab/           # 源代码目录
│   ├── Controls/           # 自定义控件
│   │   └── TabCanvas.cs    # 六线谱画布（核心渲染）
│   ├── Models/             # 数据模型
│   │   ├── Song.cs         # 歌曲模型
│   │   ├── Measure.cs      # 小节模型
│   │   ├── Note.cs         # 音符模型
│   │   └── Tuning.cs       # 调弦模型
│   ├── ViewModels/         # 视图模型 (MVVM)
│   │   └── MainViewModel.cs
│   ├── Services/           # 服务层
│   │   ├── FileService.cs  # 文件读写
│   │   └── PdfExportService.cs  # PDF导出
│   ├── Utils/              # 工具类
│   │   └── KeyboardHandler.cs   # 键盘快捷键
│   ├── Resources/          # 资源文件
│   │   ├── Styles.xaml     # 样式定义
│   │   └── Strings.*.xaml  # 多语言字符串
│   ├── MainWindow.xaml     # 主窗口界面
│   └── MainWindow.xaml.cs  # 主窗口代码
├── docs/                   # 文档
├── FancyTab.bat           # 启动脚本
├── FancyTab.sln           # 解决方案文件
└── README.md
```

### src 目录说明

`src/FancyTab/` 是项目的核心源代码目录，采用 MVVM 架构：

| 目录 | 用途 |
|------|------|
| **Controls/** | 自定义WPF控件，`TabCanvas.cs` 负责六线谱的绘制和交互 |
| **Models/** | 数据模型，定义歌曲、小节、音符、调弦等数据结构 |
| **ViewModels/** | 视图模型，处理UI逻辑和数据绑定 |
| **Services/** | 业务服务，包括文件操作和PDF导出 |
| **Utils/** | 工具类，如键盘输入处理 |
| **Resources/** | XAML资源文件，包括样式和多语言支持 |

---

## 技术栈

- **.NET 8** + **WPF** (Windows Presentation Foundation)
- **MVVM** 架构 (CommunityToolkit.Mvvm)
- **PDFsharp** - PDF生成
- **System.Text.Json** - JSON序列化

---

## 文件格式

项目文件使用 JSON 格式（.ftab / .json）：

```json
{
  "title": "歌曲名",
  "artist": "艺术家",
  "tempo": 120,
  "tuning": { "name": "Standard", ... },
  "measures": [...]
}
```

---

## 许可证

MIT License

---

**Fancy Tab** - 让吉他谱创作更简单
