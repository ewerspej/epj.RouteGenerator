namespace RouteGeneratorSample;

internal abstract class BasePage<T> : ContentPage where T : class
{
    protected BasePage(T viewModel, string pageTitle)
    {
        base.BindingContext = viewModel;

        Title = pageTitle;
    }

    protected new T BindingContext => (T)base.BindingContext;
}