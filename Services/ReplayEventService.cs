using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FortniteStatsDesktop.Models;

namespace FortniteStatsDesktop.Services
{
    /// <summary>
    /// Service singleton central : scanne le dossier de replays, parse chaque fichier non encore analysé,
    /// gère les parties "en cours" (retry silencieux toutes les 15s), et diffuse les événements aux composants UI.
    /// </summary>
    public class ReplayEventService : IDisposable
    {
        private readonly ReplayWatcherService _watcher;
        private readonly ReplayService _replayService;
        private readonly MatchDataService _matchDataService;
        private readonly SettingsService _settingsService;

        // Événement déclenché quand le parsing "réussi" est diffusé à l'UI
        public event Func<string, Task>? OnParsingStarted;

        // Événement déclenché quand une nouvelle partie est parsée et sauvegardée
        public event Func<ParsedMatchData, Task>? OnNewMatchParsed;

        // État global
        public ParsedMatchData? LatestMatch { get; private set; }
        public bool IsParsing { get; private set; }
        public string? CurrentReplayName { get; private set; }

        /// <summary>
        /// Indique si on est actuellement dans une phase de retry d'une partie ongoing.
        /// (N'affecte plus l'affichage de "Analyse en cours" car l'utilisateur veut du silence)
        /// </summary>
        public bool IsOngoingMatch { get; private set; }

        public ReplayEventService(
            ReplayWatcherService watcher,
            ReplayService replayService,
            MatchDataService matchDataService,
            SettingsService settingsService)
        {
            _watcher = watcher;
            _replayService = replayService;
            _matchDataService = matchDataService;
            _settingsService = settingsService;

            // Abonnement au scanner
            _watcher.OnReplayDetected += HandleReplayDetected;

            // Démarrage automatique
            InitializeWatcher();
        }

        private void InitializeWatcher()
        {
            if (_watcher.IsWatching) return;

            try
            {
                string fnPath = _settingsService.CurrentSettings.ReplaysFolderPath;
                if (string.IsNullOrWhiteSpace(fnPath))
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    fnPath = Path.Combine(localAppData, "FortniteGame", "Saved", "Demos");
                }

                if (Directory.Exists(fnPath))
                {
                    // Utilisation d'un fichier d'historique persistant dans le dossier des matches parsés
                    string historyFile = Path.Combine(_matchDataService.DataDirectory, "parsing_history.json");
                    _watcher.StartWatching(fnPath, historyFile);
                    Console.WriteLine($"[ReplayEventService] Scan démarré : {fnPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReplayEventService] Erreur démarrage : {ex.Message}");
            }
        }

        public void RestartWatcher()
        {
            InitializeWatcher();
        }

        /// <summary>
        /// Handle déclenché par le scanner pour chaque fichier à analyser.
        /// </summary>
        private async Task HandleReplayDetected(string fullPath, bool isLatest, bool isOngoing)
        {
            // Éviter les analyses parallèles (sécurité)
            if (IsParsing) return;

            // REQUIREMENT: Analyse "sans afficher si la game est parsable" (silence UI initial)
            try
            {
                var playerUsername = _settingsService.CurrentSettings.PlayerUsername;
                
                // Analyse silencieuse (Task.Run pour ne pas bloquer le thread de scan)
                var matchData = await Task.Run(() => _replayService.ParseReplay(fullPath, playerUsername));

                if (matchData != null)
                {
                    // Vérifier si la partie est terminée
                    bool isCompleted = matchData.Leaderboard.Count > 5 &&
                                      matchData.Leaderboard.Any(l => l.Username == matchData.PovStats.Username);

                    if (isCompleted)
                    {
                        // REQUIREMENT: "si elle l'est (game terminé) affiche les infos analyse en cours comme normalement"
                        IsParsing = true;
                        CurrentReplayName = Path.GetFileName(fullPath);
                        IsOngoingMatch = false;

                        if (OnParsingStarted != null)
                            await OnParsingStarted.Invoke(CurrentReplayName);

                        // Laisser un court délai pour l'effet visuel "en cours" si on veut
                        await Task.Delay(500);

                        await _matchDataService.SaveMatchAsync(matchData);
                        _watcher.MarkAsParsed(fullPath);
                        
                        LatestMatch = matchData;

                        if (OnNewMatchParsed != null)
                            await OnNewMatchParsed.Invoke(matchData);
                        
                        IsParsing = false;
                    }
                    else
                    {
                        // Match parseable mais incomplet (en cours)
                        if (isLatest)
                        {
                            _watcher.MarkAsOngoing(fullPath);
                            Console.WriteLine($"[ReplayEventService] ⏳ Replay en cours (muet) : {Path.GetFileName(fullPath)}");
                        }
                        else
                        {
                            _watcher.MarkAsIgnored(fullPath);
                            Console.WriteLine($"[ReplayEventService] ⏩ Ancien match incomplet ignoré : {Path.GetFileName(fullPath)}");
                        }
                    }
                }
                else
                {
                    // Parsing échoué (corrompu ou format non supporté)
                    if (isLatest)
                    {
                        _watcher.MarkAsOngoing(fullPath);
                    }
                    else
                    {
                        // REQUIREMENT: Ignorer corromps sauf le dernier
                        _watcher.MarkAsIgnored(fullPath);
                        Console.WriteLine($"[ReplayEventService] ⏩ Replay corrompu ou illisible ignoré : {Path.GetFileName(fullPath)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReplayEventService] ❌ Erreur critique HandleDetected : {ex.Message}");
                if (!isLatest) _watcher.MarkAsIgnored(fullPath);
            }
            finally
            {
                // On s'assure de reset IsParsing au cas où
                IsParsing = false;
            }
        }

        /// <summary>
        /// Parse un fichier replay à la volée (drag & drop).
        /// </summary>
        public async Task<ParsedMatchData?> ParseReplayPreviewAsync(string fullPath)
        {
            try
            {
                var playerUsername = _settingsService.CurrentSettings.PlayerUsername;
                return await Task.Run(() => _replayService.ParseReplay(fullPath, playerUsername));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReplayEventService] Erreur preview : {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            _watcher.OnReplayDetected -= HandleReplayDetected;
        }
    }
}
