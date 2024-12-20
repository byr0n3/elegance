using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Elegance.Enums.Sources
{
	internal static class EnumSources
	{
		public const string EnumAttributeFullName = "Elegance.Enums.EnumAttribute";
		public const string EnumValueAttributeName = "EnumValue";

		private const string enumAttribute =
			// language=csharp
			"""
			namespace Elegance.Enums
			{
				[System.AttributeUsage(System.AttributeTargets.Enum)]
				public sealed class EnumAttribute : System.Attribute;
			}
			""";

		private const string enumValueAttribute =
			// language=csharp
			"""
			namespace Elegance.Enums
			{
				[System.AttributeUsage(System.AttributeTargets.Field)]
				public sealed class EnumValueAttribute : System.Attribute
				{
					public readonly string Value;
			
					public EnumValueAttribute(string value) =>
						this.Value = value;
				}
			}
			""";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Register(IncrementalGeneratorPostInitializationContext context)
		{
			context.AddSource("Enum.g.cs", EnumSources.enumAttribute);
			context.AddSource("EnumValue.g.cs", EnumSources.enumValueAttribute);
		}
	}
}
