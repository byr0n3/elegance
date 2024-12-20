namespace Elegance.Enums
{
	public sealed partial class EnumGenerator
	{
		private static void AppendEnumParserStatement(IndentedSourceBuilder builder,
													  DeclaredEnum @enum,
													  System.Func<DeclaredEnum, DeclaredEnum.Field, string> lhs,
													  System.Func<DeclaredEnum, DeclaredEnum.Field, string> rhs,
													  string @default = "default")
		{
			builder.AppendLine("return (value) switch {");
			builder.AppendPush();
			{
				foreach (var field in @enum.Fields)
				{
					builder.AppendLine($"{lhs(@enum, field)} => {rhs(@enum, field)},");
				}

				builder.AppendLine($"_ => {@default},");
			}
			builder.AppendPop();
			builder.AppendLine("};");
		}
	}
}
