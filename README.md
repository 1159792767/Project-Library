# Windows 程序内更新演示
## 下载体验
请到 Releases 下载 **Demo.App-1.1.0-win-x64.zip**，不是 Source code。解压整个文件夹，双击 Demo.App.exe。无需安装 .NET。
1. 点击计算总金额：旧版显示 10，预期 30。
2. 点击检查更新，再点下载更新并重启。
3. 下载经 SHA256 校验、解压并自检后，启动 1.1.1，计算结果变成 30。
更新安装到 %LOCALAPPDATA%/PublicLibraryDemo/versions 下的独立目录。以后运行提示的新路径；旧目录保留用于回退，原快捷方式不会自动改变。

## 源码调试
拉取 main 后使用支持 .NET 10 的 Visual Studio 打开 Demo.App.csproj。现在是 Windows Forms 界面，不再是控制台程序。

## 发布和验证
Actions 的 Publish desktop demo 手动构建并发布两个自包含 Windows x64 版本。两个 tag 用同一源提交，通过工作流分别选择公共库包和程序版本。旧版的计算测试应失败，新版应通过。Updater.Tests 覆盖降级、校验失败和 Zip 路径越界。

## 演示边界
需要能访问 GitHub Release 和 Release 下载服务，不需要公司 VPN。更新清单使用 Release 下载地址，不调用匿名 API。当前手动检查、用户确认更新，独立目录安装并重启；不是热替换 DLL。SHA256 来自同一 HTTPS Release 的更新清单，不是独立发行签名。正式发布还需签名、稳定启动入口、超时重试和数据迁移策略。公共库升级 PR 与桌面发布是两条流程，本次不自动合并依赖 PR。

程序版本 1.1.0 引用公共库 1.0.0；程序版本 1.1.1 引用公共库 1.0.1。早期 1.0.x 程序使用匿名 API 检查，可能限流，请下载 1.1.0 开始演示。
