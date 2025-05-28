using System.Text;

namespace Elegance.Enums
{
	public sealed partial class EnumGenerator
	{
		private static string GetEnumParser(DeclaredEnum @enum,
											System.Func<DeclaredEnum, DeclaredEnum.Field, string> lhs,
											System.Func<DeclaredEnum, DeclaredEnum.Field, string> rhs,
											string @default = "default")
		{
			var builder = new StringBuilder();

			builder.AppendLine("return (value) switch {");
			{
				foreach (var field in @enum.Fields)
				{
					builder.AppendLine($"{lhs(@enum, field)} => {rhs(@enum, field)},");
				}

				builder.AppendLine($"_ => {@default},");
			}
			builder.AppendLine("};");

			return builder.ToString();
		}

		private static string GetEnumSpanParser(DeclaredEnum @enum,
												System.Func<DeclaredEnum, DeclaredEnum.Field, string> lhs,
												System.Func<DeclaredEnum, DeclaredEnum.Field, string> rhs,
												string @default = "default")
		{
			var builder = new StringBuilder();

			foreach (var field in @enum.Fields)
			{
				builder.AppendLine($$"""
									 if (System.MemoryExtensions.SequenceEqual({{lhs(@enum, field)}}, value)) {
									     return {{rhs(@enum, field)}};
									 }

									 """);
			}

			builder.AppendLine($"return {@default};");

			return builder.ToString();
		}
	}
}
