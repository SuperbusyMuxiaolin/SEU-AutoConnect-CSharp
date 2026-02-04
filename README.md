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

