using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("MATCHES")]
[Index("AwayTeamId", Name = "IX_MATCHES_AWAY_TEAM_ID")]
[Index("GameDate", Name = "IX_MATCHES_GAME_DATE")]
[Index("HomeTeamId", Name = "IX_MATCHES_HOME_TEAM_ID")]
public partial class Match
{
    [Key]
    [Column("MATCH_ID")]
    public int MatchId { get; set; }

    [Column("SEASON")]
    [StringLength(9)]
    public string Season { get; set; } = null!;

    [Column("MATCH_TYPE")]
    [StringLength(20)]
    public string MatchType { get; set; } = null!;

    [Column("GAME_DATE", TypeName = "datetime")]
    public DateTime GameDate { get; set; }

    [Column("HOME_TEAM_ID")]
    public int HomeTeamId { get; set; }

    [Column("AWAY_TEAM_ID")]
    public int AwayTeamId { get; set; }

    [Column("HOME_TEAM_SCORE")]
    public int HomeTeamScore { get; set; }

    [Column("AWAY_TEAM_SCORE")]
    public int AwayTeamScore { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Match")]
    public virtual ICollection<Statistic> Statistics { get; set; } = new List<Statistic>();
}
