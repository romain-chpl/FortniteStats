using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FortniteReplayReader;
using FortniteStatsDesktop.Models;

namespace FortniteStatsDesktop.Services
{
    public class ReplayService
    {
        public ParsedMatchData? ParseReplay(string replayPath, string playerUsername = "")
        {
            if (!File.Exists(replayPath))
            {
                Console.WriteLine("❌ Fichier replay introuvable.");
                return null;
            }

            try
            {
                Console.WriteLine("📡 Extraction en cours du replay ...");
                var reader = new ReplayReader();
                var replay = reader.ReadReplay(replayPath);

                if (replay == null) return null;

                // 1. DICTIONNAIRE (ID -> Pseudo) pour traduire les IDs dans le kill feed
                var playerLookup = new Dictionary<string, string>();
                if (replay.PlayerData != null)
                {
                    foreach (var p in replay.PlayerData)
                    {
                        string id = p.PlayerId ?? p.BotId ?? "";
                        if (!string.IsNullOrEmpty(id) && !playerLookup.ContainsKey(id))
                        {
                            playerLookup.Add(id, string.IsNullOrEmpty(p.PlayerName) ? "Anonyme" : p.PlayerName);
                        }
                    }
                }

                // 2. RECHERCHE DU JOUEUR POV
                // Priorité : pseudo défini par l'utilisateur > IsReplayOwner > premier joueur
                FortniteReplayReader.Models.PlayerData? owner = null;

                if (!string.IsNullOrWhiteSpace(playerUsername) && replay.PlayerData != null)
                {
                    owner = replay.PlayerData.FirstOrDefault(p =>
                        string.Equals(p.PlayerName, playerUsername, StringComparison.OrdinalIgnoreCase));
                }

                owner ??= replay.PlayerData?.FirstOrDefault(p => p.IsReplayOwner == true)
                       ?? replay.PlayerData?.FirstOrDefault();

                string ownerName = owner?.PlayerName ?? "Anonyme";
                string ownerId = owner?.PlayerId ?? owner?.BotId ?? "";

                // 3. LEADERBOARD COMPLET avec tous les champs du format JSON de référence
                var leaderboardData = new List<LeaderboardPlayer>();
                if (replay.PlayerData != null)
                {
                    leaderboardData = replay.PlayerData
                        .Where(p => p.Placement.HasValue && !p.IsBot)
                        .OrderBy(p => p.Placement!.Value)
                        .Select(p => new LeaderboardPlayer
                        {
                            Rank = (int)(p.Placement ?? 0),
                            Username = string.IsNullOrEmpty(p.PlayerName) ? "Anonyme" : p.PlayerName,
                            Kills = (int)(p.Kills ?? 0),
                            TeamIndex = p.TeamIndex.HasValue ? (int)p.TeamIndex.Value : 0,
                            Platform = p.Platform ?? "Inconnue",
                            DeathCause = TranslateDeathCause(p.DeathCause),
                            Id = p.PlayerId ?? p.BotId ?? "",
                            DeathLocation = (p.DeathLocation != null)
                                ? new DeathLocation
                                {
                                    X = p.DeathLocation.X,
                                    Y = p.DeathLocation.Y,
                                    Z = p.DeathLocation.Z
                                }
                                : null
                        }).ToList();
                }

                // 4. KILL FEED avec isKnocked et cause traduite
                var killFeedData = new List<KillFeedEntry>();
                if (replay.Eliminations != null)
                {
                    killFeedData = replay.Eliminations.Select(e => new KillFeedEntry
                    {
                        Time = e.Time ?? "00:00",
                        Killer = (!string.IsNullOrEmpty(e.Eliminator) && playerLookup.ContainsKey(e.Eliminator))
                            ? playerLookup[e.Eliminator] : (e.Eliminator ?? "Inconnu"),
                        Victim = (!string.IsNullOrEmpty(e.Eliminated) && playerLookup.ContainsKey(e.Eliminated))
                            ? playerLookup[e.Eliminated] : (e.Eliminated ?? "Inconnu"),
                        Cause = TranslateGunType(e.GunType, e.IsSelfElimination),
                        IsKnocked = false // La librairie ne fournit pas IsKnocked
                    }).ToList();
                }

                // 5. STATS POV complètes
                string matchId = replay.Header?.Branch ?? $"Match_{DateTime.Now:yyyyMMdd_HHmmss}";

                var finalData = new ParsedMatchData
                {
                    MatchInfo = new MatchInfoData
                    {
                        Branch = matchId,
                        MapName = replay.Header?.LevelNamesAndTimes?.FirstOrDefault().Item1 ?? "Athena_Terrain",
                        TotalElims = killFeedData.Count(k => !k.IsKnocked),
                        MatchDate = File.GetCreationTime(replayPath)
                    },
                    PovStats = new PovStatsData
                    {
                        Id = ownerId,
                        Username = ownerName,
                        DamageDealt = (int)(replay.Stats?.DamageToPlayers ?? 0),
                        DamageTaken = (int)(replay.Stats?.DamageTaken ?? 0),
                        WeaponDamage = (int)(replay.Stats?.DamageToPlayers ?? 0),
                        OtherDamage = 0,
                        Accuracy = replay.Stats?.Accuracy ?? 0f,
                        MaterialsUsed = (int)(replay.Stats?.MaterialsUsed ?? 0)
                    },
                    Leaderboard = leaderboardData,
                    KillFeed = killFeedData
                };

                Console.WriteLine($"✅ Parsing terminé : {ownerName} | TOP {owner?.Placement ?? 0} | {killFeedData.Count} events");
                return finalData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur de parsing : {ex.Message}");
                return null;
            }
        }

        private static string TranslateGunType(byte? gunType, bool isSelfElim)
        {
            if (isSelfElim) return "Suicide / Environnement";
            if (!gunType.HasValue || gunType == 0) return "Inconnu / Décor";
            return gunType.Value switch
            {
                1 => "Pistolet",
                2 => "Fusil à pompe",
                3 => "Fusil d'assaut",
                4 => "PM (SMG)",
                5 => "Sniper",
                6 => "Pioche",
                13 => "Explosif",
                _ => $"Arme ({gunType})"
            };
        }

        private static string TranslateDeathCause(int? rawCause)
        {
            if (!rawCause.HasValue) return "En vie / Inconnu";
            return rawCause.Value switch
            {
                0 => "En vie / Inconnu",
                1 => "Arme \u00e0 feu / \u00c9limination",
                2 => "Environnement",
                3 => "La Temp\u00eate (Zone)",
                4 => "Chute",
                5 => "Explosion",
                6 => "Pioche",
                7 => "Suicide / Environnement",
                8 => "Dommages de v\u00e9hicule",
                9 => "Gravit\u00e9",
                _ => $"Cause ({rawCause})"
            };
        }
    }
}