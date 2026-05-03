using Golyath.UI;

namespace Golyath;

public partial class App
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        _services = services;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new SplashPage(_services));
    }
}