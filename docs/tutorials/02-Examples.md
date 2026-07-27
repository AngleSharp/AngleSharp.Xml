---
title: "Examples"
section: "AngleSharp.Xml"
---
# Example Code

This is a (growing) list of examples for every-day usage of AngleSharp.Xml.

## Parse and query

```cs
var parser = new XmlParser();
var document = parser.ParseDocument(@"<catalog>
  <book id=""b1""><title>AngleSharp in Action</title></book>
  <book id=""b2""><title>XML Basics</title></book>
</catalog>");

var titles = document.QuerySelectorAll("book > title")
	.Select(t => t.TextContent)
	.ToArray();
```

## Update content and serialize

```cs
var root = document.DocumentElement;
var newBook = document.CreateElement("book");
newBook.SetAttribute("id", "b3");

var title = document.CreateElement("title");
title.TextContent = "Practical XML";

newBook.AppendChild(title);
root.AppendChild(newBook);

var xml = document.ToXml();
```

## Parse with diagnostics

```cs
var parser = new XmlParser(new XmlParserOptions
{
	IsKeepingSourceReferences = true,
	OnCreated = (element, position) =>
	{
		Console.WriteLine($"Created {element.NodeName} at {position.Line}:{position.Column}");
	}
});

var document = parser.ParseDocument("<a><b/><c/></a>");
```

## Namespace-aware attributes

```cs
var document = new XmlParser().ParseDocument(@"<doc xml:lang=""en"">text</doc>");
var attr = document.DocumentElement.Attributes["xml:lang"];

Console.WriteLine(attr.NamespaceUri);
// http://www.w3.org/XML/1998/namespace
```

## Force empty tags to be self-closing

```cs
var formatter = new XmlMarkupFormatter
{
	IsAlwaysSelfClosing = true,
};

var output = document.ToHtml(formatter);
```

## Load XML through browsing context

```cs
var config = Configuration.Default
	.WithDefaultLoader()
	.WithXml();

var context = BrowsingContext.New(config);
var document = await context.OpenAsync(req =>
{
	req.Content("<feed><item>One</item></feed>");
	req.Header("Content-Type", "application/xml");
});
```

This mode is useful when you want uniform loading behavior for HTML, XML, and SVG in the same application.
