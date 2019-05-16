var target = Argument("target", "Default");
var projectName = "AngleSharp.Xml";
var solutionName = "AngleSharp.Xml";
var frameworks = new Dictionary<String, String>
{
    { "netstandard1.3", "netstandard1.3" },
};

#load tools/anglesharp.cake

RunTarget(target);
