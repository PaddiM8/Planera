using Microsoft.AspNetCore.Mvc;
using Planera.Data.Dto;
using Planera.Extensions;
using Planera.Models.Ticket;
using Planera.Services;

namespace Planera.Controllers;

[ApiController]
[Route("notes")]
public class NoteController : ControllerBase
{
    private readonly NoteService _noteService;

    public NoteController(NoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpGet("{projectId}/{ticketId}")]
    [ProducesResponseType(typeof(ICollection<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int projectId, int ticketId)
    {
        var result = await _noteService.GetAllAsync(
            User.FindFirst("Id")!.Value,
            ticketId, projectId
        );

        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteModel model)
    {
        var result = await _noteService.AddAsync(
            User.FindFirst("Id")!.Value,
            model.ProjectId,
            model.TicketId, model.Content
        );

        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _noteService.RemoveAsync(
            User.FindFirst("Id")!.Value,
            id
        );

        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Edit(int id, [FromBody] EditNoteModel model)
    {
        var result = await _noteService.EditAsync(
            User.FindFirst("Id")!.Value,
            id,
            model.Content,
            model.Status
        );

        return result.ToActionResult();
    }
}