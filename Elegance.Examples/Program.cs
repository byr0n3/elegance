using System.Text.Json;
using Elegance.Examples;
using Elegance.Localization;
using Elegance.Utilities;

var person = new Person
{
	Name = "John Doe",
	Gender = Gender.Male,
};

var json = JsonSerializer.Serialize(person, AppJsonSerializerContext.Default.Person!);

System.Console.WriteLine(json);

var now = System.DateTime.Now;

System.Console.WriteLine(AppLocalization.Get("yes"));
System.Console.WriteLine(AppLocalization.Get("no"));
System.Console.WriteLine(AppLocalization.Get("greeting", "John"));
System.Console.WriteLine(AppLocalization.Get("date", now));
System.Console.WriteLine(AppLocalization.Get(Culture.NL, "date", now));
System.Console.WriteLine(AppLocalization.Get("year", now));
System.Console.WriteLine(TestLocalization.Get("test"));
