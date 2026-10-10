using System.IO.Compression;
using System.Security.Cryptography;
namespace Demo.Updates;
public static class UpdateCore {
 public static Version ValidateVersion(string value) {
  if (!Version.TryParse(value,out var v) || v.Major<0 || v.Minor<0 || v.Build<0 || value.Any(c=>!char.IsAsciiDigit(c)&&c!='.')) throw new InvalidDataException("Invalid version");
  return v;
 }
 public static bool IsNewer(string candidate,string current)=>ValidateVersion(candidate)>ValidateVersion(current);
 public static void VerifyHash(string file,string expected) {
  var actual=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)));
  if(!string.Equals(actual,expected,StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("SHA256 mismatch");
 }
 public static void ExtractSafely(string zip,string directory) {
  Directory.CreateDirectory(directory); var root=Path.GetFullPath(directory)+Path.DirectorySeparatorChar;
  using var archive=ZipFile.OpenRead(zip);
  foreach(var entry in archive.Entries) {
   var path=Path.GetFullPath(Path.Combine(directory,entry.FullName));
   if(!path.StartsWith(root,StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unsafe archive path");
  }
  archive.ExtractToDirectory(directory,false);
 }
}
