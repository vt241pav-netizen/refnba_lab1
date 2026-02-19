namespace NBA.EFCore.DTOs
{
    public class TeamStatsDto
    {
        public string TeamName { get; set; } = null!;
        public int PlayerCount { get; set; }
        public double AverageHeight { get; set; }
        public int TotalMatches { get; set; }
        public double AveragePointsScored { get; set; }
        public double AveragePointsConceded { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }
     public class TeamDetailedStatsDto
    {
        public string TeamName { get; set; } = null!;
        public string ArenaName { get; set; } = null!;
        public string DivisionName { get; set; } = null!;
        public int PlayerCount { get; set; }
        public double AverageHeight { get; set; }
        public int TotalMatches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinPercentage { get; set; }
        public int TotalPointsScored { get; set; }
        public int TotalPointsConceded { get; set; }
        public double AveragePointsScored { get; set; }
        public double AveragePointsConceded { get; set; }
    }
}