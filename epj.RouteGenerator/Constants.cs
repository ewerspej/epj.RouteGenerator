namespace epj.RouteGenerator;

public static class Constants
{
    public const string Namespace = "epj.RouteGenerator";
    public const string AutoRoutesAttribute = "AutoRoutesAttribute";
    public const string ExtraRouteAttribute = "ExtraRouteAttribute";
    public const string ARG001 = "ARG001";
    public const string EXR001 = "EXR001";
    public const string EXR002 = "EXR002";
    public const string AutoRoutesFullName = $"{Namespace}.{AutoRoutesAttribute}";
    public const string ExtraRouteFullName = $"{Namespace}.{ExtraRouteAttribute}";
    public const string RoutesGenFileName = "Routes.g.cs";
    public const string Error = "Error";
    public const string ErrorCategoryCompilation = "Compilation";
    public const string ClassNameRegex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
}