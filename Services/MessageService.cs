namespace backend.Services;

using backend.Data;
using backend.Dtos.Message;
using backend.IServices;
using backend.Models;
using Microsoft.EntityFrameworkCore;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;

    public MessageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task EnvoyerMessageAsync(int expediteurId, int destinataireId, string contenu)
    {
        var message = new Message
        {
            ExpediteurId = expediteurId,
            DestinataireId = destinataireId,
            Contenu = contenu,
            DateEnvoi = DateTime.Now
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Message>> GetConversationAsync(int userId1, int userId2)
    {
        return await _context.Messages
            .Where(m =>
                (m.ExpediteurId == userId1 && m.DestinataireId == userId2) ||
                (m.ExpediteurId == userId2 && m.DestinataireId == userId1))
            .OrderBy(m => m.DateEnvoi)
            .ToListAsync();
    }

    public async Task<IEnumerable<UtilisateurMessageDto>> GetContactsAsync(int userId)
    {
        var utilisateur = await _context.Utilisateur.FindAsync(userId);
        if (utilisateur == null) return [];

        if (utilisateur.Role == "doctor")
        {
            var patients = await _context.Patients
                .Where(p => p.MedecinId == userId)
                .Select(p => new UtilisateurMessageDto
                {
                    Id = p.Id,
                    Nom = p.Nom,
                    Role = "patient",
                    isOnline = p.isOnline,
                    PhotoUrl = p.PhotoUrl
                })
                .ToListAsync();

            var autresMedecins = await _context.Utilisateur
                .Where(u => u.Role == "doctor" && u.Id != userId)
                .Select(m => new UtilisateurMessageDto
                {
                    Id = m.Id,
                    Nom = m.Nom,
                    Role = "doctor",
                    isOnline = m.isOnline,
                    PhotoUrl = m.PhotoUrl
                })
                .ToListAsync();
            var Admins = await _context.Utilisateur
                .Where(u => u.Role == "admin")
                .Select(m => new UtilisateurMessageDto
                {
                    Id = m.Id,
                    Nom = m.Nom,
                    Role = "admin",
                    isOnline = m.isOnline,
                    PhotoUrl = m.PhotoUrl
                })
                .ToListAsync();

            return patients.Concat(autresMedecins).Concat(Admins).ToList();
        }
        else if (utilisateur.Role == "patient")
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == userId);
            if (patient?.MedecinId == null) return [];

            var medecin = await _context.Utilisateur
                .Where(m => m.Id == patient.MedecinId)
                .Select(m => new UtilisateurMessageDto
                {
                    Id = m.Id,
                    Nom = m.Nom,
                    Role = "doctor",
                    isOnline = m.isOnline,
                    PhotoUrl = m.PhotoUrl
                })
                .FirstOrDefaultAsync();
            var Admins = await _context.Utilisateur
                .Where(u => u.Role == "admin")
                .Select(m => new UtilisateurMessageDto
                {
                    Id = m.Id,
                    Nom = m.Nom,
                    Role = "admin",
                    isOnline = m.isOnline,
                    PhotoUrl = m.PhotoUrl
                })
                .ToListAsync();

            return medecin != null ? new[] { medecin }.Concat(Admins) : Admins;
        }

        else if (utilisateur.Role == "admin")
        {
            var Medecins = await _context.Utilisateur
                .Where(u => u.Id != userId && u.Role == "doctor")
                .Select(m => new UtilisateurMessageDto
                {
                    Id = m.Id,
                    Nom = m.Nom,
                    Role = "doctor",
                    isOnline = m.isOnline,
                    PhotoUrl = m.PhotoUrl
                })
                .ToListAsync();
            var Patient = await _context.Utilisateur
                .Where(u => u.Id != userId && u.Role == "patient")
                .Select(m => new UtilisateurMessageDto
                {
                    Id = m.Id,
                    Nom = m.Nom,
                    Role = "patient",
                    isOnline = m.isOnline,
                    PhotoUrl = m.PhotoUrl
                })
                .ToListAsync();
            var AutresAdmins = await _context.Utilisateur
                .Where(u => u.Id != userId && u.Role == "admin")
                .Select(m => new UtilisateurMessageDto
                {
                    Id = m.Id,
                    Nom = m.Nom,
                    Role = "admin",
                    isOnline = m.isOnline,
                    PhotoUrl = m.PhotoUrl
                })
                .ToListAsync();

            return Medecins.Concat(Patient).Concat(AutresAdmins).ToList();
        }

        return [];
    }
    public async Task<bool> DeleteMessageAsync(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
            return false;

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdateMessageAsync(int id, string nouveauContenu)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
            return false;
        if (message.Contenu == nouveauContenu)
            return true;

        message.Contenu = nouveauContenu;

        await _context.SaveChangesAsync();
        return true;
    }


}

