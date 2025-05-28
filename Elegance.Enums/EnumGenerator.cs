using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Elegance.Enums.Extensions;
using Elegance.Enums.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Elegance.Enums
{
	[Generator]
	public sealed partial class EnumGenerator : IIncrementalGenerator
	{
		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			context.RegisterPostInitializationOutput(EnumSources.Register);

			var enums = context.SyntaxProvider.ForAttributeWithMetadataName(
				EnumSources.EnumAttributeFullName,
				static (_, _) => true,
				EnumGenerator.GetEnumDeclaration
			);

			context.RegisterSourceOutput(enums.Collect(), EnumGenerator.Generate);
		}

		private static void Generate(SourceProductionContext context, ImmutableArray<DeclaredEnum> enums)
		{
			if (enums.Length == 0)
			{
				return;
			}

			var builder = new StringBuilder();

			foreach (var @enum in enums)
			{
				var defaultStrValue = @enum.Fields.Count == 0 ? "string.Empty" : $"\"{@enum.Fields[0].Value}\"";

				builder.Clear();

				builder.AppendLine(
					// language=csharp
					$"""
					 using System.Collections.Frozen;
					 using System.Collections.Generic;
					 using System.Diagnostics.CodeAnalysis;
					 using System.Text.Json;
					 using System.Text.Json.Serialization;

					 namespace {@enum.Namespace};
					 """
				);

				EnumGenerator.AppendEnumData(builder, @enum, defaultStrValue);

				context.AddSource($"{@enum.Name}.EnumData.g.cs", builder.ToString());

				EnumGenerator.AppendJsonConverter(builder, @enum);

				context.AddSource($"{@enum.Name}.JsonConverter.g.cs", builder.ToString());
			}
		}

		private static DeclaredEnum GetEnumDeclaration(GeneratorAttributeSyntaxContext context, CancellationToken token)
		{
			if (context.TargetSymbol.ContainingNamespace is null)
			{
				throw new System.Exception($"Symbol {context.TargetSymbol.Name} has no containing namespace.");
			}

			var name = context.TargetSymbol.Name;
			var @namespace = context.TargetSymbol.ContainingNamespace.ToDisplayString();
			var fields = EnumGenerator.GetEnumFields((EnumDeclarationSyntax)context.TargetNode, context.SemanticModel);

			return new DeclaredEnum
			{
				Name = name,
				Namespace = @namespace,
				Fields = fields,
			};
		}

		private static List<DeclaredEnum.Field> GetEnumFields(EnumDeclarationSyntax node, SemanticModel model)
		{
			var fields = new List<DeclaredEnum.Field>(node.Members.Count);

			foreach (var member in node.Members)
			{
				var label = member.Identifier.ValueText;
				var value = GetEnumValue(member, model);

				if (value is null)
				{
					continue;
				}

				fields.Add(new DeclaredEnum.Field
				{
					Label = label,
					Value = value,
				});
			}

			return fields;

			static string? GetEnumValue(EnumMemberDeclarationSyntax member, SemanticModel model)
			{
				var attr = member.FindAttribute(EnumSources.EnumValueAttributeName);

				if ((attr?.ArgumentList is null) || (attr.ArgumentList.Arguments.Count == 0))
				{
					return null;
				}

				return model.GetConstantValue(attr.ArgumentList.Arguments[0].Expression).ToString();
			}
		}

		private readonly struct DeclaredEnum
		{
			public required string Name { get; init; }

			public required string Namespace { get; init; }

			public required List<Field> Fields { get; init; }

			public string FullName =>
				$"{this.Namespace}.{this.Name}";

			public readonly struct Field
			{
				public required string Label { get; init; }

				public required string Value { get; init; }
			}
		}
	}
}
