using System.Globalization;
using System.Windows;
using FancyTab.Services;

namespace FancyTab;

public partial class App : Application
{
    public static LocalizationService Localization { get; } = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set default culture based on system
        var culture = CultureInfo.CurrentUICulture;
        if (culture.Name.StartsWith("zh"))
        {
            Localization.SetLanguage("zh-CN");
        }
        else
        {
            Localization.SetLanguage("en-US");
        }
    }
}
