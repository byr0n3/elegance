using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elegance.Enums;

namespace Elegance.Examples
{
	[Enum]
	[JsonConverter(typeof(JsonGenderConverter))]
	public enum Gender
	{
		[EnumValue("unspecified")] Unspecified,

		[EnumValue("male")] Male,

		[EnumValue("female")] Female,
	}

	// The `System.Net.Json` system currently seems to have problems using Source Generated JsonConverters.
	// This is a temporary fix.
	public sealed class JsonGenderConverter : JsonConverterFactory
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool CanConvert(System.Type _) =>
			true;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override JsonConverter CreateConverter(System.Type _, JsonSerializerOptions __) =>
			new JsonGenderConverterFactory.Converter();
	}
}
