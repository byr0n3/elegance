using System.Collections.Generic;
using System.Xml;

namespace Elegance.Icons
{
	internal readonly struct Element
	{
		public readonly string Tag;
		public readonly Dictionary<string, string> Attributes;
		public readonly List<Element>? Children;

		private Element(string tag, Dictionary<string, string> attributes, List<Element>? children)
		{
			this.Tag = tag;
			this.Attributes = attributes;
			this.Children = children;
		}

		public void Append(IndentedSourceBuilder builder)
		{
			builder.AppendLine($"__builder.OpenElement(0, \"{this.Tag}\");");

			foreach (var pair in this.Attributes)
			{
				builder.AppendLine($"__builder.AddAttribute(0, \"{pair.Key}\", \"{pair.Value}\");");
			}

			if (this.Children is not null)
			{
				foreach (var child in this.Children)
				{
					child.Append(builder);
				}
			}

			builder.AppendLine("__builder.CloseElement();");
		}

		public static bool TryRead(XmlReader reader, out Element @out)
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				@out = default;
				return false;
			}

			var tag = reader.Name;

			var attributeCount = reader.AttributeCount;
			var attributes = new Dictionary<string, string>(attributeCount);

			for (var i = 0; i < attributeCount; i++)
			{
				reader.MoveToAttribute(i);

				attributes[reader.Name] = reader.Value;
			}

			// Move back to the parent element, we're (possibly) currently on an attribute.
			reader.MoveToElement();

			var children = Element.ReadChildren(reader);

			@out = new Element(tag, attributes, children);
			return true;
		}

		private static List<Element>? ReadChildren(XmlReader reader)
		{
			// By reading the subtree, we create a reader that will start from the parent node and then can go through all the descendants.
			var subtree = reader.ReadSubtree();

			// Read the first element (the current/parent node).
			subtree.Read();

			// Self-closing elements (like <path ... />) counts as an 'empty element', so we need to quit when this happens.
			if (subtree.IsEmptyElement)
			{
				return null;
			}

			List<Element>? children = null;

			while (subtree.Read())
			{
				if ((subtree.NodeType == XmlNodeType.EndElement) || !Element.TryRead(subtree, out var child))
				{
					break;
				}

				if (children is null)
				{
					children = [child];
				}
				else
				{
					children.Add(child);
				}
			}

			subtree.Dispose();

			return children;
		}
	}
}
