using Demo.Updates;
using System.Diagnostics;
internal sealed partial class MainWindow {
 async Task Install() {
  if(url is null || version is null || hash is null)return;
  if(MessageBox.Show("将下载完整新版程序并重启。当前目录文件保留，可用于回退。","确认更新",MessageBoxButtons.OKCancel)!=DialogResult.OK)return;
  install.Enabled=false;check.Enabled=false;
  try {
   var root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PublicLibraryDemo","versions",version+"-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
   var zip=Path.Combine(root,"update.zip");
   using(var response=await Http.GetAsync(url,HttpCompletionOption.ResponseHeadersRead)) {
    response.EnsureSuccessStatusCode();var total=response.Content.Headers.ContentLength;
    await using var input=await response.Content.ReadAsStreamAsync();await using var output=File.Create(zip);
    var buffer=new byte[81920];long count=0;int n;
    while((n=await input.ReadAsync(buffer))>0) {await output.WriteAsync(buffer.AsMemory(0,n));count+=n;status.Text=$"正在下载：{count/1048576.0:F1} MB / {(total/1048576.0)?.ToString("F1") ?? "?"} MB";}
   }
   status.Text="正在校验并解压…";await Task.Run(()=>{UpdateCore.VerifyHash(zip,hash);UpdateCore.ExtractSafely(zip,Path.Combine(root,"app"));});
   var exe=Path.Combine(root,"app","Demo.App.exe");if(!File.Exists(exe))throw new InvalidDataException("包中没有程序入口");
   using(var verify=Process.Start(new ProcessStartInfo(exe,"--self-test") {UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(exe)!})) {
    if(verify is null)throw new IOException("无法验证新版程序");using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(30));
    await verify.WaitForExitAsync(timeout.Token);
    if(verify.ExitCode!=0)throw new InvalidDataException("新版程序自检失败，已保留旧版");
   }
   MessageBox.Show("新版已安装到：\n"+Path.GetDirectoryName(exe)+"\n将启动新版。原目录仍保留旧版。","更新完成");
   Process.Start(new ProcessStartInfo(exe) {UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(exe)!});Application.Exit();
  }catch(Exception e){status.Text="更新未完成，旧版仍可使用："+e.Message;check.Enabled=true;install.Enabled=true;}
 }
}
