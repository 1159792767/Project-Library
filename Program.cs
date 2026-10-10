using Demo.Common;
using Team.DesktopUpdater;
using System.Reflection;
internal static class Program {
 [STAThread] static int Main(string[] args) {
  if(args.Contains("--self-test")) return PriceCalculator.Total(10m,3)==30m ? 0 : 1;
  ApplicationConfiguration.Initialize(); Application.Run(new MainWindow()); return 0;
 }
}
internal sealed class MainWindow : Form {
 readonly UpdateClient updater;
 public MainWindow(){
  var version=Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
  updater=new UpdateClient(new UpdateOptions("app-github-demo",version,"Demo.App.exe",
   new Uri("https://raw.githubusercontent.com/1159792767/Project-Library/updater-channel/app-github-demo.json"),
   "https://github.com/1159792767/Project-Library/releases/download/","--self-test"));
  Text="公共库更新与回退演示";Width=800;Height=540;MinimumSize=new(760,500);Font=new("Microsoft YaHei UI",11);StartPosition=FormStartPosition.CenterScreen;
  var panel=new FlowLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(24),FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true};Controls.Add(panel);
  panel.Controls.Add(new Label{Text="公共库更新与版本回退",AutoSize=true,Font=new(Font.FontFamily,20,FontStyle.Bold)});
  panel.Controls.Add(new Label{Text=$"程序 {version}  /  公共库 {typeof(PriceCalculator).Assembly.GetName().Version!.ToString(3)}",AutoSize=true});
  var price=new NumericUpDown{Value=10,DecimalPlaces=2,Maximum=100000,Width=180};var quantity=new NumericUpDown{Value=3,Maximum=10000,Width=180};
  panel.Controls.Add(new Label{Text="单价",AutoSize=true});panel.Controls.Add(price);panel.Controls.Add(new Label{Text="数量",AutoSize=true});panel.Controls.Add(quantity);
  var result=new Label{AutoSize=true};var calculate=new Button{Text="计算总金额",AutoSize=true};calculate.Click+=(_,_)=>result.Text=$"实际：{PriceCalculator.Total(price.Value,(int)quantity.Value)}    预期：{price.Value*quantity.Value}";panel.Controls.Add(calculate);panel.Controls.Add(result);
  var manage=new Button{Text="检查更新 / 版本管理与回退",AutoSize=true};manage.Click+=(_,_)=>UpdateDialog.Show(this,updater,Application.Exit);panel.Controls.Add(manage);
  var notification=new Label{Width=710,Height=90,Text="公共包发布后，GitHub Actions 自动构建本程序。启动时及每 15 分钟检查更新。\n请保留旧目录用于回退。"};panel.Controls.Add(notification);
  var path=new TextBox{Text=AppContext.BaseDirectory,ReadOnly=true,Width=710};panel.Controls.Add(path);
  Shown+=(_,_)=>{try{updater.RegisterCurrent();}catch(Exception e){MessageBox.Show("历史记录失败："+e.Message);manage.Enabled=false;}};
  bool checking=false;
  async void Check(){if(checking)return;checking=true;try{var update=await updater.CheckAsync();notification.Text=update is null?"当前已是最新版本。":$"发现新版本 {update.Version}：{update.Notes}\n点击“检查更新 / 版本管理与回退”安装。";}catch(Exception e){notification.Text="自动检查暂未成功，可稍后手动重试："+e.GetBaseException().Message;}finally{checking=false;}}
  Shown+=(_,_)=>Check();
  var timer=new System.Windows.Forms.Timer{Interval=15*60*1000};timer.Tick+=(_,_)=>Check();timer.Start();FormClosed+=(_,_)=>timer.Dispose();
 }
}
