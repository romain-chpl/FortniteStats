using System;
using System.Linq;
using System.Windows;
using FortniteStatsDesktop.Services;
using Microsoft.Extensions.DependencyInjection;

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

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    // On récupère le service de drag & drop depuis le provider
                    var serviceProvider = Application.Current.Resources["services"] as IServiceProvider;
                    var dragDropService = serviceProvider?.GetService<DragDropService>();

                    if (dragDropService != null)
                    {
                        // On notifie Blazor du premier fichier déposé
                        dragDropService.NotifyFileDropped(files[0]);
                    }
                }
            }
        }
    }
}