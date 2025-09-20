using Microsoft.EntityFrameworkCore;
using backend.Models.Depist.Questionnaire;
using backend.Models;
using backend.Dtos.compte;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Utilisateur> Utilisateur { get; set; }
        public DbSet<Medecin> Medecins { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Alerte> Alertes { get; set; }
        public DbSet<Conseil> Conseils { get; set; }
        public DbSet<Depistage> Depistage { get; set; }
        public DbSet<Reponse> Reponse { get; set; }
        public DbSet<ResultatIA> ResultatIA { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<RendezVous> RendezVous { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Telemedecine> Telemedecines { get; set; }
        public DbSet<ParticipantTelemedecine> ParticipantsTelemedecine { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Medecin>().ToTable("medecins");
            modelBuilder.Entity<Admin>().ToTable("admins");
            modelBuilder.Entity<Patient>().ToTable("patients");
            modelBuilder.Entity<RefreshToken>().ToTable("refreshtokens");
            modelBuilder.Entity<Utilisateur>().ToTable("utilisateur");
            modelBuilder.Entity<RendezVous>().ToTable("rendezvous");
            modelBuilder.Entity<PasswordReset>().ToTable("passwordresets");
            modelBuilder.Entity<Questionnaire>().ToTable("questionnaires");
            modelBuilder.Entity<Question>().ToTable("questions");
            modelBuilder.Entity<Alerte>().ToTable("alertes");
            modelBuilder.Entity<Conseil>().ToTable("conseils");
            modelBuilder.Entity<Depistage>().ToTable("depistage");
            modelBuilder.Entity<Reponse>().ToTable("reponse");
            modelBuilder.Entity<ResultatIA>().ToTable("resultatia");
            modelBuilder.Entity<Message>().ToTable("messages");
            modelBuilder.Entity<Notification>().ToTable("notifications");
            modelBuilder.Entity<Telemedecine>().ToTable("telemedecines");
            modelBuilder.Entity<ParticipantTelemedecine>().ToTable("participantstelemedecine");


            modelBuilder.Entity<Patient>()
               .HasOne(p => p.Medecin)
               .WithMany(m => m.Patients)
               .HasForeignKey(p => p.MedecinId);

            modelBuilder.Entity<Depistage>()
               .HasOne(d => d.Patient)
               .WithMany(p => p.Depistages)
               .HasForeignKey(d => d.PatientId);

            modelBuilder.Entity<Reponse>()
                .HasOne(r => r.Depistage)
                .WithMany(d => d.Reponses)
                .HasForeignKey(r => r.DepistageId);

            modelBuilder.Entity<Reponse>()
                .HasOne(r => r.Question)
                .WithMany()
                .HasForeignKey(r => r.QuestionId);

            modelBuilder.Entity<ResultatIA>()
                .HasOne(r => r.Depistage)
                .WithOne(d => d.ResultatIA)
                .HasForeignKey<ResultatIA>(r => r.DepistageId);

            // Configuration des relations pour Telemedecine
            modelBuilder.Entity<Telemedecine>()
                .HasOne(t => t.Createur)
                .WithMany()
                .HasForeignKey(t => t.CreateurId);

            modelBuilder.Entity<ParticipantTelemedecine>()
                .HasOne(p => p.Utilisateur)
                .WithMany()
                .HasForeignKey(p => p.UtilisateurId);

            modelBuilder.Entity<ParticipantTelemedecine>()
                .HasOne(p => p.Telemedecine)
                .WithMany(t => t.Participants)
                .HasForeignKey(p => p.TelemedicineId);
        }
    }
}