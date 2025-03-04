using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Elegance.Utilities
{
	[PublicAPI]
	public ref struct CharSpanBuilder
	{
		private readonly System.Span<char> buffer;
		private int position;

		public readonly System.ReadOnlySpan<char> Value =>
			this.buffer.Slice(0, this.position);

		[System.Obsolete("Apply a Span<char> using the constructor arguments", true)]
		public CharSpanBuilder() =>
			throw new System.NotSupportedException();

		public CharSpanBuilder(System.Span<char> buffer)
		{
			this.buffer = buffer;
			this.position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly void AssertAvailable(int size) =>
			Debug.Assert(this.position + size < this.buffer.Length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly System.Span<char> Take() =>
			this.buffer.Slice(this.position);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly System.Span<char> Take(int size) =>
			this.buffer.Slice(this.position, size);

		/// <summary>
		/// Moves the <see cref="position"/> based on <paramref name="length"/>.
		/// </summary>
		/// <param name="length">The length to add to <see cref="position"/>.</param>
		/// <remarks>
		/// Keep in mind that this will affect the output of the builder.
		/// Manually moving the position forward isn't needed nor recommended,
		/// as it's unlikely to be data in that part of the buffer.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Move(int length) =>
			this.position += length;

		public void Append(char value)
		{
			this.AssertAvailable(1);

			this.buffer[this.position++] = value;
		}

		public void Append(System.ReadOnlySpan<char> value)
		{
			this.AssertAvailable(value.Length);

			var copied = value.TryCopyTo(this.Take(value.Length));

			if (copied)
			{
				this.position += value.Length;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(string value) =>
			this.Append(System.MemoryExtensions.AsSpan(value));

		public void Append<T>(T value, System.ReadOnlySpan<char> format = default, System.IFormatProvider? provider = null)
			where T : System.ISpanFormattable
		{
			// @todo Somehow get expected length?
			this.AssertAvailable(1);

			var copied = value.TryFormat(this.Take(), out var written, format, provider);

			if (copied)
			{
				this.position += written;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly override string ToString() =>
			new(this.Value);
	}
}
