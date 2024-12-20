using System.Text.Json.Serialization;

namespace Elegance.Examples
{
	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower
	)]
	[JsonSerializable(typeof(Person))]
	internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
}
