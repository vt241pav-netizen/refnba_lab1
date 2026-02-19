using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Table("ARENAS")]
public partial class Arena
{
    [Key]
    [Column("ARENA_ID")]
    public int ArenaId { get; set; }

    [Column("ARENA_NAME")]
    [StringLength(50)]
    public string ArenaName { get; set; } = null!;

    [Column("CITY")]
    [StringLength(50)]
    public string City { get; set; } = null!;

    [Column("CAPACITY")]
    public int Capacity { get; set; }

    [InverseProperty("Arena")]
    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
