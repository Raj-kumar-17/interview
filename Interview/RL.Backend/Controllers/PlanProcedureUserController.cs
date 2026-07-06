using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Commands;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class PlanProcedureUserController : ControllerBase
{
    private readonly RLContext _context;
    private readonly IMediator _mediator;

    public PlanProcedureUserController(RLContext context, IMediator mediator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [EnableQuery]
    public IQueryable<PlanProcedureUser> Get()
    {
        return _context.PlanProcedureUsers.Include(p => p.User);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddPlanProcedureUserCommand command, CancellationToken token)
    {
        var response = await _mediator.Send(command, token);
        return response.ToActionResult();
    }

    [HttpDelete]
    public async Task<IActionResult> Remove([FromQuery] int planId, [FromQuery] int procedureId, [FromQuery] int userId, CancellationToken token)
    {
        var response = await _mediator.Send(new RemovePlanProcedureUserCommand
        {
            PlanId = planId,
            ProcedureId = procedureId,
            UserId = userId
        }, token);

        return response.ToActionResult();
    }

    [HttpDelete("RemoveAll")]
    public async Task<IActionResult> RemoveAll([FromQuery] int planId, [FromQuery] int procedureId, CancellationToken token)
    {
        var response = await _mediator.Send(new RemoveAllPlanProcedureUsersCommand
        {
            PlanId = planId,
            ProcedureId = procedureId
        }, token);

        return response.ToActionResult();
    }
}
