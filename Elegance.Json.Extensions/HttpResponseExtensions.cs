using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Elegance.Json.Extensions
{
	[PublicAPI]
	public static class JsonExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task WriteJsonAsync<T>(this HttpResponse @this, T value, CancellationToken token = default)
			where T : IJsonSerializable<T>
		{
			JsonExtensions.SetHeaders(@this);

			return JsonSerializer.SerializeAsync(@this.BodyWriter, value, T.TypeInfo, token);
		}

		public static async Task WriteJsonAsync<T>(this HttpResponse @this,
												   IAsyncEnumerable<T> enumerable,
												   CancellationToken token = default)
			where T : IJsonSerializable<T>
		{
			JsonExtensions.SetHeaders(@this);

			var writer = new Utf8JsonWriter(@this.BodyWriter);

			await using (writer.ConfigureAwait(false))
			{
				writer.WriteStartArray();
				{
					await foreach (var item in enumerable.WithCancellation(token).ConfigureAwait(false))
					{
						JsonSerializer.Serialize(writer, item, T.TypeInfo);
					}
				}
				writer.WriteEndArray();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetHeaders(HttpResponse response)
		{
			response.StatusCode = StatusCodes.Status200OK;
			response.Headers.ContentType = MediaTypeNames.Application.Json;
		}
	}
}
