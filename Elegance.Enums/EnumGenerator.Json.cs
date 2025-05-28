using System.Linq;
using System.Text;

namespace Elegance.Enums
{
	public sealed partial class EnumGenerator
	{
		private static void AppendJsonConverter(StringBuilder builder, DeclaredEnum @enum)
		{
			var read = EnumGenerator.GetEnumSpanParser(
				@enum,
				static (_, field) => $"\"{field.Value}\"u8",
				static (@enum, field) => $"{@enum.Name}.{field.Label}"
			);

			var write = EnumGenerator.GetEnumParser(
				@enum,
				static (@enum, field) => $"{@enum.Name}.{field.Label}",
				static (_, field) => $"\"\\\"{field.Value}\\\"\"u8"
			);

			builder.Clear();

			var valueMaxLength = @enum.Fields.Max(static (f) => f.Value.Length);

			builder.Append(
				// language=csharp
				$$"""
				  #nullable enable

				  using System.Text.Json;
				  using System.Text.Json.Serialization;

				  namespace {{@enum.Namespace}};

				  public sealed class Json{{@enum.Name}}Converter : JsonConverter<{{@enum.Name}}>
				  {
				  		public override {{@enum.Name}} Read(ref Utf8JsonReader reader, System.Type _, JsonSerializerOptions __)
				  		{
				  			if (reader.TokenType != JsonTokenType.String)
				  			{
				  				return default;
				  			}

				  			System.Span<byte> buffer = stackalloc byte[{{valueMaxLength}}];

				  			var read = reader.CopyString(buffer);

				  			var value = buffer.Slice(0, read);

				  			{{read}}
				  		}

				  		public override void Write(Utf8JsonWriter writer, {{@enum.Name}} value, JsonSerializerOptions _)
				  		{
				  			writer.WriteRawValue(GetWriteValue(value));
				  		}

				  		private static System.ReadOnlySpan<byte> GetWriteValue({{@enum.Name}} value)
				  		{
				  			{{write}}
				  		}
				  }
				  """
			);
		}
	}
}
