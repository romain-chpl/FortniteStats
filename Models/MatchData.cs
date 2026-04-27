namespace FortniteStats.Models
{
    public class MatchData
    {
        public MatchInfo Info { get; set; } = new();
        public PovStats Pov { get; set; } = new();
        public List<PlayerStat> Leaderboard { get; set; } = new();
        public List<KillEvent> Killfeed { get; set; } = new();
    }

    public class MatchInfo
    {
        public string MatchId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Playlist { get; set; } = string.Empty;
    }

    public class PovStats
    {
        public int Kills { get; set; }
        public int DamageDealt { get; set; }
        public double Accuracy { get; set; } // En pourcentage (ex: 35.5)
        public int Placement { get; set; }
    }

    public class PlayerStat
    {
        public string PlayerName { get; set; } = string.Empty;
        public int Placement { get; set; }
        public int Kills { get; set; }
    }

    public class KillEvent
    {
        public string Killer { get; set; } = string.Empty;
        public string Victim { get; set; } = string.Empty;
        public string Weapon { get; set; } = string.Empty;
        public TimeSpan Time { get; set; }
    }
}
