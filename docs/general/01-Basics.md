---
title: "Getting Started"
section: "AngleSharp.Xml"
---
# Getting Started

## Requirements

AngleSharp.Xml comes currently in two flavors: on Windows for .NET 4.6 and in general targetting .NET Standard 2.0 platforms.

Most of the features of the library do not require .NET 4.6, which means you could create your own fork and modify it to work with previous versions of the .NET-Framework.

You need to have AngleSharp installed already. This could be done via NuGet:

```ps1
Install-Package AngleSharp
```

## Getting AngleSharp.Xml over NuGet

The simplest way of integrating AngleSharp.Xml to your project is by using NuGet. You can install AngleSharp.Xml by opening the package manager console (PM) and typing in the following statement:

```ps1
Install-Package AngleSharp.Xml
```

You can also use the graphical library package manager ("Manage NuGet Packages for Solution"). Searching for "AngleSharp.Xml" in the official NuGet online feed will find this library.

## Setting up AngleSharp.Xml

To use AngleSharp.Xml you need to add it to your `Configuration` coming from AngleSharp itself.

If you just want a configuration *that works* you should use the following code:

```cs
var config = Configuration.Default
    .WithXml(); // from AngleSharp.Xml
```

This will register a parser for XML related content.
