using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FortniteStatsDesktop.Services
{
    /// <summary>
    /// Service qui scanne un dossier de replays, analyse les fichiers non encore parsés,
    /// et retente toutes les 15s uniquement le dernier fichier (partie en cours).
    /// </summary>
    public class ReplayWatcherService : IDisposable
    {
        // Ensemble des fichiers (noms uniquement ou ID unique) déjà traités avec succès
        private HashSet<string> _parsedNames = new(StringComparer.OrdinalIgnoreCase);
        // Fichiers détectés comme "en cours" (à retenter)
        private readonly HashSet<string> _ongoingFiles = new(StringComparer.OrdinalIgnoreCase);

        private CancellationTokenSource? _cts;
        private Task? _scanTask;
        private string? _historyFilePath;

        public string? CurrentPath { get; private set; }
        public bool IsWatching => _scanTask != null && !_scanTask.IsCompleted;

        /// <summary>
        /// Déclenché pour chaque fichier .replay à parser.
        /// (path, isLatest, isOngoing)
        /// </summary>
        public event Func<string, bool, bool, Task>? OnReplayDetected;

        /// <summary>Démarre le scan périodique du dossier spécifié.</summary>
        /// <param name="path">Chemin des replays</param>
        /// <param name="historyPath">Chemin du fichier d'historique (persistance)</param>
        public void StartWatching(string path, string historyPath)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                throw new ArgumentException("Le dossier spécifié n'existe pas ou est invalide.", nameof(path));

            _historyFilePath = historyPath;
            LoadHistory();

            StopWatching();
            CurrentPath = path;
            _cts = new CancellationTokenSource();
            _scanTask = ScanLoopAsync(path, _cts.Token);
            Console.WriteLine($"[ReplayWatcher] 👀 Scan démarré : {path}");
        }

        private void LoadHistory()
        {
            try
            {
                if (_historyFilePath != null && File.Exists(_historyFilePath))
                {
                    string json = File.ReadAllText(_historyFilePath);
                    var list = JsonSerializer.Deserialize<List<string>>(json);
                    if (list != null)
                        _parsedNames = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReplayWatcher] ❌ Erreur chargement historique : {ex.Message}");
            }
        }

        private void SaveHistory()
        {
            try
            {
                if (_historyFilePath != null)
                {
                    var dir = Path.GetDirectoryName(_historyFilePath);
                    if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    string json = JsonSerializer.Serialize(_parsedNames.ToList());
                    File.WriteAllText(_historyFilePath, json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReplayWatcher] ❌ Erreur sauvegarde historique : {ex.Message}");
            }
        }

        /// <summary>Arrête le scan en cours.</summary>
        public void StopWatching()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _scanTask = null;
            Console.WriteLine("[ReplayWatcher] 🛑 Scan arrêté.");
        }

        /// <summary>Marque un fichier comme correctement parsé (persistant).</summary>
        public void MarkAsParsed(string fullPath)
        {
            string fileName = Path.GetFileName(fullPath);
            _parsedNames.Add(fileName);
            _ongoingFiles.Remove(fullPath);
            SaveHistory();
        }

        /// <summary>Indique qu'un fichier est en cours ou corrompu.</summary>
        public void MarkAsOngoing(string fullPath)
        {
            if (!_parsedNames.Contains(Path.GetFileName(fullPath)))
                _ongoingFiles.Add(fullPath);
        }

        /// <summary>Ignore définitivement un fichier (ex: corrompu et pas le dernier).</summary>
        public void MarkAsIgnored(string fullPath)
        {
            MarkAsParsed(fullPath); // On le traite comme parsé pour ne plus le voir
        }

        private async Task ScanLoopAsync(string path, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var files = Directory.GetFiles(path, "*.replay")
                                         .Select(f => new FileInfo(f))
                                         .OrderBy(f => f.CreationTime)
                                         .ToList();

                    if (files.Any())
                    {
                        var latestFile = files.Last();

                        foreach (var fileInfo in files)
                        {
                            if (ct.IsCancellationRequested) break;

                            string file = fileInfo.FullName;
                            string name = fileInfo.Name;
                            bool isLatest = file == latestFile.FullName;
                            bool isOngoing = _ongoingFiles.Contains(file);
                            bool alreadyParsed = _parsedNames.Contains(name);

                            if (!alreadyParsed)
                            {
                                // REQUIREMENT: Ignorer corrompus sauf le dernier
                                if (isOngoing && !isLatest)
                                {
                                    Console.WriteLine($"[ReplayWatcher] ⏩ Ignorance d'un fichier corrompu (ancien) : {name}");
                                    MarkAsIgnored(file);
                                    continue;
                                }

                                if (OnReplayDetected != null)
                                    await OnReplayDetected.Invoke(file, isLatest, isOngoing);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ReplayWatcher] ❌ Erreur scan : {ex.Message}");
                }

                // Attendre 15 secondes avant le prochain scan
                try { await Task.Delay(15_000, ct); }
                catch (TaskCanceledException) { break; }
            }
        }

        public void Dispose()
        {
            StopWatching();
        }
    }
}
