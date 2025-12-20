using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Elegance.Enums.Extensions
{
	internal static class MemberDeclarationSyntaxExtensions
	{
		extension(MemberDeclarationSyntax @this)
		{
			public AttributeSyntax? FindAttribute(string name)
			{
				foreach (var attribute in @this.AttributeLists.SelectMany(static (list) => list.Attributes))
				{
					if (Cmp(attribute.Name, name))
					{
						return attribute;
					}
				}

				return null;

				static bool Cmp(NameSyntax name, string expected)
				{
					if ((name is SimpleNameSyntax simple) && (simple.Identifier.Text == expected))
					{
						return true;
					}

					return (name is QualifiedNameSyntax qualified) && (qualified.Right.Identifier.Text == expected);
				}
			}
		}
	}
}
