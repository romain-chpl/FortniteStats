using FortniteStatsDesktop.Models;

namespace FortniteStatsDesktop.Services
{
    /// <summary>
    /// Service permettant de partager un match spécifique entre les pages (ex: pour l'aperçu).
    /// </summary>
    public class MatchStateService
    {
        public ParsedMatchData? CurrentMatch { get; set; }
        public bool IsPreview { get; set; }

        public void SetMatch(ParsedMatchData match, bool isPreview = false)
        {
            CurrentMatch = match;
            IsPreview = isPreview;
        }

        public void Clear()
        {
            CurrentMatch = null;
            IsPreview = false;
        }
    }
}
