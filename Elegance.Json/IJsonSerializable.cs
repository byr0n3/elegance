using System.Text.Json.Serialization.Metadata;

namespace Elegance.Json
{
	public interface IJsonSerializable<T> where T : IJsonSerializable<T>
	{
		public static abstract JsonTypeInfo<T> TypeInfo { get; }
	}
}
