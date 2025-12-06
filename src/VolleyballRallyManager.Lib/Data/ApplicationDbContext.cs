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
        public DbSet<MatchSet> MatchSets { get; set; }
        public DbSet<Bulletin> Bulletins { get; set; }
        public DbSet<MatchUpdate> MatchUpdates { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentDivision> TournamentDivisions { get; set; }
        public DbSet<TournamentTeamDivision> TournamentTeamDivisions { get; set; }
        public DbSet<TournamentRound> TournamentRounds { get; set; }
        public DbSet<TournamentRoundTeam> TournamentRoundTeams { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementHistoryLog> AnnouncementHistoryLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TournamentDivision and TournamentTeamDivision use single TournamentRoundId primary keys (inherited from BaseEntity)

            builder.Entity<TournamentTeamDivision>()
                .HasOne(ttd => ttd.Tournament)
                .WithMany(t => t.TournamentTeamDivisions)
                .HasForeignKey(ttd => ttd.TournamentId);

            /*builder.Entity<TournamentTeamDivision>()
                .HasOne(ttd => ttd.Team)
                .WithMany(t => t.TournamentTeamDivisions)
                .HasForeignKey(ttd => ttd.TeamId);
*/
            builder.Entity<TournamentTeamDivision>()
                .HasOne(ttd => ttd.Division)
                .WithMany(d => d.TournamentTeamDivisions)
                .HasForeignKey(ttd => ttd.DivisionId);

            // Configure Team relationships
            /* builder.Entity<TournamentTeamDivision>()
                 .HasMany(t => t.HomeMatches);

             builder.Entity<TournamentTeamDivision>()
                 .HasMany(t => t.AwayMatches);
 */
            // Configure Division relationships
            builder.Entity<Division>()
                .HasMany(d => d.TournamentTeamDivisions);
            //.WithOne(t => t.Division)
            //.HasForeignKey("DivisionId")
            //.OnDelete(DeleteBehavior.Restrict);

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

            builder.Entity<Match>()
                .HasMany(m => m.Sets)
                .WithOne(ms => ms.Match)
                .HasForeignKey(ms => ms.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
            /*builder.Entity<Match>()
                            .WithOne(m => m.HomeTeam)
                            .HasForeignKey(m => m.HomeTeamId)
                            .OnDelete(DeleteBehavior.Restrict);
                            builder.Entity<Match>()
                            .WithOne(m => m.AwayTeam)
                            .HasForeignKey(m => m.AwayTeamId)
                            .OnDelete(DeleteBehavior.Restrict);*/

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

            builder.Entity<Bulletin>()
                .Property(a => a.Content)
                .IsRequired();

            builder.Entity<Announcement>()
                .Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Entity<Announcement>()
                .Property(a => a.Content)
                .IsRequired();

            builder.Entity<Announcement>()
                .HasMany(a => a.HistoryLogs)
                .WithOne(h => h.Announcement)
                .HasForeignKey(h => h.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MatchUpdate>()
                .Property(u => u.Content)
                .IsRequired();
                
        builder.Entity<Bulletin>()
            .Property(o => o.Priority)
            .HasConversion<string>(); // Tells EF Core to store as string
            
        builder.Entity<MatchUpdate>()
            .Property(o => o.UpdateType)
            .HasConversion<string>(); // Tells EF Core to store as string

        builder.Entity<Announcement>()
            .Property(o => o.Priority)
            .HasConversion<string>(); // Tells EF Core to store as string

            // Configure TournamentRound relationships
            builder.Entity<TournamentRound>()
                .HasOne(tr => tr.Tournament)
                .WithMany()
                .HasForeignKey(tr => tr.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TournamentRound>()
                .HasOne(tr => tr.Division)
                .WithMany()
                .HasForeignKey(tr => tr.DivisionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TournamentRound>()
                .HasOne(tr => tr.Round)
                .WithMany()
                .HasForeignKey(tr => tr.RoundId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TournamentRound>()
                .HasMany(tr => tr.TournamentRoundTeams)
                .WithOne(trt => trt.TournamentRound)
                .HasForeignKey(trt => trt.TournamentRoundId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TournamentRound enums as strings
            builder.Entity<TournamentRound>()
                .Property(tr => tr.AdvancingTeamSelectionStrategy)
                .HasConversion<string>();

            builder.Entity<TournamentRound>()
                .Property(tr => tr.MatchGenerationStrategy)
                .HasConversion<string>();

            // Configure TournamentRoundTeam relationships
            builder.Entity<TournamentRoundTeam>()
                .HasOne(trt => trt.Tournament)
                .WithMany()
                .HasForeignKey(trt => trt.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TournamentRoundTeam>()
                .HasOne(trt => trt.Division)
                .WithMany()
                .HasForeignKey(trt => trt.DivisionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TournamentRoundTeam>()
                .HasOne(trt => trt.Round)
                .WithMany()
                .HasForeignKey(trt => trt.RoundId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TournamentRoundTeam>()
                .HasOne(trt => trt.Team)
                .WithMany()
                .HasForeignKey(trt => trt.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
