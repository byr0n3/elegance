using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Elegance.Utilities
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential)]
	public ref struct SplitEnumerator
	{
		private System.ReadOnlySpan<char> span;
		private readonly char separator;

		public System.ReadOnlySpan<char> Current { get; private set; }

		public readonly int Count =>
			this.span.IsEmpty ? default : (System.MemoryExtensions.Count(this.span, this.separator) + 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SplitEnumerator(System.ReadOnlySpan<char> span, char separator)
		{
			this.span = span;
			this.separator = separator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SplitEnumerator GetEnumerator() =>
			this;

		public bool MoveNext()
		{
			if (this.span.IsEmpty)
			{
				return false;
			}

			var index = System.MemoryExtensions.IndexOf(this.span, this.separator);

			// No more separators in the span, this is the last part
			if (index == -1)
			{
				this.Current = this.span;
				this.span = default;
			}
			else
			{
				this.Current = this.span.Slice(0, index);
				this.span = this.span.Slice(index + 1);
			}

			return true;
		}
	}
}
