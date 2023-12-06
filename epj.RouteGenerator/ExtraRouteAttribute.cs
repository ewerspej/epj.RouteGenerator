using System;

namespace epj.RouteGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class ExtraRouteAttribute : Attribute
{
    public string Route { get; }

    public ExtraRouteAttribute(string route)
    {
        Route = route;
    }
}