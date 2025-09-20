using backend.Dtos.Message;
using backend.Models;

namespace backend.IServices;

public interface IMessageService
{
    Task EnvoyerMessageAsync(int expediteurId, int destinataireId, string contenu);
    Task<IEnumerable<Message>> GetConversationAsync(int userId1, int userId2);
    Task<IEnumerable<UtilisateurMessageDto>> GetContactsAsync(int userId);
    Task<bool> UpdateMessageAsync(int id, string messageNouveau);
    Task<bool> DeleteMessageAsync(int id);
}
