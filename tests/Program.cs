using Demo.Updates;
using System.IO.Compression;
using System.Security.Cryptography;
int checks=0;
void Check(bool value,string name) { if(!value) throw new Exception(name); Console.WriteLine("PASS " + name); checks++; }
void Reject(Action action,string name) { try { action(); } catch(InvalidDataException) { Check(true,name); return; } throw new Exception("Accepted " + name); }
Check(UpdateCore.IsNewer("1.0.1","1.0.0"), "newer version");
Check(!UpdateCore.IsNewer("1.0.0","1.0.1"), "no downgrade");
Check(!UpdateCore.IsNewer("1.0.1","1.0.1"), "no equal update");
var root=Path.Combine(Path.GetTempPath(),"updater-test-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
try {
 var zip=Path.Combine(root,"good.zip");
 using(var a=ZipFile.Open(zip,ZipArchiveMode.Create)) { using var w=new StreamWriter(a.CreateEntry("Demo.App.exe").Open());w.Write("demo"); }
 var hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(zip)));
 UpdateCore.VerifyHash(zip,hash); Check(true,"correct hash");
 Reject(()=>UpdateCore.VerifyHash(zip,new string('0',64)),"corrupt download rejected");
 UpdateCore.ExtractSafely(zip,Path.Combine(root,"good")); Check(File.Exists(Path.Combine(root,"good","Demo.App.exe")),"valid package extracted");
 var bad=Path.Combine(root,"bad.zip");
 using(var a=ZipFile.Open(bad,ZipArchiveMode.Create)) { using var w=new StreamWriter(a.CreateEntry("../escaped.txt").Open());w.Write("bad"); }
 Reject(()=>UpdateCore.ExtractSafely(bad,Path.Combine(root,"bad")),"zip traversal rejected");
 Check(!File.Exists(Path.Combine(root,"escaped.txt")),"no file escaped");
 Reject(()=>UpdateCore.ValidateVersion("../../bad"),"invalid version rejected");
} finally { Directory.Delete(root,true); }
Console.WriteLine($"{checks} checks passed");
