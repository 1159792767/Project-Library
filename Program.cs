using Demo.Common;
using Demo.Updates;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
internal static class Program {
 [STAThread] static int Main(string[] args) {
  if(args.Contains("--self-test")) return PriceCalculator.Total(10m,3)==30m ? 0 : 1;
  ApplicationConfiguration.Initialize(); Application.Run(new MainWindow()); return 0;
 }
}
internal sealed partial class MainWindow : Form {
 const string Repo="https://api.github.com/repos/1159792767/Project-Library/releases/latest";
 static readonly HttpClient Http=new() { Timeout=TimeSpan.FromMinutes(10) };
 readonly Label status=new() { AutoSize=false, Height=80, Width=620 };
 readonly NumericUpDown price=new() { Value=10, Maximum=1000000, DecimalPlaces=2, Width=180 };
 readonly NumericUpDown quantity=new() { Value=3, Maximum=10000, Width=180 };
 readonly Button install=new() { Text="下载更新并重启", AutoSize=true, Enabled=false };
 readonly Button check=new() { Text="检查更新", AutoSize=true };
 string current=Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
 string? version,url,hash;
 public MainWindow() {
  Text="公共库升级演示"; Width=740;Height=540;MinimumSize=new(720,520);StartPosition=FormStartPosition.CenterScreen;Font=new("Microsoft YaHei UI",11);
  Http.DefaultRequestHeaders.UserAgent.ParseAdd("PublicLibraryUpdateDemo/1.0");
  var flow=new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(25),AutoScroll=true };Controls.Add(flow);
  flow.Controls.Add(new Label { Text="公共库计算与在线更新",Font=new(Font.FontFamily,20,FontStyle.Bold),AutoSize=true });
  flow.Controls.Add(new Label { Text=$"程序版本 {current}    公共库版本 {typeof(PriceCalculator).Assembly.GetName().Version!.ToString(3)}",AutoSize=true });
  flow.Controls.Add(new Label { Text="单价",AutoSize=true });flow.Controls.Add(price);
  flow.Controls.Add(new Label { Text="数量",AutoSize=true });flow.Controls.Add(quantity);
  var calc=new Button { Text="计算总金额",AutoSize=true };var result=new Label { AutoSize=true };
  calc.Click+=(_,_)=>result.Text=$"计算结果：{PriceCalculator.Total(price.Value,(int)quantity.Value)}    预期：{price.Value*quantity.Value}";
  flow.Controls.Add(calc);flow.Controls.Add(result); var buttons=new FlowLayoutPanel { Width=640,Height=45 };buttons.Controls.Add(check);buttons.Controls.Add(install);flow.Controls.Add(buttons);
  status.Text="点击检查更新。旧版 1.0.0 故意存在数量计算 Bug。";flow.Controls.Add(status);
  check.Click+=async(_,_)=>await Check(); install.Click+=async(_,_)=>await Install();
 }
 async Task Check() {
  check.Enabled=false;install.Enabled=false;status.Text="正在连接 GitHub 检查版本…";
  try {
   using var json=JsonDocument.Parse(await Http.GetStringAsync(Repo));var r=json.RootElement;
   var candidate=r.GetProperty("tag_name").GetString()!.TrimStart('v');
   if(!UpdateCore.IsNewer(candidate,current)) {status.Text="已是最新版本。";return;}
   string? download=null,digest=null;
   foreach(var a in r.GetProperty("assets").EnumerateArray()) if(a.GetProperty("name").GetString()=="Demo.App-"+candidate+"-win-x64.zip") {
    download=a.GetProperty("browser_download_url").GetString();
    if(a.TryGetProperty("digest",out var dg)) digest=dg.GetString();
   }
   if(download is null || digest is null || !digest.StartsWith("sha256:")) throw new InvalidDataException("发布包或 SHA256 校验信息缺失");
   if(!download.StartsWith("https://github.com/1159792767/Project-Library/releases/download/",StringComparison.Ordinal)) throw new InvalidDataException("不受信任的下载地址");
   version=candidate;url=download;hash=digest[7..];install.Enabled=true;
   status.Text=$"发现新版 {candidate}：修复公共库数量计算问题。点击下载更新并重启。";
  } catch(Exception e) {status.Text="检查失败："+e.Message+"。可稍后重试。";} finally {check.Enabled=true;}
 }
}
