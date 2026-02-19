using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("DIVISIONS")]
public partial class Division
{
    [Key]
    [Column("DIVISION_ID")]
    public int DivisionId { get; set; }

    [Column("CONFERENCE_ID")]
    public int ConferenceId { get; set; }

    [Column("DIVISION_NAME")]
    [StringLength(50)]
    public string DivisionName { get; set; } = null!;

    [ForeignKey("ConferenceId")]
    [InverseProperty("Divisions")]
    public virtual Conference Conference { get; set; } = null!;

    [InverseProperty("Division")]
    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
