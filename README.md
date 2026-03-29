# 网易云音乐工具 (NetEase Cloud Music Tool)

一个基于 WPF 的网易云音乐 NCM 文件解密和下载工具，采用 MVVM 架构设计，支持现代化的浅色主题界面。

![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-blue)
![WPF](https://img.shields.io/badge/WPF-Application-orange)
![License](https://img.shields.io/badge/License-MIT-green)

## ✨ 功能特性

### 1. NCM 文件解密转换
- **单文件模式**：选择单个 NCM 文件进行解密
- **批量文件夹模式**：扫描整个文件夹中的所有 NCM 文件
- **多文件选择模式**：同时选择多个 NCM 文件
- **支持输出格式**：MP3、FLAC、WAV
- **实时进度显示**：带进度条和百分比显示
- **取消功能**：支持随时取消转换任务

### 2. 音乐下载功能
- 通过网易云音乐 ID 下载歌曲
- 支持批量下载
- 实时下载进度显示

### 3. 现代化 UI 设计
- **浅色主题**：舒适的浅色卡片式界面
- **圆角设计**：现代化的圆角按钮和卡片
- **动画效果**：
  - 按钮点击动画
  - 列表项悬停效果
  - 进度条动画
  - 完成提示动画
- **响应式布局**：自适应窗口大小

## 🛠️ 技术架构

### 项目结构
```
网易云音乐下载/
├── Commands/
│   └── RelayCommand.cs          # MVVM 命令实现
├── Converters/
│   ├── CountToVisibilityConverter.cs
│   ├── EnumToBooleanConverter.cs
│   ├── InputModeConverter.cs
│   ├── InverseBoolConverter.cs
│   ├── InverseBooleanConverter.cs
│   └── ProgressBarWidthConverter.cs
├── Models/
│   ├── DownloadTaskInfo.cs      # 下载任务模型
│   ├── InputMode.cs             # 输入模式枚举
│   └── NcmFileInfo.cs           # NCM 文件信息模型
├── Services/
│   ├── AudioConverterService.cs # NCM 解密核心服务
│   └── NeteaseDownloadService.cs # 音乐下载服务
├── ViewModels/
│   ├── MainViewModel.cs         # 主视图模型
│   └── ViewModelBase.cs         # 视图模型基类
├── MainWindow.xaml              # 主窗口 UI
├── MainWindow.xaml.cs
├── App.xaml                     # 应用程序资源
└── App.xaml.cs
```

### NCM 解密原理

本项目实现了网易云音乐 NCM 文件的完整解密流程：

1. **文件头验证**：检查 "CTENFDAM" 魔数
2. **密钥解密**：
   - XOR 0x64 预处理
   - AES-128-ECB 解密（密钥：hzHRAmso5kInbaxW）
   - 去除 "neteasecloudmusic" 前缀
   - PKCS#7 填充去除
3. **音频解密**：
   - 使用网易云变种 RC4 算法
   - KSA（密钥调度算法）同标准 RC4
   - PRGA（伪随机生成算法）不交换且 j=(i+S[i])
4. **元数据提取**：解析 JSON 格式的歌曲信息

### 技术栈
- **框架**：.NET Framework 4.7.2
- **UI 框架**：WPF (Windows Presentation Foundation)
- **设计模式**：MVVM (Model-View-ViewModel)
- **加密算法**：
  - AES-128-ECB
  - 变种 RC4
- **异步编程**：async/await + CancellationToken

## 📋 系统要求

- Windows 7 SP1 或更高版本
- .NET Framework 4.7.2 或更高版本
- Visual Studio 2019 或更高版本（用于开发）

## 🚀 使用方法

### 1. NCM 文件转换

1. 启动应用程序
2. 选择输入模式：
   - **单文件**：点击"浏览"选择单个 .ncm 文件
   - **批量文件夹**：选择包含 NCM 文件的文件夹
   - **多文件**：按住 Ctrl 选择多个 NCM 文件
3. 选择输出格式（MP3/FLAC/WAV）
4. 选择输出文件夹
5. 点击"开始转换"
6. 等待转换完成

### 2. 音乐下载

1. 切换到"下载音乐"标签页
2. 输入网易云音乐歌曲 ID
3. 点击"添加任务"
4. 点击"开始下载"

## 🔧 编译说明

### 使用 Visual Studio
1. 克隆仓库
2. 使用 Visual Studio 2019+ 打开 `.sln` 文件
3. 按 `F5` 或点击"开始调试"

### 使用命令行
```bash
# 还原 NuGet 包
nuget restore

# 编译项目
msbuild 网易云音乐下载.csproj /p:Configuration=Release
```

## 📁 文件格式说明

### NCM 文件结构
```
[8 字节] 魔数 "CTENFDAM"
[2 字节] 版本号 (0x03 0x00)
[4 字节] 加密密钥长度
[N 字节] 加密密钥数据
[4 字节] 元数据长度
[M 字节] 元数据（JSON 格式）
[5 字节] CRC32（可能为空）
[剩余] 加密音频数据
```

### 解密流程
```
加密密钥 → XOR 0x64 → AES-128-ECB 解密 → 去除前缀 → RC4 密钥
加密音频 → 变种 RC4 解密 → 原始音频数据
```

## ⚠️ 免责声明

本项目仅供学习研究使用，请勿用于商业用途。使用本项目产生的任何后果由使用者自行承担。

## 📄 许可证

本项目采用 [MIT 许可证](LICENSE) 开源。

## 🙏 致谢

- 感谢 [ncmdump](https://github.com/anonymous5l/ncmdump) 项目的启发
- 感谢网易云音乐提供的音乐服务

## 📞 联系方式

如有问题或建议，欢迎提交 Issue 或 Pull Request。

---

**注意**：本项目与网易云音乐官方无关，仅为第三方工具。
