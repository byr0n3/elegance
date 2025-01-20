using Elegance.Localization;
using Elegance.Utilities;

var now = System.DateTime.Now;

System.Console.WriteLine(AppLocalization.Get("yes"));
System.Console.WriteLine(AppLocalization.Get("no"));
System.Console.WriteLine(AppLocalization.Get("greeting", "John"));
System.Console.WriteLine(AppLocalization.Get("date", now));
System.Console.WriteLine(AppLocalization.Get(Culture.NL, "date", now));
System.Console.WriteLine(AppLocalization.Get("year", now));
System.Console.WriteLine(TestLocalization.Get("test"));
