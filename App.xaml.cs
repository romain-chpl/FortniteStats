using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using FortniteStatsDesktop.Services;

namespace FortniteStatsDesktop
{
    public partial class App : Application
    {
        public App()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView(); // Active le moteur de rendu Chromium

            // On enregistre tes services habituels
            serviceCollection.AddSingleton<ReplayService>();
            serviceCollection.AddSingleton<ReplayWatcherService>();
            serviceCollection.AddSingleton<MatchDataService>();
            serviceCollection.AddSingleton<SettingsService>();
            serviceCollection.AddSingleton<ReplayEventService>();

            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }
    }
}