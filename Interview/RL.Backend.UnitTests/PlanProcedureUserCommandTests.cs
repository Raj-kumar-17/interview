using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.Plans;
using RL.Backend.Exceptions;
using RL.Data.DataModels;

namespace RL.Backend.UnitTests;

[TestClass]
public class PlanProcedureUserCommandTests
{
    [TestMethod]
    public async Task AddPlanProcedureUserCommandHandler_WhenValidAssignment_AddsAssignment()
    {
        var context = DbContextHelper.CreateContext();
        context.Plans.Add(new Plan { PlanId = 1 });
        context.Procedures.Add(new Procedure { ProcedureId = 2, ProcedureTitle = "Test Procedure" });
        context.PlanProcedures.Add(new PlanProcedure { PlanId = 1, ProcedureId = 2 });
        context.Users.Add(new User { UserId = 3, Name = "Test User" });
        await context.SaveChangesAsync();

        var sut = new AddPlanProcedureUserCommandHandler(context);
        var request = new AddPlanProcedureUserCommand { PlanId = 1, ProcedureId = 2, UserId = 3 };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        var savedAssignment = await context.PlanProcedureUsers.FindAsync(1, 2, 3);
        savedAssignment.Should().NotBeNull();
    }

    [TestMethod]
    [DataRow(0, 2, 3)]
    [DataRow(1, 0, 3)]
    [DataRow(1, 2, 0)]
    public async Task AddPlanProcedureUserCommandHandler_WhenIdsAreInvalid_ReturnsBadRequest(int planId, int procedureId, int userId)
    {
        var context = DbContextHelper.CreateContext();
        var sut = new AddPlanProcedureUserCommandHandler(context);
        var request = new AddPlanProcedureUserCommand { PlanId = planId, ProcedureId = procedureId, UserId = userId };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Exception.Should().BeOfType<BadRequestException>();
    }

    [TestMethod]
    public async Task AddPlanProcedureUserCommandHandler_WhenPlanProcedureDoesNotExist_ReturnsNotFound()
    {
        var context = DbContextHelper.CreateContext();
        context.Users.Add(new User { UserId = 3, Name = "Test User" });
        await context.SaveChangesAsync();

        var sut = new AddPlanProcedureUserCommandHandler(context);
        var request = new AddPlanProcedureUserCommand { PlanId = 1, ProcedureId = 2, UserId = 3 };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Exception.Should().BeOfType<NotFoundException>();
    }

    [TestMethod]
    public async Task AddPlanProcedureUserCommandHandler_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var context = DbContextHelper.CreateContext();
        context.PlanProcedures.Add(new PlanProcedure { PlanId = 1, ProcedureId = 2 });
        await context.SaveChangesAsync();

        var sut = new AddPlanProcedureUserCommandHandler(context);
        var request = new AddPlanProcedureUserCommand { PlanId = 1, ProcedureId = 2, UserId = 3 };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Exception.Should().BeOfType<NotFoundException>();
    }

    [TestMethod]
    public async Task AddPlanProcedureUserCommandHandler_WhenAssignmentAlreadyExists_ReturnsSuccessWithoutDuplicate()
    {
        var context = DbContextHelper.CreateContext();
        context.PlanProcedures.Add(new PlanProcedure { PlanId = 1, ProcedureId = 2 });
        context.Users.Add(new User { UserId = 3, Name = "Test User" });
        context.PlanProcedureUsers.Add(new PlanProcedureUser { PlanId = 1, ProcedureId = 2, UserId = 3 });
        await context.SaveChangesAsync();

        var sut = new AddPlanProcedureUserCommandHandler(context);
        var request = new AddPlanProcedureUserCommand { PlanId = 1, ProcedureId = 2, UserId = 3 };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        var assignments = await context.PlanProcedureUsers.Where(x => x.PlanId == 1 && x.ProcedureId == 2 && x.UserId == 3).ToListAsync();
        assignments.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task RemovePlanProcedureUserCommandHandler_WhenAssignmentExists_RemovesAssignment()
    {
        var context = DbContextHelper.CreateContext();
        context.PlanProcedureUsers.Add(new PlanProcedureUser { PlanId = 1, ProcedureId = 2, UserId = 3 });
        await context.SaveChangesAsync();

        var sut = new RemovePlanProcedureUserCommandHandler(context);
        var request = new RemovePlanProcedureUserCommand { PlanId = 1, ProcedureId = 2, UserId = 3 };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        var savedAssignment = await context.PlanProcedureUsers.FindAsync(1, 2, 3);
        savedAssignment.Should().BeNull();
    }

    [TestMethod]
    [DataRow(0, 2, 3)]
    [DataRow(1, 0, 3)]
    [DataRow(1, 2, 0)]
    public async Task RemovePlanProcedureUserCommandHandler_WhenIdsAreInvalid_ReturnsBadRequest(int planId, int procedureId, int userId)
    {
        var context = DbContextHelper.CreateContext();
        var sut = new RemovePlanProcedureUserCommandHandler(context);
        var request = new RemovePlanProcedureUserCommand { PlanId = planId, ProcedureId = procedureId, UserId = userId };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Exception.Should().BeOfType<BadRequestException>();
    }

    [TestMethod]
    public async Task RemovePlanProcedureUserCommandHandler_WhenAssignmentDoesNotExist_ReturnsNotFound()
    {
        var context = DbContextHelper.CreateContext();
        var sut = new RemovePlanProcedureUserCommandHandler(context);
        var request = new RemovePlanProcedureUserCommand { PlanId = 1, ProcedureId = 2, UserId = 3 };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Exception.Should().BeOfType<NotFoundException>();
    }

    [TestMethod]
    public async Task RemoveAllPlanProcedureUsersCommandHandler_WhenAssignmentsExist_RemovesAll()
    {
        var context = DbContextHelper.CreateContext();
        context.PlanProcedureUsers.AddRange(
            new PlanProcedureUser { PlanId = 1, ProcedureId = 2, UserId = 3 },
            new PlanProcedureUser { PlanId = 1, ProcedureId = 2, UserId = 4 });
        await context.SaveChangesAsync();

        var sut = new RemoveAllPlanProcedureUsersCommandHandler(context);
        var request = new RemoveAllPlanProcedureUsersCommand { PlanId = 1, ProcedureId = 2 };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        var remainingAssignments = await context.PlanProcedureUsers
            .Where(x => x.PlanId == 1 && x.ProcedureId == 2)
            .ToListAsync();

        remainingAssignments.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(0, 2)]
    [DataRow(1, 0)]
    public async Task RemoveAllPlanProcedureUsersCommandHandler_WhenIdsAreInvalid_ReturnsBadRequest(int planId, int procedureId)
    {
        var context = DbContextHelper.CreateContext();
        var sut = new RemoveAllPlanProcedureUsersCommandHandler(context);
        var request = new RemoveAllPlanProcedureUsersCommand { PlanId = planId, ProcedureId = procedureId };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Exception.Should().BeOfType<BadRequestException>();
    }

    [TestMethod]
    public async Task RemoveAllPlanProcedureUsersCommandHandler_WhenNoAssignmentsExist_ReturnsSuccess()
    {
        var context = DbContextHelper.CreateContext();
        var sut = new RemoveAllPlanProcedureUsersCommandHandler(context);
        var request = new RemoveAllPlanProcedureUsersCommand { PlanId = 1, ProcedureId = 2 };

        var result = await sut.Handle(request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
    }
}
