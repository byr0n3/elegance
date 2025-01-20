using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.CodeAnalysis;

namespace Elegance.Icons
{
	[Generator]
	public sealed class IconsGenerator : IIncrementalGenerator
	{
		private static readonly XmlReaderSettings readerSettings = new()
		{
			Async = true,
			IgnoreComments = true,
			IgnoreWhitespace = true,
		};

		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			var icons = context.AdditionalTextsProvider
							   .Where(static (file) => file.Path.EndsWith(".svg", System.StringComparison.Ordinal))
							   .Select(static (file, token) => new IconFile
								{
									Name = IconsGenerator.NormalizeName(Path.GetFileNameWithoutExtension(file.Path)),
									Content = file.GetText(token)?.ToString(),
								});

			context.RegisterSourceOutput(icons.Collect(), IconsGenerator.Generate);
		}

		private static void Generate(SourceProductionContext context, ImmutableArray<IconFile> icons)
		{
			if (icons.IsEmpty)
			{
				return;
			}

			var builder = new IndentedSourceBuilder();

			builder.AppendLine(
				// language=csharp
				"""
				using Microsoft.AspNetCore.Components;
				using System.Diagnostics.CodeAnalysis;

				namespace Elegance.Icons;

				internal static class Icons {
				"""
			);

			builder.AppendPush();
			{
				foreach (var icon in icons)
				{
					IconsGenerator.AppendIcon(builder, icon);
				}
			}
			builder.AppendPop('}');

			context.AddSource("Icons.g.cs", builder.ToString());
		}

		private static void AppendIcon(IndentedSourceBuilder builder, IconFile icon)
		{
			if (icon.Content is null)
			{
				return;
			}

			using (var stream = new StringReader(icon.Content))
			using (var reader = XmlReader.Create(stream, IconsGenerator.readerSettings))
			{
				reader.Read();

				if (!Element.TryRead(reader, out var element))
				{
					return;
				}

				builder.AppendLine($"[NotNull] public static readonly RenderFragment {icon.Name} = static (__builder) => {{");
				builder.AppendPush();
				{
					element.Append(builder);
				}
				builder.AppendPopLine("};");
			}
		}

		private static string NormalizeName(System.ReadOnlySpan<char> name)
		{
			System.Span<char> buffer = stackalloc char[name.Length];

			// Append the name and replace the first character with the uppercase variant of said character,
			// as most, if not all, icon names are in snakeCase
			var copied = name.TryCopyTo(buffer);
			Debug.Assert(copied);

			buffer[0] = char.ToUpper(name[0]);

			int idx;
			var written = name.Length;

			// Remove all instances of `-` and transform into snakeCase
			while ((idx = System.MemoryExtensions.IndexOf(buffer, '-')) != -1)
			{
				buffer.Slice(idx + 1).TryCopyTo(buffer.Slice(idx));
				buffer[idx] = char.ToUpper(buffer[idx]);
				written--;
			}

			return new string(buffer.Slice(0, written));
		}

		private readonly struct IconFile
		{
			public required string Name { get; init; }

			public required string? Content { get; init; }
		}
	}
}
