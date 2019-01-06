![logo](https://raw.githubusercontent.com/AngleSharp/AngleSharp.Xml/master/header.png)

# AngleSharp.Xml

[![Build Status](https://img.shields.io/appveyor/ci/FlorianRappl/AngleSharp-Xml.svg?style=flat-square)](https://ci.appveyor.com/project/FlorianRappl/AngleSharp-Xml)
[![GitHub Tag](https://img.shields.io/github/tag/AngleSharp/AngleSharp.Xml.svg?style=flat-square)](https://github.com/AngleSharp/AngleSharp.Xml/releases)
[![NuGet Count](https://img.shields.io/nuget/dt/AngleSharp.Xml.svg?style=flat-square)](https://www.nuget.org/packages/AngleSharp.Xml/)
[![Issues Open](https://img.shields.io/github/issues/AngleSharp/AngleSharp.Xml.svg?style=flat-square)](https://github.com/AngleSharp/AngleSharp.Xml/issues)
[![StackOverflow Questions](https://img.shields.io/stackexchange/stackoverflow/t/anglesharp.svg?style=flat-square)](https://stackoverflow.com/tags/anglesharp)
[![CLA Assistant](https://cla-assistant.io/readme/badge/AngleSharp/AngleSharp.Xml?style=flat-square)](https://cla-assistant.io/AngleSharp/AngleSharp.Xml)

AngleSharp.Xml extends the core AngleSharp library with some XML capabilities. This repository is the home of the source for the AngleSharp.Xml NuGet package.

## Basic Configuration

If you just want a configuration *that works* you should use the following code:

```cs
var config = Configuration.Default
    .WithXml(); // from AngleSharp.Xml
```

This will register a parser for XML related content.

## Advantages of AngleSharp.Xml over System.Xml

(tbd)

## Features

- Fully compliant XML parser
- XML DOM similar to HTML(5)
- DTD parser and validation check
- XML serialization

## Participating

Participation in the project is highly welcome. For this project the same rules as for the AngleSharp core project may be applied.

If you have any question, concern, or spot an issue then please report it before opening a pull request. An initial discussion is appreciated regardless of the nature of the problem.

## License

The MIT License (MIT)

Copyright (c) 2019 AngleSharp

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
