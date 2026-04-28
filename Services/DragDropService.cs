using System;

namespace FortniteStatsDesktop.Services
{
    /// <summary>
    /// Service permettant de faire le pont entre les événements de Drag & Drop natifs (WPF)
    /// et les composants Blazor.
    /// </summary>
    public class DragDropService
    {
        /// <summary>
        /// Événement déclenché lorsqu'un fichier est déposé sur la fenêtre.
        /// </summary>
        public event Action<string>? OnFileDropped;

        /// <summary>
        /// Appelé par le code natif (MainWindow.xaml.cs) pour notifier Blazor.
        /// </summary>
        /// <param name="filePath">Chemin complet du fichier déposé.</param>
        public void NotifyFileDropped(string filePath)
        {
            OnFileDropped?.Invoke(filePath);
        }
    }
}
