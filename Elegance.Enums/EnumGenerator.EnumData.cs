namespace Elegance.Enums
{
	public sealed partial class EnumGenerator
	{
		private static void AppendEnumData(IndentedSourceBuilder builder, DeclaredEnum @enum, string defaultStrValue)
		{
			builder.AppendLine($"public static class {@enum.Name}EnumData {{");
			builder.AppendPush();
			{
				builder.AppendLine($"public static {@enum.Name} FromValue(string value) {{");
				builder.AppendPush();
				{
					EnumGenerator.AppendEnumParserStatement(
						builder,
						@enum,
						static (_, field) => $"\"{field.Value}\"",
						static (@enum, field) => $"{@enum.Name}.{field.Label}"
					);
				}
				builder.AppendPop();
				builder.AppendLine("}");

				builder.AppendLine($"public static string GetValue({@enum.Name} value) {{");
				builder.AppendPush();
				{
					EnumGenerator.AppendEnumParserStatement(
						builder,
						@enum,
						static (@enum, field) => $"{@enum.Name}.{field.Label}",
						static (_, field) => $"\"{field.Value}\"",
						defaultStrValue
					);
				}
				builder.AppendPop();
				builder.AppendLine("}");
			}
			builder.AppendPop();
			builder.AppendLine("}");
		}
	}
}
