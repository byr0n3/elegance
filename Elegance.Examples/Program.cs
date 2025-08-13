using System.Text.Json;
using Elegance.Examples;

var person = new Person
{
	Name = "John Doe",
	Gender = Gender.Male,
};

var json = JsonSerializer.Serialize(person, AppJsonSerializerContext.Default.Person!);

System.Console.WriteLine(json);
