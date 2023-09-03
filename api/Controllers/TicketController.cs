using Microsoft.AspNetCore.Mvc;
using Planera.Data;
using Planera.Data.Dto;
using Planera.Extensions;
using Planera.Models.Ticket;
using Planera.Services;

namespace Planera.Controllers;

[ApiController]
[Route("tickets")]
public class TicketController : ControllerBase
{
    private const long TICKET_SIZE_LIMIT = 10000000;

    private readonly TicketService _ticketService;

    public TicketController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("{username}/{slug}/{ticketId}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string username, string slug, int ticketId)
    {
        var result = await _ticketService.GetAsync(
            User.FindFirst("Id")!.Value,
            username,
            slug,
            ticketId
        );

        return result.ToActionResult();
    }

    [HttpGet("{username}/{slug}")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(string username, string slug, int startIndex, int amount) {
        var result = await _ticketService.GetAllAsync(
            User.FindFirst("Id")!.Value,
            username,
            slug,
            startIndex,
            amount
        );

        return result.ToActionResult();
    }

    [HttpGet("{username}/{slug}/query")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        string username,
        string slug,
        int startIndex,
        int amount,
        TicketSorting sorting,
        TicketFilter? filter)
    {
        var result = await _ticketService.GetAllAsync(
            User.FindFirst("Id")!.Value,
            username,
            slug,
            startIndex,
            amount,
            searchQuery: null,
            sorting,
            filter
        );

        return result.ToActionResult();
    }

    [HttpPost("{projectId}")]
    [RequestSizeLimit(TICKET_SIZE_LIMIT)]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(string projectId, [FromBody] CreateTicketModel model)
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

    [HttpPost("{projectId}/{ticketId}")]
    public async Task<IActionResult> Edit(string projectId, int ticketId, [FromBody] EditTicketModel model)
    {
        var result = await _ticketService.EditTicketAsync(
            User.FindFirst("Id")!.Value,
            projectId,
            ticketId,
            model.Title,
            model.Description
        );

        return result.ToActionResult();
    }

    [HttpDelete("{projectId}/{ticketId}")]
    public async Task<IActionResult> Delete(string projectId, int ticketId)
    {
        var result = await _ticketService.RemoveTicketAsync(
            User.FindFirst("Id")!.Value,
            projectId,
            ticketId
        );

        return result.ToActionResult();
    }

    [HttpPatch("{projectId}/{ticketId}/status")]
    public async Task<IActionResult> SetStatus(string projectId, int ticketId, TicketStatus status)
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