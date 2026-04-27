using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace FortniteStatsDesktop.Models
{
    public class ParsedMatchData
    {
        [JsonPropertyName("matchInfo")]
        public MatchInfoData MatchInfo { get; set; } = new();

        [JsonPropertyName("povStats")]
        public PovStatsData PovStats { get; set; } = new();

        [JsonPropertyName("leaderboard")]
        public List<LeaderboardPlayer> Leaderboard { get; set; } = new();

        [JsonPropertyName("killFeed")]
        public List<KillFeedEntry> KillFeed { get; set; } = new();

        // Custom property to store filename or save date securely
        [JsonIgnore]
        public string FileName { get; set; } = string.Empty;
    }

    public class MatchInfoData
    {
        [JsonPropertyName("mapName")]
        public string MapName { get; set; } = string.Empty;
        
        [JsonPropertyName("totalElims")]
        public int TotalElims { get; set; }
        
        [JsonPropertyName("branch")]
        public string Branch { get; set; } = string.Empty;

        [JsonPropertyName("matchDate")]
        public DateTime MatchDate { get; set; }
    }

    public class PovStatsData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("damageDealt")]
        public int DamageDealt { get; set; }

        [JsonPropertyName("damageTaken")]
        public int DamageTaken { get; set; }

        [JsonPropertyName("weaponDamage")]
        public int WeaponDamage { get; set; }

        [JsonPropertyName("otherDamage")]
        public int OtherDamage { get; set; }

        [JsonPropertyName("accuracy")]
        public double Accuracy { get; set; }

        [JsonPropertyName("materialsUsed")]
        public int MaterialsUsed { get; set; }
    }

    public class LeaderboardPlayer
    {
        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("kills")]
        public int Kills { get; set; }

        [JsonPropertyName("teamIndex")]
        public int TeamIndex { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; } = string.Empty;

        [JsonPropertyName("deathCause")]
        public string DeathCause { get; set; } = string.Empty;

        [JsonPropertyName("deathLocation")]
        public DeathLocation? DeathLocation { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class DeathLocation
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("z")]
        public double Z { get; set; }
    }

    public class KillFeedEntry
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("killer")]
        public string Killer { get; set; } = string.Empty;

        [JsonPropertyName("victim")]
        public string Victim { get; set; } = string.Empty;

        [JsonPropertyName("cause")]
        public string Cause { get; set; } = string.Empty;

        [JsonPropertyName("isKnocked")]
        public bool IsKnocked { get; set; }
    }
}
