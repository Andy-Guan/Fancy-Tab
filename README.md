# Fancy Tab

简洁高效的吉他六线谱编辑器 / A Clean & Efficient Guitar Tablature Editor

**跨平台支持** - Windows 电脑 & Android 平板

---

## 功能特点

- **六线谱编辑** - 直观的可视化编辑界面
- **键盘快捷输入** - 数字键快速输入品数
- **多种调弦** - Standard、Drop D、Open G 等 7 种预设
- **吉他技巧** - 击弦(H)、勾弦(P)、滑音(S)、推弦(B)、揉弦(V)、闷音(M)等
- **节奏显示** - 音符下方显示时值符号（符干+连杠）
- **PDF导出** - 专业级乐谱导出
- **中英双语** - 界面语言可切换
- **触屏优化** - Android 平板触控友好界面

---

## 使用方法

### Windows 电脑

#### 方式一：下载安装包（推荐）

1. 前往 [Releases](https://github.com/yourusername/FancyTab/releases) 页面
2. 下载最新版 `FancyTab-Windows-x64.zip`
3. 解压到任意目录
4. 双击 `FancyTab.exe` 运行

#### 方式二：从源码运行

**环境要求：** [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

```bash
# 克隆仓库
git clone https://github.com/yourusername/FancyTab.git
cd FancyTab

# 运行桌面版
dotnet run --project src/FancyTab.Desktop/FancyTab.Desktop.csproj
```

---

### Android 平板

#### 方式一：下载 APK 安装包（推荐）

1. 前往 [Releases](https://github.com/yourusername/FancyTab/releases) 页面
2. 下载最新版 `FancyTab-Android.apk`
3. 在平板上打开 APK 文件
4. 如提示"未知来源"，请在设置中允许安装
5. 安装完成后打开应用即可使用

**系统要求：** Android 5.0 或更高版本

#### 方式二：从源码构建 APK

**环境要求：**
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Android Studio](https://developer.android.com/studio)（用于安装 Android SDK）

```bash
# 安装 Android workload
dotnet workload install android

# 设置 Android SDK 路径（安装 Android Studio 后）
# Windows CMD:
setx ANDROID_HOME "%LOCALAPPDATA%\Android\Sdk"

# 构建 APK
dotnet publish src/FancyTab.Android/FancyTab.Android.csproj -c Release

# APK 输出位置：
# src/FancyTab.Android/bin/Release/net9.0-android/com.fancytab.guitar-Signed.apk
```

---

## 操作指南

### Windows 键盘快捷键

| 按键 | 功能 |
|------|------|
| **0-9** | 输入品数 |
| **方向键** | 移动光标 |
| **Delete** | 删除音符 |
| **Space** | 插入休止符 |
| **W/H/Q/8/16** | 时值选择（全/二分/四分/八分/十六分） |
| **H** | 击弦 Hammer-on |
| **P** | 勾弦 Pull-off |
| **S** | 滑音 Slide |
| **B** | 推弦 Bend |
| **V** | 揉弦 Vibrato |
| **M** | 闷音 Mute |
| **Ctrl+N** | 新建文件 |
| **Ctrl+O** | 打开文件 |
| **Ctrl+S** | 保存文件 |
| **Ctrl+E** | 导出 PDF |
| **F1** | 快捷键帮助 |

### Android 平板触控操作

| 操作 | 功能 |
|------|------|
| **点击六线谱** | 选择位置 |
| **点击工具栏按钮** | 选择功能 |
| **数字键盘** | 输入品数 |
| **滑动** | 滚动乐谱 |

---

## 文件格式

- **保存格式：** `.gtab`（JSON 格式，可用文本编辑器查看）
- **导出格式：** `.pdf`（标准 PDF，可打印或分享）

文件可在 Windows 和 Android 之间互相传输使用。

---

## 项目结构（开发者参考）

```
Fancy Tab/
├── src/
│   ├── FancyTab/                 # 原版 WPF 应用 (Windows)
│   ├── FancyTab.Core/            # 跨平台核心库（业务逻辑）
│   ├── FancyTab.Avalonia/        # Avalonia UI 共享层
│   ├── FancyTab.Desktop/         # Windows 桌面入口
│   └── FancyTab.Android/         # Android 入口
├── docs/                         # 文档
└── README.md
```

### 技术栈

| 版本 | 框架 | 支持平台 |
|------|------|----------|
| 原版 | .NET 8 + WPF | Windows |
| 跨平台版 | .NET 9 + Avalonia UI | Windows, Android, macOS, Linux |

---

## 常见问题

### Windows

**Q: 提示缺少 .NET 运行时？**  
A: 下载安装 [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)

**Q: 杀毒软件报警？**  
A: 这是误报，可添加信任或从源码自行编译

### Android

**Q: 安装时提示"未知来源"？**  
A: 设置 → 安全 → 允许安装未知应用 → 选择文件管理器 → 允许

**Q: 安装后闪退？**  
A: 请确保 Android 版本 ≥ 5.0，如问题持续请提交 Issue

**Q: 如何传输文件到电脑？**  
A: 使用 USB 数据线、云盘、或蓝牙传输 `.gtab` 文件

---

## 许可证

MIT License

---

**Fancy Tab** - 让吉他谱创作更简单，随时随地
