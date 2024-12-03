using System.Runtime.CompilerServices;

namespace RouteGeneratorSample.Cars;

partial class AudiPage
{
    private void SayHello([CallerMemberName] string name = "")
    {
        Console.WriteLine($"Hello from {name}");
    }
}