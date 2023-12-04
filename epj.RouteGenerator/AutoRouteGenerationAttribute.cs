using System;

namespace epj.RouteGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class AutoRouteGenerationAttribute : Attribute
{
    public string Suffix { get; }

    public AutoRouteGenerationAttribute(string suffix)
    {
        Suffix = suffix;
    }
}