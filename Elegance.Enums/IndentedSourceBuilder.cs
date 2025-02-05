using System.Text;

namespace Elegance.Enums
{
	// @todo Place in `Elegance.SourceGeneration.Shared`
	internal sealed class IndentedSourceBuilder
	{
		private readonly StringBuilder builder = new();

		private int level;

		private StringBuilder AppendLevel(int depth) =>
			this.builder.Append('\t', depth);

		public IndentedSourceBuilder AppendPush(string? @string = default)
		{
			this.AppendLevel(this.level++).Append(@string);
			return this;
		}

		public IndentedSourceBuilder AppendLinePush(string? @string = default)
		{
			this.AppendLevel(this.level++).Append(@string).Append('\n');
			return this;
		}

		public IndentedSourceBuilder AppendPop(string? @string = default)
		{
			this.AppendLevel(--this.level).Append(@string);
			return this;
		}

		public IndentedSourceBuilder AppendLinePop(string? @string = default)
		{
			this.AppendLevel(--this.level).Append(@string).Append('\n');
			return this;
		}

		public IndentedSourceBuilder Append(string? @string)
		{
			this.AppendLevel(this.level).Append(@string);
			return this;
		}

		public IndentedSourceBuilder AppendLine(string? @string)
		{
			this.AppendLevel(this.level).Append(@string).Append('\n');
			return this;
		}

		public IndentedSourceBuilder AppendLine(char? @char)
		{
			this.AppendLevel(this.level).Append(@char).Append('\n');
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
