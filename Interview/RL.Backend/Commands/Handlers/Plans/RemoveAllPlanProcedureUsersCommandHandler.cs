using MediatR;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Commands.Handlers.Plans;

public class RemoveAllPlanProcedureUsersCommandHandler : IRequestHandler<RemoveAllPlanProcedureUsersCommand, ApiResponse<Unit>>
{
    private readonly RLContext _context;

    public RemoveAllPlanProcedureUsersCommandHandler(RLContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<Unit>> Handle(RemoveAllPlanProcedureUsersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.PlanId < 1 || request.ProcedureId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid plan or procedure id."));

            var assignments = _context.PlanProcedureUsers.Where(pu => pu.PlanId == request.PlanId && pu.ProcedureId == request.ProcedureId);
            _context.PlanProcedureUsers.RemoveRange(assignments);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<Unit>.Succeed(new Unit());
        }
        catch (Exception e)
        {
            return ApiResponse<Unit>.Fail(e);
        }
    }
}
