using System;
using System.Windows;

namespace FortniteStatsDesktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // ÉTAPE CRUCIALE : On force les arguments Chromium AVANT l'initialisation des composants
            // Cela règle le problème de dégradés (Color Banding) en forçant le sRGB
            Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--force-color-profile=srgb");

            InitializeComponent();
        }

    }
}