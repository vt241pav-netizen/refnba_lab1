using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("TEAMS")]
public partial class Team
{
    [Key]
    [Column("TEAM_ID")]
    public int TeamId { get; set; }

    [Column("ARENA_ID")]
    public int ArenaId { get; set; }

    [Column("DIVISION_ID")]
    public int DivisionId { get; set; }

    [Column("TEAM_NAME")]
    [StringLength(50)]
    public string TeamName { get; set; } = null!;

    [Column("ABBREVIATION")]
    [StringLength(50)]
    public string Abbreviation { get; set; } = null!;

    [Column("YEAR_FOUNDED")]
    public DateOnly? YearFounded { get; set; }

    [Column("GENERAL_MANAGER")]
    [StringLength(50)]
    public string? GeneralManager { get; set; }

    [Column("CONFERENCE_ID")]
    public int ConferenceId { get; set; }

    [ForeignKey("ArenaId")]
    [InverseProperty("Teams")]
    public virtual Arena Arena { get; set; } = null!;

    [InverseProperty("Team")]
    public virtual ICollection<Coach> Coaches { get; set; } = new List<Coach>();

    [ForeignKey("DivisionId")]
    [InverseProperty("Teams")]
    public virtual Division Division { get; set; } = null!;

    [InverseProperty("Team")]
    public virtual ICollection<Player> Players { get; set; } = new List<Player>();

    public bool IsDeleted { get; set; }
}
