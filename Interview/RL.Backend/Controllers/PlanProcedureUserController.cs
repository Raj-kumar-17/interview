using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class PlanProcedureUserController : ControllerBase
{
    private readonly ILogger<PlanProcedureUserController> _logger;
    private readonly RLContext _context;

    public PlanProcedureUserController(ILogger<PlanProcedureUserController> logger, RLContext context)
    {
        _logger = logger;
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [EnableQuery]
    public IQueryable<PlanProcedureUser> Get()
    {
        return _context.PlanProcedureUsers.Include(p => p.User);
    }

    [HttpPost]
    public async Task<IActionResult> Add(PlanProcedureUser command, CancellationToken token)
    {
        if (command.PlanId < 1 || command.ProcedureId < 1 || command.UserId < 1)
        {
            return BadRequest("Invalid plan, procedure or user id.");
        }

        var exists = await _context.PlanProcedures.AnyAsync(pp => pp.PlanId == command.PlanId && pp.ProcedureId == command.ProcedureId, token);
        if (!exists)
        {
            return NotFound($"PlanProcedure not found for plan {command.PlanId} and procedure {command.ProcedureId}.");
        }

        var user = await _context.Users.FindAsync(new object[] { command.UserId }, token);
        if (user is null)
        {
            return NotFound($"UserId: {command.UserId} not found");
        }

        var existingAssignment = await _context.PlanProcedureUsers.FindAsync(new object[] { command.PlanId, command.ProcedureId, command.UserId }, token);
        if (existingAssignment is not null)
        {
            return Ok();
        }

        _context.PlanProcedureUsers.Add(command);
        await _context.SaveChangesAsync(token);
        return Ok(command);
    }

    [HttpDelete]
    public async Task<IActionResult> Remove([FromQuery] int planId, [FromQuery] int procedureId, [FromQuery] int userId, CancellationToken token)
    {
        if (planId < 1 || procedureId < 1 || userId < 1)
        {
            return BadRequest("Invalid plan, procedure or user id.");
        }

        var assignment = await _context.PlanProcedureUsers.FindAsync(new object[] { planId, procedureId, userId }, token);
        if (assignment is null)
        {
            return NotFound();
        }

        _context.PlanProcedureUsers.Remove(assignment);
        await _context.SaveChangesAsync(token);
        return Ok();
    }

    [HttpDelete("RemoveAll")]
    public async Task<IActionResult> RemoveAll([FromQuery] int planId, [FromQuery] int procedureId, CancellationToken token)
    {
        if (planId < 1 || procedureId < 1)
        {
            return BadRequest("Invalid plan or procedure id.");
        }

        var assignments = _context.PlanProcedureUsers.Where(pu => pu.PlanId == planId && pu.ProcedureId == procedureId);
        _context.PlanProcedureUsers.RemoveRange(assignments);
        await _context.SaveChangesAsync(token);
        return Ok();
    }
}
