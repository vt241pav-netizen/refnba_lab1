using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NBA.EFCore.EFModels;

[Keyless]
public partial class VwPlayerTeam
{
    [Column("PLAYER_ID")]
    public int PlayerId { get; set; }

    [Column("FIRST_NAME")]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Column("LAST_NAME")]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [Column("TEAM_NAME")]
    [StringLength(50)]
    public string TeamName { get; set; } = null!;

    [Column("ARENA_NAME")]
    [StringLength(50)]
    public string ArenaName { get; set; } = null!;

    [Column("CITY")]
    [StringLength(50)]
    public string City { get; set; } = null!;
}
