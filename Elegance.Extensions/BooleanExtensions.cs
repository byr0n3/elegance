namespace Elegance.Extensions
{
	public static class BooleanExtensions
	{
		extension(bool value)
		{
			public string Str() =>
				value ? "true" : "false";
		}
	}
}
