using ISHealthMonitor.Core.Contracts;
using ISHealthMonitor.Core.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Implementations
{
    public class Employee : IEmployee
    {
        private readonly DatawarehouseContext _datawarehouseContext;



        public Employee(DatawarehouseContext context)
        {
            _datawarehouseContext = context;
        }


        public EmployeeDTO GetEmployeeByUserName(string userName)
        {
            var emp = _datawarehouseContext.Employee.Where(x => x.NetworkLogon == userName).FirstOrDefault();

            var employeeDTO = new EmployeeDTO()

            {
                Company = emp.Company.Trim(),
                Department = emp.Department.Trim(),
                DisplayName = emp.DisplayName.Trim(),
                Email = emp.Email.Trim(),
                FirstName = emp.FirstName.Trim(),
                GUID = emp.GUID.Trim(),
                LastName = emp.LastName.Trim(),
                Manager = emp.Manager.Trim(),
                NetworkLogon = emp.NetworkLogon.Trim(),
                Office = emp.Office.Trim(),
                Title = emp.Title.Trim()
            };

            return employeeDTO;
        }

        public List<EmployeeDTO> GetAll()
        {
            var employees = _datawarehouseContext.Employee
                .Where(x => x.empid > 0)
                .Select(emp => new EmployeeDTO
                {
                    Company = emp.Company.Trim(),
                    Department = emp.Department.Trim(),
                    DisplayName = emp.DisplayName.Trim(),
                    Email = emp.Email.Trim(),
                    FirstName = emp.FirstName.Trim(),
                    GUID = emp.GUID.Trim(),
                    LastName = emp.LastName.Trim(),
                    Manager = emp.Manager.Trim(),
                    NetworkLogon = emp.NetworkLogon.Trim(),
                    Office = emp.Office.Trim(),
                    Title = emp.Title.Trim()
                })
                .ToList();

            return employees;
        }

		public string GetEmailByGuid(Guid guid)
		{
            var guidStr = guid.ToString();

			var email = _datawarehouseContext.Employee.Where(x => x.GUID.Trim() == guidStr).Select(x => x.Email.Trim()).FirstOrDefault();

            if (email == null)
            {
                throw new Exception("No employee email found for " + guidStr);
            }

            return email;
		}

        public EmployeeDTO GetEmployeeByGuid(Guid guid)
        {
            var guidStr = guid.ToString();
            var emp = _datawarehouseContext.Employee.Where(x => x.GUID.Trim() == guidStr).FirstOrDefault();

            var employeeDTO = new EmployeeDTO()

            {
                Company = emp.Company.Trim(),
                Department = emp.Department.Trim(),
                DisplayName = emp.DisplayName.Trim(),
                Email = emp.Email.Trim(),
                FirstName = emp.FirstName.Trim(),
                GUID = emp.GUID.Trim(),
                LastName = emp.LastName.Trim(),
                Manager = emp.Manager.Trim(),
                NetworkLogon = emp.NetworkLogon.Trim(),
                Office = emp.Office.Trim(),
                Title = emp.Title.Trim()
            };

            return employeeDTO;
        }
    }
}
