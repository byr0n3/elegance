using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;

namespace Elegance.Localization
{
	// @todo Don't use `string.Format`.
	[Generator]
	public sealed class LocalizationGenerator : IIncrementalGenerator
	{
		private const string fileSuffix = ".lang.json";
		private const string defaultLocale = "en";

		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			var localizationFiles = context.AdditionalTextsProvider
										   .Where(
												static (file) =>
													file.Path.EndsWith(LocalizationGenerator.fileSuffix, System.StringComparison.Ordinal)
											)
										   .Select(static (file, token) =>
											{
												var (name, locale) = LocalizationGenerator.GetLocalizationNameAndLocale(file.Path);

												return new LocalizationFile
												{
													Name = name,
													Locale = locale ?? LocalizationGenerator.defaultLocale,
													Content = file.GetText(token)?.ToString(),
												};
											});

			context.RegisterSourceOutput(localizationFiles.Collect(), LocalizationGenerator.Generate);
		}

		private static void Generate(SourceProductionContext context, ImmutableArray<LocalizationFile> localizationFiles)
		{
			if (localizationFiles.IsEmpty)
			{
				return;
			}

			var builder = new StringBuilder();

			foreach (var localization in localizationFiles)
			{
				builder.Clear();

				builder.AppendLine(
					// language=csharp
					$$"""
					  #nullable enable
					  using System.Diagnostics.CodeAnalysis;
					  using System.Collections.Generic;
					  using System.Collections.Frozen;
					  using System.Globalization;
					  using System.Runtime.CompilerServices;
					  using Microsoft.AspNetCore.Components;

					  namespace Elegance.Localization;

					  internal static class {{localization.Name}}Localization {
					  
					  	private static readonly FrozenDictionary<string, string> lang = new Dictionary<string, string>(System.StringComparer.Ordinal)
					  	{
					  """
				);

				LocalizationGenerator.Append(builder, localization);

				builder.AppendLine(
					// language=csharp
					"""
						}.ToFrozenDictionary();
					
						/// <summary>
						/// Transform the given <paramref name="key"/> into a localized text.
						/// </summary>
						/// <param name="key">The key of the localized text to find.</param>
						/// <returns>The localized text, or <paramref name="key"/> if no localized text was found.</returns>
						[MethodImpl(MethodImplOptions.AggressiveInlining)]
						public static string Get(string key) =>
							lang.TryGetValue(key, out var value) ? value : key;
					
						/// <summary>
						/// Transform the given <paramref name="key"/> into a localized text, using <paramref name="args"/> to format the text.
						/// </summary>
						/// <param name="key">The key of the localized text to find.</param>
						/// <param name="args">The arguments to use when formatting the text.</param>
						/// <returns>The transformed localized text, or <paramref name="key"/> if no localized text was found.</returns>
						[MethodImpl(MethodImplOptions.AggressiveInlining)]
						public static string Get(string key, params System.ReadOnlySpan<object?> args) =>
							Get(CultureInfo.InvariantCulture, key, args);
					
						/// <summary>
						/// Transform the given <paramref name="key"/> into a localized text, using <paramref name="args"/> to format the text.
						/// </summary>
						/// <param name="culture">The <see cref="CultureInfo"/> to use when formatting the found text.</param>
						/// <param name="key">The key of the localized text to find.</param>
						/// <param name="args">The arguments to use when formatting the text.</param>
						/// <returns>The transformed localized text, or <paramref name="key"/> if no localized text was found.</returns>
						[MethodImpl(MethodImplOptions.AggressiveInlining)]
						public static string Get(CultureInfo culture, string key, params System.ReadOnlySpan<object?> args) =>
							lang.TryGetValue(key, out var value) ? 
								string.Format(culture, System.Text.CompositeFormat.Parse(value), args) :
								key;
					}
					"""
				);

				context.AddSource($"{localization.Name}Localization.g.cs", builder.ToString());
			}
		}

		private static void Append(StringBuilder builder, LocalizationFile localization)
		{
			if (localization.Content is null)
			{
				return;
			}

			var entries = JsonSerializer.Deserialize(localization.Content, LocalizationJsonContext.Default.DictionaryStringString);

			if (entries is null)
			{
				return;
			}

			foreach (var (key, value) in entries)
			{
				builder.AppendLine($"{{ \"{key}\", \"{value}\" }},");
			}
		}

		private static (string, string?) GetLocalizationNameAndLocale(System.ReadOnlySpan<char> path)
		{
			var idx = System.MemoryExtensions.LastIndexOf(path, '/');

			if (idx != -1)
			{
				path = path.Slice(idx + 1);
			}

			// Path is now the name of the localization file + possibly the locale.
			path = path.Slice(0, path.Length - LocalizationGenerator.fileSuffix.Length);

			string? locale = null;
			var localeOffset = System.MemoryExtensions.IndexOf(path, '.');

			if (localeOffset != -1)
			{
				locale = new string(path.Slice(localeOffset + 1));
				path = path.Slice(0, localeOffset);
			}

			System.Span<char> name = stackalloc char[path.Length];

			path.TryCopyTo(name);

			name[0] = char.ToUpperInvariant(name[0]);

			return (new string(name), locale);
		}

		private readonly struct LocalizationFile
		{
			public required string Name { get; init; }

			public required string Locale { get; init; }

			public required string? Content { get; init; }
		}
	}

	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower
	)]
	[JsonSerializable(typeof(Dictionary<string, string>))]
	internal sealed partial class LocalizationJsonContext : JsonSerializerContext;
}
