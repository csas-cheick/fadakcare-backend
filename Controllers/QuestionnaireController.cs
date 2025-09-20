using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Services.IServices;
using backend.Dtos.Questionnaire;

namespace backend.Controllers
{
    [Route("api/admin/depistage")]
    [ApiController]
    [Authorize(Roles="admin")]
    public class QuestionnaireController : ControllerBase
    {
        private readonly IQuestionnaireService _questionnaireService;

        public QuestionnaireController(IQuestionnaireService questionnaireService)
        {
            _questionnaireService = questionnaireService;
        }

    [HttpPost("creationQuestionnaire")]
    [Authorize(Roles="admin")]
        public async Task<IActionResult> CreateQuestionnaire([FromBody] CreateQuestionnaireDto dto)
        {
            var result = await _questionnaireService.CreateQuestionnaire(dto);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(new { message = result.Message, id = result.QuestionnaireId });
        }

    [HttpGet("liste")]
    [Authorize(Roles="admin")]
        public async Task<ActionResult<IEnumerable<QuestionnaireDto>>> GetQuestionnaires()
        {
            var questionnaires = await _questionnaireService.GetQuestionnaires();
            return Ok(questionnaires);
        }

    [HttpDelete("{id}")]
    [Authorize(Roles="admin")]
        public async Task<IActionResult> DeleteQuestionnaire(int id)
        {
            var result = await _questionnaireService.DeleteQuestionnaire(id);

            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(new { message = result.Message });
        }

    [HttpPut("{id}")]
    [Authorize(Roles="admin")]
        public async Task<IActionResult> UpdateQuestionnaire(int id, [FromBody] CreateQuestionnaireDto dto)
        {
            var result = await _questionnaireService.UpdateQuestionnaire(id, dto);

            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(new { message = result.Message });
        }
    }
}
