using MediatR;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.Plans;

public class RemovePlanProcedureUserCommandHandler : IRequestHandler<RemovePlanProcedureUserCommand, ApiResponse<Unit>>
{
    private readonly RLContext _context;

    public RemovePlanProcedureUserCommandHandler(RLContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<Unit>> Handle(RemovePlanProcedureUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.PlanId < 1 || request.ProcedureId < 1 || request.UserId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid plan, procedure or user id."));

            var assignment = await _context.PlanProcedureUsers.FindAsync(new object[] { request.PlanId, request.ProcedureId, request.UserId }, cancellationToken);
            if (assignment is null)
                return ApiResponse<Unit>.Fail(new NotFoundException());

            _context.PlanProcedureUsers.Remove(assignment);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<Unit>.Succeed(new Unit());
        }
        catch (Exception e)
        {
            return ApiResponse<Unit>.Fail(e);
        }
    }
}
