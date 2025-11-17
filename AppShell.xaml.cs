using Microsoft.Maui.Controls;

namespace DailyHelper;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for Shell navigation (used by GoToAsync with route names)
        Routing.RegisterRoute(nameof(KrediHesaplama), typeof(KrediHesaplama));
        Routing.RegisterRoute(nameof(VKIHesaplama), typeof(VKIHesaplama));
        Routing.RegisterRoute(nameof(RenkSecici), typeof(RenkSecici));
    }
}