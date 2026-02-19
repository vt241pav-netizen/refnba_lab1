using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("STATISTICS")]
[Index("MatchId", Name = "IX_STATISTICS_MATCH_ID")]
[Index("PlayerId", Name = "IX_STATISTICS_PLAYER_ID")]
public partial class Statistic
{
    [Key]
    [Column("STATS_ID")]
    public int StatsId { get; set; }

    [Column("MATCH_ID")]
    public int MatchId { get; set; }

    [Column("PLAYER_ID")]
    public int PlayerId { get; set; }

    [Column("POINTS")]
    public int? Points { get; set; }

    [Column("REBOUNDS")]
    public int? Rebounds { get; set; }

    [Column("ASSISTS")]
    public int? Assists { get; set; }

    [Column("MINUTES_PLAYED")]
    public TimeOnly? MinutesPlayed { get; set; }

    [Column("STEALS")]
    public int? Steals { get; set; }

    [Column("BLOCKS")]
    public int? Blocks { get; set; }

    [Column("TURNOVERS")]
    public int? Turnovers { get; set; }

    [ForeignKey("MatchId")]
    [InverseProperty("Statistics")]
    public virtual Match Match { get; set; } = null!;

    [ForeignKey("PlayerId")]
    [InverseProperty("Statistics")]
    public virtual Player Player { get; set; } = null!;

    public bool IsDeleted { get; set; }
}
