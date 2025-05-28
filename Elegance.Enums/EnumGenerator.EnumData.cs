using System.Text;

namespace Elegance.Enums
{
	public sealed partial class EnumGenerator
	{
		private static void AppendEnumData(StringBuilder builder, DeclaredEnum @enum, string defaultStrValue)
		{
			var stringValues = EnumGenerator.GetValuesDictionary(
				@enum,
				"Values",
				@enum.FullName,
				"string",
				(f) => $"{@enum.FullName}.{f.Label}",
				static (f) => $"\"{f.Value}\""
			);

			var charSpanParser = EnumGenerator.GetEnumSpanParser(
				@enum,
				static (_, field) => $"\"{field.Value}\"",
				static (@enum, field) => $"{@enum.Name}.{field.Label}"
			);

			var byteSpanParser = EnumGenerator.GetEnumSpanParser(
				@enum,
				static (_, field) => $"\"{field.Value}\"u8",
				static (@enum, field) => $"{@enum.Name}.{field.Label}"
			);

			var enumParser = EnumGenerator.GetEnumParser(
				@enum,
				static (@enum, field) => $"{@enum.Name}.{field.Label}",
				static (_, field) => $"\"{field.Value}\"",
				defaultStrValue
			);

			builder.AppendLine($$"""
								 	public static class {{@enum.Name}}EnumData {
								 		{{stringValues}}

								 		public static {{@enum.Name}} FromValue(System.ReadOnlySpan<char> value) {
								 			{{charSpanParser}}
								 		}

								 		public static {{@enum.Name}} FromValue(System.ReadOnlySpan<byte> value) {
								 			{{byteSpanParser}}
								 		}

								 		public static string GetValue({{@enum.Name}} value) {
								 			{{enumParser}}
								 		}
								 	}
								 """);
		}

		private static string GetValuesDictionary(DeclaredEnum @enum,
												  string name,
												  string keyType,
												  string valueType,
												  System.Func<DeclaredEnum.Field, string> key,
												  System.Func<DeclaredEnum.Field, string> value)
		{
			var builder = new StringBuilder();

			builder.AppendLine(
				$"public static readonly FrozenDictionary<{keyType}, {valueType}> {name} = new Dictionary<{keyType}, {valueType}> {{"
			);

			foreach (var field in @enum.Fields)
			{
				builder.AppendLine($$"""
									 			{ {{key(field)}}, {{value(field)}} },
									 """);
			}

			builder.AppendLine("\t\t}.ToFrozenDictionary();");

			return builder.ToString();
		}
	}
}
