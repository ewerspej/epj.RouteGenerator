# epj.RouteGenerator

![License](https://img.shields.io/github/license/ewerspej/epj.RouteGenerator)
[![Nuget](https://img.shields.io/nuget/v/epj.RouteGenerator)](https://www.nuget.org/packages/epj.RouteGenerator/)


Tired of manually specifying route identifiers and fixing typos in the route-based navigation of your .NET app? This source generator will take away that pain.

## Introduction

Route Generator is a C# source generator that generates a static `Routes` class for your .NET app containing all route identifiers for your string-based route navigation.

Although the sample project is using .NET MAUI, this generator can also be used with other .NET technologies since it targets .NET Standard 2.0.

## Basic Usage

First, add the [epj.RouteGenerator](https://www.nuget.org/packages/epj.RouteGenerator/) nuget package to your target project that contains the classes (i.e. pages) from which routes should be automatically generated.

Then, use the `[AutoRoutes]` attribute from the `epj.RouteGenerator`namespace on one of the classes at the root of your application. This must be within the same project and namespace containing the pages.

You must provide a *suffix* argument which represents the naming convention for your routes to the attribute, e.g. "Page" like above. It doesn't have to be "Page", it depends on the naming convention you use for pages in your app. If all your page classes end in "Site", then you would pass "Site" to the attribute.
This suffix is used in order to identify all classes that should be included as routes in the generated `Routes` class based on their *class name*.

When using .NET MAUI, I recommend to use the attribute to decorate the `MauiProgram` class:

```c#
[AutoRoutes("Page")]
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // ...

        return builder.Build();
    }
}
```

The source generator will then pick up all pages that end in the specified suffix and generate the `Routes` class with these identifiers within the same root namespace as the entry point:

```c#
// <auto-generated/>
using System.Collections.ObjectModel;

namespace RouteGeneratorSample
{
    public static class Routes
    {
        public const string MainPage = "MainPage";
        public const string VolvoPage = "VolvoPage";
        public const string AudiPage = "AudiPage";
    
        private static List<string> allRoutes = new()
        {
            MainPage,
            VolvoPage,
            AudiPage,
        };
        
        public static ReadOnlyCollection<string> AllRoutes => allRoutes.AsReadOnly();
    }
}
```

Now, you can use these identifiers for your navigation without having to worry about typos in your string-based route navigation:

```c#
await Shell.Current.GoToAsync($"/{Routes.AudiPage}");
```

If the `AudiPage` would ever get renamed to some other class name, you would instantly notice, because the compiler wouldn't find the `Routes.AudiPage` symbol anymore and emit an error, letting you know that it has changed. When using verbatim strings instead of an identifier like this, you would notice this change until the app either crashes or stops behaving the way it should.

## Extra Routes

There may be situations where you need to be able to specify extra routes, e.g. when a route doesn't follow the specified naming convention using the suffix or when a routes is defined in a different way (in MAUI or Xamarin.Forms this could be using a `<ShellContent>` element in XAML).

For situations like these, the Route Generator exposes a second attribute called `[ExtraRoute]` and it takes a single argument representing the name of the route. You may not pass null, empty strings or whitespace as well as special characters. Duplicates will be ignored.

```c#
namespace RouteGeneratorSample;

[AutoRoutes("Page")]
[ExtraRoute("SomeOtherRoute")] // valid
[ExtraRoute("SomeFaulty!Route")] // invalid
[ExtraRoute("YetAnotherRoute")] // valid
[ExtraRoute("YetAnotherRoute")] // ignored, because it's a duplicate
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // ...

        return builder.Build();
    }
}
```

This would then result in the following `Routes`:

```c#
// <auto-generated/>
using System.Collections.ObjectModel;

namespace RouteGeneratorSample
{
    public static class Routes
    {
        public const string MainPage = "MainPage";
        public const string VolvoPage = "VolvoPage";
        public const string AudiPage = "AudiPage";
        public const string SomeOtherRoute = "SomeOtherRoute";
        public const string YetAnotherRoute = "YetAnotherRoute";
    
        private static List<string> allRoutes = new()
        {
            MainPage,
            VolvoPage,
            AudiPage,
            SomeOtherRoute,
            YetAnotherRoute
        };
        
        public static ReadOnlyCollection<string> AllRoutes => allRoutes.AsReadOnly();
    }
}
```

## Route registration (e.g. in .NET MAUI)

[Miguel Delgado](https://github.com/mdelgadov) pointed out that routes *could* technically be registered like follows using reflection, e.g. when using .NET MAUI (thanks for this):

```c#
foreach (var route in Routes.AllRoutes)
{
    Routing.RegisterRoute(route, Type.GetType(route));
}
```

**Note:** This only works if the `foreach`-loop is executed from within the same namespace as the pages, which often is not the case. This is because `Type.GetType(typename)` doesn't walk the up namespaces to find the matching type.

Since the library is not MAUI-specific, I will not add such a utility method directly to this library. However, as mentioned below, automatic registration could be handled in a MAUI-specific layer.

# Future Ideas

- Platform-specific layer(s), e.g. epj.RouteGenerator.MAUI
  - Automatic route registration
  - Automatic registration of Pages and ViewModels as services
  - Generation of route-specific extensions or methods (e.g. `Shell.Current.GoToMyAwesomePage(params)`)

# Remarks about Native AOT support

While Native AOT is still experimental in .NET 8.0 (e.g., it's not supported for Android yet and even iOS still is experiencing some hiccups), the latest version of Route Generator should technically be [AOT-compatible](https://learn.microsoft.com/dotnet/core/deploying/native-aot#limitations-of-native-aot-deployment). However, I cannot test this properly while there are still issues. Full Native AOT support will probably only be available with .NET 9.0 or higher: https://github.com/dotnet/maui/issues/18839#issuecomment-1828006233
