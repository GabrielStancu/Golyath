using Golyath.Views;

namespace Golyath
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("exercise-detail", typeof(ExerciseDetailPage));
            Routing.RegisterRoute("session-detail", typeof(SessionDetailPage));
            Routing.RegisterRoute("settings", typeof(SettingsPage));
            Routing.RegisterRoute("templates", typeof(TemplateListPage));
            Routing.RegisterRoute("template-edit", typeof(TemplateEditPage));
        }
    }
}

