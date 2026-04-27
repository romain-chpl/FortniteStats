using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FortniteStatsDesktop.Models;

namespace FortniteStatsDesktop.Services
{
    public class MatchDataService
    {
        private readonly string _dataDirectory;
        public string DataDirectory => _dataDirectory;

        public MatchDataService()
        {
            // Dossier où seront stockés les .json des parties analysées
            _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ParsedMatches");
            
            // Fallback (si en environnement de prod)
            if (!Directory.Exists(_dataDirectory))
            {
                _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ParsedMatches");
                if (!Directory.Exists(_dataDirectory))
                {
                    Directory.CreateDirectory(_dataDirectory);
                }
            }
        }

        public async Task<List<ParsedMatchData>> GetAllMatchesAsync()
        {
            var matches = new List<ParsedMatchData>();

            if (!Directory.Exists(_dataDirectory))
                return matches;

            var files = Directory.GetFiles(_dataDirectory, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var match = JsonSerializer.Deserialize<ParsedMatchData>(json);
                    
                    if (match != null)
                    {
                        match.FileName = Path.GetFileName(file);
                        matches.Add(match);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur de lecture du match {file}: {ex.Message}");
                }
            }

            return matches.OrderByDescending(m => new FileInfo(Path.Combine(_dataDirectory, m.FileName)).CreationTime).ToList();
        }

        public async Task<ParsedMatchData?> GetMatchByIdAsync(string username, string matchId)
        {
            var allMatches = await GetAllMatchesAsync();
            // Recherche flexible : par ID joueur, par branch, ou dans le nom de fichier
            return allMatches.FirstOrDefault(m =>
                string.Equals(m.PovStats.Id, matchId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.MatchInfo.Branch, matchId, StringComparison.OrdinalIgnoreCase) ||
                m.FileName.Contains(matchId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task SaveMatchAsync(ParsedMatchData matchData)
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            // Génère un nom de fichier unique basé sur le MatchId + timestamp pour éviter les doublons
            string safeBranch = string.Concat(matchData.MatchInfo.Branch.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
            if (string.IsNullOrWhiteSpace(safeBranch)) safeBranch = "Unknown";
            string fileName = $"Match_{safeBranch}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(_dataDirectory, fileName);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(matchData, options);

            await File.WriteAllTextAsync(filePath, json);
            Console.WriteLine($"💾 Partie sauvegardée : {fileName}");
        }
    }
}
