using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("__MigrationsHistory")]
public partial class MigrationsHistory
{
    [Key]
    [StringLength(150)]
    public string MigrationId { get; set; } = null!;

    public DateTime AppliedAt { get; set; }

    [StringLength(64)]
    public string Hash { get; set; } = null!;
}
