using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("PLAYERS")]
[Index("LastName", Name = "IX_PLAYERS_LAST_NAME")]
[Index("TeamId", Name = "IX_PLAYERS_TEAM_ID")]
public partial class Player
{
    [Key]
    [Column("PLAYER_ID")]
    public int PlayerId { get; set; }

    [Column("TEAM_ID")]
    public int TeamId { get; set; }

    [Column("FIRST_NAME")]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Column("LAST_NAME")]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [Column("POSITION")]
    [StringLength(50)]
    public string Position { get; set; } = null!;

    [Column("JERSEY_NUMBER")]
    public int JerseyNumber { get; set; }

    [Column("BIRTH_DATE")]
    public DateOnly BirthDate { get; set; }

    [Column("COUNTRY")]
    [StringLength(50)]
    public string Country { get; set; } = null!;

    [Column("HEIGHT_CM", TypeName = "decimal(4, 1)")]
    public decimal HeightCm { get; set; }

    [Column("WEIGHT_KG", TypeName = "decimal(4, 1)")]
    public decimal WeightKg { get; set; }

    [Column("DRAFT_YEAR")]
    public int DraftYear { get; set; }

    [Column("DRAFT_ROUND")]
    public int DraftRound { get; set; }

    [Column("DRAFT_PICK")]
    public int DraftPick { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Player")]
    public virtual ICollection<Statistic> Statistics { get; set; } = new List<Statistic>();

    [ForeignKey("TeamId")]
    [InverseProperty("Players")]
    public virtual Team Team { get; set; } = null!;
}
