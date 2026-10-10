using Demo.Common;
var actual = PriceCalculator.Total(10m, 3);
Console.WriteLine($"10 x 3: expected 30, actual {actual}");
if (actual != 30m) { Console.Error.WriteLine("FAIL: public library ignores quantity"); return 1; }
Console.WriteLine("PASS: bug fixed after dependency upgrade");
return 0;
