using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NBA.EFCore.EFModels;

namespace NBA.EFCore.Data;

public partial class NbaDbContext : DbContext
{
    public NbaDbContext()
    {
    }

    public NbaDbContext(DbContextOptions<NbaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Arena> Arenas { get; set; }


    public virtual DbSet<Coach> Coaches { get; set; }

    public virtual DbSet<Conference> Conferences { get; set; }

    public virtual DbSet<Division> Divisions { get; set; }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Statistic> Statistics { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwPlayerTeam> VwPlayerTeams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=database_nba;Trusted_Connection=true;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Arena>(entity =>
        {
            entity.HasKey(e => e.ArenaId).HasName("PK_3");

            entity.ToTable("ARENAS", tb =>
                {
                    tb.HasTrigger("trg_ArenaDelete");
                    tb.HasTrigger("trg_ArenaInsert");
                });

            entity.Property(e => e.ArenaId).ValueGeneratedNever();
        });

        modelBuilder.Entity<ArenaLog>(entity =>
        {
            entity.HasKey(e => e.LogArId).HasName("PK__ArenaLog__0CBA25847EA17862");
        });

        modelBuilder.Entity<Coach>(entity =>
        {
            entity.HasKey(e => e.CoachId).HasName("PK_6");

            entity.Property(e => e.CoachId).ValueGeneratedNever();

            entity.HasOne(d => d.Team).WithMany(p => p.Coaches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_6");
        });

        modelBuilder.Entity<Conference>(entity =>
        {
            entity.HasKey(e => e.ConferenceId).HasName("PK_1");

            entity.Property(e => e.ConferenceId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Division>(entity =>
        {
            entity.HasKey(e => e.DivisionId).HasName("PK_2");

            entity.Property(e => e.DivisionId).ValueGeneratedNever();

            entity.HasOne(d => d.Conference).WithMany(p => p.Divisions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_1");
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.MatchId).HasName("PK_7");

            entity.ToTable("MATCHES", tb =>
                {
                    tb.HasTrigger("trg_DeleteMatchStats");
                    tb.HasTrigger("trg_NoNegativeScore");
                });

            entity.Property(e => e.MatchId).ValueGeneratedNever();
        });

        modelBuilder.Entity<MigrationsHistory>(entity =>
        {
            entity.HasKey(e => e.MigrationId).HasName("PK____Migrat__E5D3573B4E471BA7");

            entity.Property(e => e.AppliedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId).HasName("PK_5");

            entity.ToTable("PLAYERS", tb =>
                {
                    tb.HasTrigger("trg_PlayerDelete");
                    tb.HasTrigger("trg_PlayerInsert");
                });

            entity.Property(e => e.PlayerId).ValueGeneratedNever();

            entity.HasOne(d => d.Team).WithMany(p => p.Players)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_5");
        });


        modelBuilder.Entity<Statistic>(entity =>
        {
            entity.HasKey(e => e.StatsId).HasName("PK_8");

            entity.ToTable("STATISTICS", tb => tb.HasTrigger("trg_UpdateStatsLog"));

            entity.Property(e => e.StatsId).ValueGeneratedNever();

            entity.HasOne(d => d.Match).WithMany(p => p.Statistics)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_8");

            entity.HasOne(d => d.Player).WithMany(p => p.Statistics)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_7");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("PK_4");

            entity.Property(e => e.TeamId).ValueGeneratedNever();

            entity.HasOne(d => d.Arena).WithMany(p => p.Teams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_4");

            entity.HasOne(d => d.Division).WithMany(p => p.Teams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_3");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CB5BA1994");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<VwPlayerTeam>(entity =>
        {
            entity.ToView("vw_PlayerTeams");
        });

        OnModelCreatingPartial(modelBuilder);
    }
    
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Coach>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Match>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Statistic>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Team>().HasQueryFilter(t => !t.IsDeleted);
            
            modelBuilder.Entity<Player>()
                .Property(p => p.Position)
                .HasConversion(
                    v => v.Trim(),
                    v => v.Trim()
                );
        }
}
