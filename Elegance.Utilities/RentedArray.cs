using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Elegance.Utilities
{
	[MustDisposeResource]
	internal readonly struct RentedArray<T> : System.IDisposable
	{
		private readonly T[]? array;

		private bool Valid
		{
			[MemberNotNullWhen(true, nameof(this.array))]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this.array is not null;
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				Debug.Assert(this.Valid);

				return this.array[index];
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				Debug.Assert(this.Valid);

				this.array[index] = value;
			}
		}

		[System.Obsolete("Use a constructor with arguments instead.", true)]
		public RentedArray()
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RentedArray(int length) =>
			this.array = ArrayPool<T>.Shared.Rent(length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public System.Span<T> Slice(int start = 0, int length = 0)
		{
			Debug.Assert(this.Valid);

			if (length <= 0)
			{
				length = this.array.Length;
			}

			return new System.Span<T>(this.array, start, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public System.Memory<T> AsMemory(int start = 0, int length = 0)
		{
			Debug.Assert(this.Valid);

			if (length <= 0)
			{
				length = this.array.Length;
			}

			return new System.Memory<T>(this.array, start, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public System.ReadOnlyMemory<T> AsReadOnlyMemory(int start = 0, int length = 0)
		{
			Debug.Assert(this.Valid);

			if (length <= 0)
			{
				length = this.array.Length;
			}

			return new System.ReadOnlyMemory<T>(this.array, start, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (this.Valid)
			{
				ArrayPool<T>.Shared.Return(this.array);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T[](RentedArray<T> @this)
		{
			Debug.Assert(@this.Valid);

			return @this.array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator System.Span<T>(RentedArray<T> @this)
		{
			Debug.Assert(@this.Valid);

			return @this.array;
		}
	}
}
