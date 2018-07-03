# 上科大 Why-Fi Surfer Win 登录器

## 简介

看起来很像是 Mac 版登录器 https://github.com/TomJinW/WhyFi_Surfer_Mac 的移植，但其实是完全用 C# 重写了一遍的东西。


## 主要功能
- 支持 Windows 7 以及更新操作系统的 Windows 的电脑。
- 支持右下方图标常驻。支持 Balloon Tip/操作中心 显示登录状态。
- 与学校登录认证相同的结果反馈。
- 支持记住密码（Windows 版使用凭据管理器保存密码）。
- 支持简体中文。

## 其他
- 欢迎反馈任何 BUG，因时间关系我可能无法及时修补。但是还是会努力的。
- 需要开机自动启动的话，可以自行在系统偏好设置中设定。
- 由于自己没多少时间，这个东西匆匆忙忙就上线了，请多谅解。

## 使用的第三方库
- AdysTech.CredentialManager
- Newtonsoft.Json
- WPFTaskDialog