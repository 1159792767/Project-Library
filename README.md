# 外网公共库更新演示
项目通过真正的 NuGet PackageReference 引用公共库，不复制公共库源码。

## 体验步骤
1. Actions → Test application：初始 1.0.0 会显示预期的失败，10 × 3 错误返回 10。
2. 公共库发布 1.0.1 后，在 Actions → Check public library updates → Run workflow 扫描，或等待每小时扫描（GitHub 可能延迟）。
3. Pull requests 出现 Update Demo.Common to 1.0.1，查看版本差异和检测工作流中通过的测试。
4. 合并该 PR 后，主分支测试变绿。
5. 本地安装 .NET 10 SDK 后执行 `dotnet run`，会恢复公网包并输出 PASS。

## GitHub 一次性设置
Settings → Actions → General → Workflow permissions：允许 GitHub Actions 创建和批准 pull requests。仓库策略可能禁止此开关，需要仓库所有者配置。
Watch → All Activity，并在个人 Settings → Notifications 开启需要的网页/邮件通知。PR 出现不代表邮件一定送达；需用户确认实际收件。

## 边界
本例采用 GitHub Actions + 简单检测脚本，不是 Renovate。只支持本例的三段稳定版本号、不自动合并。机器人 PR 的工作流可能不被触发或要求仓库所有者批准。本次独立 PR 测试显示 action_required，因此检测作业在创建 PR 前直接执行更新后测试；合并后的 push 再次执行测试。
包通过公开 GitHub Raw 托管，仅供实验；企业部署应换为外网可访问且需认证的 NuGet 服务。

