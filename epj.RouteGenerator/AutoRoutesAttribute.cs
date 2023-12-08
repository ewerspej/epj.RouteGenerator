using System;

namespace epj.RouteGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class AutoRoutesAttribute : Attribute
{
    public string Suffix { get; }

    public AutoRoutesAttribute(string suffix)
    {
        Suffix = suffix;
    }
}