using Microsoft.AspNetCore.Mvc;
using Planera.Data;
using Planera.Data.Dto;
using Planera.Extensions;
using Planera.Models;
using Planera.Models.Ticket;
using Planera.Services;

namespace Planera.Controllers;

[ApiController]
[Route("tickets")]
public class TicketController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost("{projectId}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(int projectId, [FromBody] CreateTicketModel model)
    {
        var result = await _ticketService.AddTicketAsync(
            User.FindFirst("Id")!.Value,
            projectId,
            model.Title,
            model.Description,
            model.Priority,
            model.AssigneeIds
        );

        return result.ToActionResult();
    }

    [HttpPatch("{projectId}/{ticketId}")]
    public async Task<IActionResult> SetStatus(int projectId, int ticketId, TicketStatus status)
    {
        var result = await _ticketService.SetStatus(
            User.FindFirst("Id")!.Value,
            projectId,
            ticketId,
            status
        );

        return result.ToActionResult();
    }
}