namespace RouteGeneratorSampleConsole.Route;

public abstract class BaseRoute<T>  where T : new()
{
    protected BaseRoute(T value)
    {
        Value = value;
    }

    public T Value { get; }
}