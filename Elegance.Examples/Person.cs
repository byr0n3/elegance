using System.Text.Json.Serialization.Metadata;
using Elegance.Json;

namespace Elegance.Examples
{
	public readonly struct Person : IJsonSerializable<Person>
	{
		public static JsonTypeInfo<Person> TypeInfo =>
			AppJsonSerializerContext.Default.Person!;

		public required string Name { get; init; }

		public required Gender Gender { get; init; }
	}
}
