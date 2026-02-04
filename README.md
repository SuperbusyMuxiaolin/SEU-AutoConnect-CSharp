# SEU 校园网自动连接工具 (C# WPF版本)

## 简介

这是使用 C# 和 WPF 开发的 SEU 校园网自动连接工具，专为 Windows 平台优化。

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
- 通过config.json配置参数
- 可视化配置编辑和保存
- 配置文件自动管理

✅ **系统集成**
- 开机自启动功能
- 系统托盘常驻
- 单实例运行保护


## 系统要求

- Windows 10/11 (64位)
- .NET 6.0 Runtime 或更高版本

---


## 使用

### 1、填写配置文件

**config.json（扁平格式，推荐）**
```json
{
   "username": "你的学号",
   "password": "你的密码",
   "wifi_ssid": "SEU-WLAN",
   "login_ip": "http://202.119.25.2",
   "not_sign_in_title": "上网登录页",
   "result_return": "\"result\":\"1\"",
   "signed_in_title": "注销页",
   "login_request_url_template": "https://w.seu.edu.cn:801/eportal/?c=Portal&a=login&callback=dr1003&login_method=1&user_account=%2C0%2C{username}&user_password={password}&wlan_user_ip={local_ip}&wlan_user_ipv6=&wlan_user_mac=000000000000&wlan_ac_ip=&wlan_ac_name=SPL_NetEngine8000F8&jsVersion=3.3.2&v=3080",
   "check_interval": 5,
   "reconnect_delay": 3,
   "max_retry": 3
}
```

**占位符说明**（仅用于 `login_request_url_template`）：
- `{username}`：学号/一卡通号
- `{password}`：密码
- `{local_ip}`：本机 IP

**字段说明（全部）**

| 字段 | 说明 | 示例/默认值 |
|---|---|---|
| username | 学号/一卡通号 | "你的学号" |
| password | 密码 | "你的密码" |
| wifi_ssid | 需要连接的 WiFi 名称 | "SEU-WLAN" |
| login_ip | 校园网认证入口地址（用于判断是否已登录） | "http://202.119.25.2" |
| not_sign_in_title | 未认证时网页标题关键字 | "上网登录页" |
| result_return | 登录成功的返回字段关键字 | "\"result\":\"1\"" |
| signed_in_title | 已认证后网页标题关键字 | "注销页" |
| login_request_url_template | 登录请求 URL 模板（支持占位符） | 见上方示例 | 网络抓包获得
| check_interval | 轮询检查间隔（秒） | 5 |
| reconnect_delay | 断线后等待重连时间（秒） | 3 |
| max_retry | 单次循环最大重试次数 | 3 |
| auto_start_service | 启动后是否自动开始服务 | false |
| start_minimized | 启动后是否最小化到托盘 | false |

### 2、导入配置文件并启动

---

## 如何获取配置参数（抓包过程）

下面以浏览器 + 抓包工具为例，获取 `login_request_url_template` 及相关参数：

1) **准备抓包工具**
- 推荐：Fiddler / Charles / mitmproxy / Wireshark
- 若用 HTTPS 抓包工具，请先安装并信任其根证书

2) **开始抓包并触发登录**
- 断开网络后连接校园网（如 SEU-WLAN）
- 打开浏览器访问任意网页，跳转到校园网登录页
- 输入账号密码并点击登录

3) **定位登录请求**
- 在抓包列表中找到登录接口请求（通常包含 `eportal` 或 `Portal` 关键字）
- 重点查看请求的 **完整 URL**（包含 query 参数）

4) **抽取 URL 作为模板**
- 将 URL 中的账号、密码、本机 IP 替换为占位符：
   - `user_account` 中的账号 → `{username}`
   - `user_password` 中的密码 → `{password}`
   - `wlan_user_ip` → `{local_ip}`
- 其余参数保持不变，得到 `login_request_url_template`

5) **补齐其他字段（可选）**
- `login_ip`：登录页入口地址，一般为自动跳转页面的域名或 IP
- `not_sign_in_title` / `signed_in_title`：登录前/后的页面标题关键字
- `result_return`：登录成功返回内容中的关键字（可在登录请求响应中查找）

> 提示：不同校区、不同运营商或系统升级可能导致参数变化，请以实际抓包为准。
