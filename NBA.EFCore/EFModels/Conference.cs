using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("CONFERENCES")]
public partial class Conference
{
    [Key]
    [Column("CONFERENCE_ID")]
    public int ConferenceId { get; set; }

    [Column("CONFERENCE_NAME")]
    [StringLength(100)]
    public string ConferenceName { get; set; } = null!;

    [InverseProperty("Conference")]
    public virtual ICollection<Division> Divisions { get; set; } = new List<Division>();
}
