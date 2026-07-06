using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.Plans;

public class AddPlanProcedureUserCommandHandler : IRequestHandler<AddPlanProcedureUserCommand, ApiResponse<Unit>>
{
    private readonly RLContext _context;

    public AddPlanProcedureUserCommandHandler(RLContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<Unit>> Handle(AddPlanProcedureUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.PlanId < 1 || request.ProcedureId < 1 || request.UserId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid plan, procedure or user id."));

            var exists = await _context.PlanProcedures.AnyAsync(pp => pp.PlanId == request.PlanId && pp.ProcedureId == request.ProcedureId, cancellationToken);
            if (!exists)
                return ApiResponse<Unit>.Fail(new NotFoundException($"PlanProcedure not found for plan {request.PlanId} and procedure {request.ProcedureId}."));

            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user is null)
                return ApiResponse<Unit>.Fail(new NotFoundException($"UserId: {request.UserId} not found"));

            var existingAssignment = await _context.PlanProcedureUsers.FindAsync(new object[] { request.PlanId, request.ProcedureId, request.UserId }, cancellationToken);
            if (existingAssignment is not null)
                return ApiResponse<Unit>.Succeed(new Unit());

            var assignment = new PlanProcedureUser
            {
                PlanId = request.PlanId,
                ProcedureId = request.ProcedureId,
                UserId = request.UserId
            };

            _context.PlanProcedureUsers.Add(assignment);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<Unit>.Succeed(new Unit());
        }
        catch (Exception e)
        {
            return ApiResponse<Unit>.Fail(e);
        }
    }
}
