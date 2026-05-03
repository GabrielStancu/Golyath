using Golyath.UI.Views.Profile;

namespace Golyath;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
    }
}
