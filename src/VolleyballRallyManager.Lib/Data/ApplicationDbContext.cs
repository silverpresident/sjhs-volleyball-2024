using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<MatchUpdate> MatchUpdates { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentDivision> TournamentDivisions { get; set; }
        public DbSet<TournamentTeamDivision> TournamentTeamDivisions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Tournament>();

            builder.Entity<TournamentDivision>()
                .HasKey(td => new { td.TournamentId, td.DivisionId });

            builder.Entity<TournamentTeamDivision>()
                .HasKey(ttd => new { ttd.TournamentId, ttd.TeamId, ttd.DivisionId });

            builder.Entity<TournamentTeamDivision>()
                .HasOne(ttd => ttd.Tournament)
                .WithMany(t => t.TournamentTeamDivisions)
                .HasForeignKey(ttd => ttd.TournamentId);

            builder.Entity<TournamentTeamDivision>()
                .HasOne(ttd => ttd.Team)
                .WithMany(t => t.TournamentTeamDivisions)
                .HasForeignKey(ttd => ttd.TeamId);

            builder.Entity<TournamentTeamDivision>()
                .HasOne(ttd => ttd.Division)
                .WithMany(d => d.TournamentTeamDivisions)
                .HasForeignKey(ttd => ttd.DivisionId);

            // Configure Team relationships
            builder.Entity<Team>()
                .HasMany(t => t.HomeMatches)
                .WithOne(m => m.HomeTeam)
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Team>()
                .HasMany(t => t.AwayMatches)
                .WithOne(m => m.AwayTeam)
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Division relationships
            builder.Entity<Division>()
                .HasMany(d => d.Teams)
                .WithOne(t => t.Division)
                .HasForeignKey("DivisionId")
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Round relationships
            builder.Entity<Round>()
                .HasMany(r => r.Matches)
                .WithOne(m => m.Round)
                .HasForeignKey(m => m.RoundId);

            // Configure Match relationships
            builder.Entity<Match>()
                .HasMany(m => m.Updates)
                .WithOne(u => u.Match)
                .HasForeignKey(u => u.MatchId);

            // Configure required properties
            builder.Entity<Team>()
                .Property(t => t.Name)
                .IsRequired();

            builder.Entity<Division>()
                .Property(d => d.Name)
                .IsRequired();

            builder.Entity<Round>()
                .Property(r => r.Name)
                .IsRequired();

            builder.Entity<Match>()
                .Property(m => m.CourtLocation)
                .IsRequired();

            builder.Entity<Announcement>()
                .Property(a => a.Content)
                .IsRequired();

            builder.Entity<MatchUpdate>()
                .Property(u => u.Content)
                .IsRequired();
        }
    }
}
