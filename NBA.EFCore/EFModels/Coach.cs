using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("COACHES")]
[Index("TeamId", Name = "IX_COACHES_TEAM_ID")]
public partial class Coach
{
    [Key]
    [Column("COACH_ID")]
    public int CoachId { get; set; }

    [Column("TEAM_ID")]
    public int TeamId { get; set; }

    [Column("FIRST_NAME")]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Column("LAST_NAME")]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [Column("ROLE")]
    [StringLength(50)]
    public string Role { get; set; } = null!;

    [Column("START_DATE")]
    public DateOnly StartDate { get; set; }

    [Column("END_DATE")]
    public DateOnly EndDate { get; set; }

    [ForeignKey("TeamId")]
    [InverseProperty("Coaches")]
    public virtual Team Team { get; set; } = null!;
    public bool IsDeleted { get; set; }
}
