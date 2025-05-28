using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elegance.Enums;

namespace Elegance.Examples
{
	[Enum]
	[JsonConverter(typeof(JsonGenderConverter2))]
	public enum Gender
	{
		[EnumValue("unspecified")] Unspecified,

		[EnumValue("male")] Male,

		[EnumValue("female")] Female,
	}

	// The `System.Net.Json` system currently seems to have problems using Source Generated JsonConverters.
	// This is a temporary fix.
	public sealed class JsonGenderConverter2 : JsonConverter<Gender>
	{
		private static readonly JsonGenderConverter converter = new();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Gender Read(ref Utf8JsonReader reader, System.Type type, JsonSerializerOptions options) =>
			JsonGenderConverter2.converter.Read(ref reader, type, options);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void Write(Utf8JsonWriter writer, Gender value, JsonSerializerOptions options) =>
			JsonGenderConverter2.converter.Write(writer, value, options);
	}
}
