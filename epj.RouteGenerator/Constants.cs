﻿namespace epj.RouteGenerator;

public static class Constants
{
    public const string Namespace = "epj.RouteGenerator";
    public const string AutoRoutesAttribute = "AutoRoutesAttribute";
    public const string ExtraRouteAttribute = "ExtraRouteAttribute";
    public const string IgnoreRouteAttribute = "IgnoreRouteAttribute";
    public const string IgnoreRouteName = "IgnoreRoute";
    public const string RGE001 = "RGE001"; // general route generation error
    public const string ARG001 = "ARG001"; // suffix parameter required
    public const string EXR001 = "EXR001"; // invalid route name
    public const string EXR002 = "EXR002"; // duplicate route name
    public const string EXR003 = "EXR003"; // missing class or type name
    public const string AutoRoutesFullName = $"{Namespace}.{AutoRoutesAttribute}";
    public const string ExtraRouteFullName = $"{Namespace}.{ExtraRouteAttribute}";
    public const string RoutesGenFileName = "Routes.g.cs";
    public const string Error = "Error";
    public const string Warning = "Warning";
    public const string ErrorCategoryCompilation = "Compilation";
    public const string ClassNameRegex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
}