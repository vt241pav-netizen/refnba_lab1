using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("ArenaLog")]
public partial class ArenaLog
{
    [Key]
    public int LogArId { get; set; }

    public int? ArenaId { get; set; }

    [StringLength(20)]
    public string? Action { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActionDate { get; set; }
}
