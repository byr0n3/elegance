using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;
using JetBrains.Annotations;

namespace Elegance.Utilities
{
	[InterpolatedStringHandler]
	[StructLayout(LayoutKind.Sequential)]
	[UsedImplicitly(ImplicitUseTargetFlags.Members)]
	public ref struct InterpolatedStringBuilder
	{
		private readonly System.Span<byte> buffer;

		private int position;

		public readonly System.ReadOnlySpan<byte> Result
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.buffer.Slice(0, this.position);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public InterpolatedStringBuilder(System.Span<byte> buffer) =>
			this.buffer = buffer;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public InterpolatedStringBuilder(int _, int __, System.Span<byte> buffer) : this(buffer)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly System.Span<byte> Slice() =>
			this.buffer.Slice(this.position);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLiteral(string @string)
		{
			if (Utf8.FromUtf16(@string, this.Slice(), out _, out var written) == OperationStatus.Done)
			{
				this.position += written;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string @string) =>
			this.AppendLiteral(@string);

		public void AppendFormatted(scoped System.ReadOnlySpan<byte> bytes)
		{
			var length = bytes.Length;

			if (bytes[length - 1] is (byte)'\0')
			{
				bytes = bytes.Slice(0, length - 1);
			}

			if (bytes.TryCopyTo(this.Slice()))
			{
				this.position += bytes.Length;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, scoped System.ReadOnlySpan<char> format = default)
			where T : unmanaged, System.IUtf8SpanFormattable
		{
			if (value.TryFormat(this.Slice(), out var written, format, CultureInfo.InvariantCulture))
			{
				this.position += written;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly override unsafe string ToString()
		{
			var result = this.Result;

			fixed (byte* ptr = result)
			{
				return new string((sbyte*)ptr, 0, result.Length);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator System.ReadOnlySpan<byte>(InterpolatedStringBuilder builder) =>
			builder.Result;
	}
}
