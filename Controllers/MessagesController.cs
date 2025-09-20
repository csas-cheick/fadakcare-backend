using backend.Dtos.Message;
using backend.IServices;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost("envoyer")]
    public async Task<IActionResult> EnvoyerMessage([FromBody] MessageDto dto)
    {
        await _messageService.EnvoyerMessageAsync(dto.ExpediteurId, dto.DestinataireId, dto.Contenu);
        return Ok();
    }

    [HttpGet("conversation/{contactId:int}")]
    public async Task<IActionResult> GetConversation(int contactId, [FromQuery] int userId)
    {
        var result = await _messageService.GetConversationAsync(userId, contactId);
        return Ok(result);
    }

    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts([FromQuery] int userId)
    {
        var contacts = await _messageService.GetContactsAsync(userId);
        return Ok(contacts);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var result = await _messageService.DeleteMessageAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMessage(int id, [FromBody] MessageUpdateDto dto)
    {
        var result = await _messageService.UpdateMessageAsync(id, dto.Contenu);
        if (!result)
            return NotFound();

        return NoContent();
    }

}
