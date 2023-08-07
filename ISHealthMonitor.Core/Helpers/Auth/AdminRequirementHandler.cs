using ISHealthMonitor.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Helpers.Auth
{
    public class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly IHealthModel _healthModel;
        private readonly IEmployee _employee;

        public AdminRequirementHandler(IHealthModel healthModel, IEmployee employee)
        {
            _healthModel = healthModel;
            _employee = employee;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            var username = context.User.Identity.Name;
            var CurrentEmployee = _employee.GetEmployeeByEmail(username);
            var GUID = new Guid(CurrentEmployee.GUID);

            if (_healthModel.UserIsAdmin(GUID))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
