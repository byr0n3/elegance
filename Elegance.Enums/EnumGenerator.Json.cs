using System.Linq;

namespace Elegance.Enums
{
	public sealed partial class EnumGenerator
	{
		private static void AppendJsonConverter(IndentedSourceBuilder builder, DeclaredEnum @enum)
		{
			builder.Clear();

			EnumGenerator.AppendEnumParserStatement(
				builder,
				@enum,
				static (_, field) => $"\"{field.Value}\"",
				static (@enum, field) => $"{@enum.Name}.{field.Label}"
			);

			var read = builder.ToString();

			builder.Clear();

			EnumGenerator.AppendEnumParserStatement(
				builder,
				@enum,
				static (@enum, field) => $"{@enum.Name}.{field.Label}",
				static (_, field) => $"\"{field.Value}\""
			);

			var write = builder.ToString();

			builder.Clear();

			var valueMaxLength = @enum.Fields.Max(static (f) => f.Value.Length);

			builder.Append(
				// language=csharp
				$$"""
				  using System.Text.Json;
				  using System.Text.Json.Serialization;

				  namespace {{@enum.Namespace}};

				  #nullable enable
				  public sealed class Json{{@enum.Name}}ConverterFactory : JsonConverterFactory
				  {
				  	private static readonly JsonConverter converter = new Converter();
				  
				  	public Json{{@enum.Name}}ConverterFactory() : this (null, true) { }
				  
				  	public Json{{@enum.Name}}ConverterFactory(JsonNamingPolicy? _ = null, bool __ = true) { }
				  
				  	public override bool CanConvert(System.Type type) =>
				  		type == typeof({{@enum.Name}});
				  
				  	public override JsonConverter CreateConverter(System.Type _, JsonSerializerOptions __) =>
				  		Json{{@enum.Name}}ConverterFactory.converter;
				  
				  	public sealed class Converter : JsonConverter<{{@enum.Name}}>
				  	{
				  		public Converter() { }
				  
				  		public override {{@enum.Name}} Read(ref Utf8JsonReader reader, System.Type _, JsonSerializerOptions __)
				  		{
				  			if (reader.TokenType != JsonTokenType.String)
				  			{
				  				return default;
				  			}
				  
				  			System.Span<char> buffer = stackalloc char[{{valueMaxLength}}];
				  
				  			var read = reader.CopyString(buffer);
				  
				  			var value = buffer.Slice(0, read);
				  
				  			{{read}}
				  		}
				  
				  		public override void Write(Utf8JsonWriter writer, {{@enum.Name}} value, JsonSerializerOptions _)
				  		{
				  			writer.WriteRawValue(Converter.GetWriteValue(value));
				  		}
				  
				  		private static string GetWriteValue({{@enum.Name}} value)
				  		{
				  			{{write}}
				  		}
				  	}
				  }
				  """
			);
		}
	}
}
