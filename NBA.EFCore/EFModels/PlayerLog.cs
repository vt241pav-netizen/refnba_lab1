using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("PlayerLog")]
public partial class PlayerLog
{
    [Key]
    public int LogId { get; set; }

    public int? PlayerId { get; set; }

    [StringLength(20)]
    public string? Action { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActionDate { get; set; }
}
