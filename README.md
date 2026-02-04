# SEU 校园网自动连接工具 (C# WPF版本)

## 简介

这是使用 C# 和 WPF 开发的 SEU 校园网自动连接工具，专为 Windows 平台优化。相比 Python 版本，具有更好的稳定性、性能和用户体验。

## 主要特性

✅ **现代化 UI 界面**
- 直观的图形化配置界面
- 实时状态显示和日志输出
- 支持最小化到系统托盘

✅ **核心功能**
- 自动监测网络连接状态
- 自动连接到指定 WiFi (SEU-WLAN)
- 自动进行校园网认证
- 断网后自动重连

✅ **配置管理**
- 支持导入原 Python 版本的 config.json
- 可视化配置编辑和保存
- 配置文件自动管理

✅ **系统集成**
- 开机自启动功能
- 系统托盘常驻
- 单实例运行保护

## 技术栈

- **.NET 6.0** - 现代化的 .NET 框架
- **WPF** - Windows Presentation Foundation UI 框架
- **ManagedNativeWifi** - Windows WiFi API 封装
- **Hardcodet.NotifyIcon.Wpf** - 系统托盘支持
- **Newtonsoft.Json** - JSON 配置文件处理

## 系统要求

- Windows 10/11 (64位)
- .NET 6.0 Runtime 或更高版本

---

## 快速开始

### 方式一：使用已编译版本（推荐）

1. 下载 Release 版本
2. 解压到任意目录
3. 运行 `SEU-AutoConnect.exe`
4. 在界面中配置用户名和密码
5. 点击"启动服务"

### 方式二：从源码构建

#### 环境准备

**必需软件**
1. **Visual Studio 2022** (Community/Professional/Enterprise)
   - 下载地址：https://visualstudio.microsoft.com/zh-hans/
   - 安装时选择".NET 桌面开发"工作负载

2. **.NET 6.0 SDK**
   - 通常随 Visual Studio 2022 一起安装
   - 独立下载：https://dotnet.microsoft.com/download/dotnet/6.0

**验证环境**

打开 PowerShell 或命令提示符，执行：

```powershell
dotnet --version
```

应该显示 6.0.x 或更高版本。

#### 构建步骤

**使用 Visual Studio（推荐新手）**

1. **打开项目**
   - 双击 `SEU-AutoConnect.csproj` 文件
   - 或在 Visual Studio 中选择"打开项目/解决方案"

2. **还原 NuGet 包**
   - Visual Studio 会自动还原所需的 NuGet 包
   - 如未自动还原，右键解决方案 → "还原 NuGet 程序包"

3. **构建项目**
   - 选择 Release 配置
   - 点击"生成" → "生成解决方案"
   - 或按快捷键 `Ctrl+Shift+B`

4. **运行程序**
   - 按 `F5` 或点击"启动"按钮
   - 程序将在 `bin\Release\net6.0-windows\` 目录下生成

**使用命令行（推荐高级用户）**

```powershell
# 1. 进入项目目录
cd SEU-AutoConnect-CSharp

# 2. 还原依赖
dotnet restore

# 3. 编译项目（Debug 版本）
dotnet build -c Debug

# 或编译 Release 版本
dotnet build -c Release

# 4. 运行程序
dotnet run -c Release
```

#### 发布独立可执行文件

**选项一：单文件版本（包含 .NET Runtime）**

优点：用户无需安装 .NET Runtime  
缺点：文件较大（约 60-80 MB）

```powershell
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true
```

**选项二：框架依赖版本**

优点：文件小（约 1-2 MB）  
缺点：用户需要安装 .NET 6.0 Runtime

```powershell
dotnet publish -c Release -r win-x64 --self-contained false `
  -p:PublishSingleFile=true
```

**选项三：最小化体积（高级）**

使用 Trimming 和 AOT 优化：

```powershell
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:PublishTrimmed=true `
  -p:TrimMode=link
```

**注意：** Trimming 可能导致某些反射功能失效，请充分测试。

#### 输出位置

编译后的文件位于：

- **Build 输出：** `bin\Release\net6.0-windows\`
- **Publish 输出：** `bin\Release\net6.0-windows\win-x64\publish\`

---

## 配置说明

### 配置文件格式

程序支持两种配置文件格式：

**config格式**
```json
{
  "seu": {
    "username": "你的学号",
    "password": "你的密码",
    "login_ip": "http://202.119.25.2",
    "not_sign_in_title": "上网登录页",
    "result_return": "\"result\":\"1\"",
    "signed_in_title": "注销页"
  },
  "wifi_ssid": "SEU-WLAN",
  "check_interval": 5,
  "reconnect_delay": 3,
  "max_retry": 3
}
```

### 配置项说明

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| username | 学号 | 必填 |
| password | 密码 | 必填 |
| wifi_ssid | WiFi名称 | SEU-WLAN |
| check_interval | 检查间隔（秒） | 5 |
| reconnect_delay | 重连延迟（秒） | 3 |
| max_retry | 最大重试次数 | 3 |
| auto_start_service | 开机自启后自动启动服务 | false |
| start_minimized | 静默启动到托盘 | false |

---

## 使用说明

### 首次使用

1. **启动程序**
   - 双击 `SEU-AutoConnect.exe`

2. **配置账号**
   - 填写学号和密码
   - 点击"保存配置"

3. **启动服务**
   - 点击"启动服务"按钮
   - 程序开始自动监控网络状态

4. **设置开机自启**
   - 右键系统托盘图标
   - 勾选"开机自启动"

### 导入原配置

导入配置好的`config.json` 文件，程序会自动识别并导入配置

### 系统托盘操作

程序最小化后会常驻系统托盘：
- **双击图标**：显示主窗口
- **右键菜单**：
  - 显示主窗口
  - 立即连接
  - 开机自启动（开关）
  - 退出程序

---

## 常见问题

### 构建相关

**Q: 编译时提示找不到 .NET SDK**

解决方案：
```powershell
# 下载并安装 .NET 6.0 SDK
# https://dotnet.microsoft.com/download/dotnet/6.0

# 验证安装
dotnet --version
```

**Q: NuGet 包还原失败**

解决方案：
```powershell
# 清除 NuGet 缓存
dotnet nuget locals all --clear

# 重新还原
dotnet restore
```

**Q: 提示缺少 Windows SDK**

解决方案：
- 打开 Visual Studio Installer
- 修改安装
- 勾选"Windows 10 SDK"或"Windows 11 SDK"

**Q: 发布后程序无法运行**

解决方案：
1. 检查是否包含所有必需的 DLL
2. 使用依赖分析工具检查缺失的依赖：
   ```powershell
   # 在发布目录运行
   dumpbin /dependents SEU-AutoConnect.exe
   ```
3. 尝试以管理员身份运行

### 使用相关

**Q: 提示"无法连接WiFi"**

解决方案：
1. 确保 WiFi 名称正确（区分大小写）
2. 确保该 WiFi 之前已经连接过（Windows 有保存配置）
3. 尝试手动连接一次该 WiFi

**Q: 提示"登录失败"**

解决方案：
1. 检查用户名和密码是否正确
2. 确认账号状态正常
3. 查看日志获取详细错误信息

**Q: 程序无法启动**

解决方案：
1. 确认已安装 .NET 6.0 Runtime
2. 以管理员身份运行
3. 检查 Windows 防火墙设置

**Q: 无法设置开机自启动**

解决方案：
1. 以管理员身份运行程序
2. 检查注册表权限
3. 使用任务计划程序手动添加

---

## 开发说明

### 项目结构

```
SEU-AutoConnect-CSharp/
├── App.xaml                  # 应用程序定义
├── App.xaml.cs              # 应用程序逻辑
├── MainWindow.xaml          # 主窗口界面
├── MainWindow.xaml.cs       # 主窗口逻辑
├── ConfigManager.cs         # 配置管理
├── NetworkService.cs        # 网络服务
├── AutoStartManager.cs      # 自启动管理
├── Resources/
│   ├── Styles.xaml         # UI 样式
│   └── ico/                # 应用图标
│       ├── 网络.ico        # 主图标
│       ├── Check.ico       # 检查图标
│       ├── Cross.ico       # 错误图标
│       ├── Questionmark.ico # 问号图标
│       └── Tips.ico        # 提示图标
├── config.json             # 配置文件
└── SEU-AutoConnect.csproj  # 项目文件
```

### 核心类说明

- **ConfigManager**: 处理配置文件的读取、保存和导入
- **NetworkService**: 处理 WiFi 连接、网络检测和校园网认证
- **AutoStartManager**: 管理开机自启动功能
- **MainWindow**: 主窗口，处理 UI 交互和服务循环


### 测试清单

发布前请确保测试以下功能：

- [ ] 程序正常启动
- [ ] 配置文件导入导出
- [ ] WiFi 连接功能
- [ ] 校园网认证功能
- [ ] 系统托盘图标
- [ ] 开机自启动
- [ ] 单实例检查
- [ ] 最小化到托盘
- [ ] 日志正常显示

### 版本发布

**版本号规范**

使用语义化版本：`主版本.次版本.修订号`

```xml
<!-- 在 .csproj 中设置 -->
<PropertyGroup>
  <Version>1.0.0</Version>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
</PropertyGroup>
```

**发布清单**

1. 更新版本号
2. 更新 README.md 中的更新日志
3. 编译 Release 版本
4. 测试所有功能
5. 创建发布包（ZIP）
6. 创建安装包（可选）
7. 准备发布说明

**发布包内容**

```
SEU-AutoConnect-v1.0.0/
├── SEU-AutoConnect.exe
├── config.json.example
├── README.md
├── LICENSE
└── CHANGELOG.md
```