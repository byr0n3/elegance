using System.Text;

namespace Elegance.Icons
{
	// @todo Place in `Elegance.SourceGeneration.Shared`
	internal sealed class IndentedSourceBuilder
	{
		private readonly StringBuilder builder = new();

		private int level;

		private StringBuilder AppendLevel(int depth) =>
			this.builder.Append('\t', depth);

		public IndentedSourceBuilder AppendPush()
		{
			this.AppendLevel(this.level++);
			return this;
		}

		public IndentedSourceBuilder AppendPush(string? @string)
		{
			this.AppendLevel(this.level++).Append(@string);
			return this;
		}

		public IndentedSourceBuilder AppendPush(char? @char)
		{
			this.AppendLevel(this.level++).Append(@char);
			return this;
		}

		public IndentedSourceBuilder AppendPushLine()
		{
			this.AppendLevel(this.level++).Append('\n');
			return this;
		}

		public IndentedSourceBuilder AppendPushLine(string? @string)
		{
			this.AppendLevel(this.level++).Append(@string).Append('\n');
			return this;
		}

		public IndentedSourceBuilder AppendPushLine(char? @char)
		{
			this.AppendLevel(this.level++).Append(@char).Append('\n');
			return this;
		}

		public IndentedSourceBuilder AppendPop()
		{
			this.AppendLevel(--this.level);
			return this;
		}

		public IndentedSourceBuilder AppendPop(string? @string)
		{
			this.AppendLevel(--this.level).Append(@string);
			return this;
		}

		public IndentedSourceBuilder AppendPop(char? @char)
		{
			this.AppendLevel(--this.level).Append(@char);
			return this;
		}

		public IndentedSourceBuilder AppendPopLine()
		{
			this.AppendLevel(--this.level).Append('\n');
			return this;
		}

		public IndentedSourceBuilder AppendPopLine(string? @string)
		{
			this.AppendLevel(--this.level).Append(@string).Append('\n');
			return this;
		}

		public IndentedSourceBuilder AppendPopLine(char? @char)
		{
			this.AppendLevel(--this.level).Append(@char).Append('\n');
			return this;
		}

		public IndentedSourceBuilder Append(char? @char)
		{
			this.AppendLevel(this.level).Append(@char);
			return this;
		}

		public IndentedSourceBuilder Append(string? @string)
		{
			this.AppendLevel(this.level).Append(@string);
			return this;
		}

		public IndentedSourceBuilder AppendLine(string? @string = null)
		{
			this.AppendLevel(this.level).Append(@string).Append('\n');
			return this;
		}

		public void Clear()
		{
			this.builder.Clear();
			this.level = default;
		}

		public override string ToString() =>
			this.builder.ToString();
	}
}
