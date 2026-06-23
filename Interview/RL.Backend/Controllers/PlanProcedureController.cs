using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class PlanProcedureController : ControllerBase
{
    private readonly ILogger<PlanProcedureController> _logger;
    private readonly RLContext _context;

    public PlanProcedureController(ILogger<PlanProcedureController> logger, RLContext context)
    {
        _logger = logger;
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [EnableQuery]
    public IQueryable<PlanProcedure> Get()
    {
        return _context.PlanProcedures
            .Include(pp => pp.Procedure)
            .Include(pp => pp.PlanProcedureUsers)
                .ThenInclude(pu => pu.User);
    }
}
