﻿using ISHealthMonitor.Core.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Contracts
{
	public interface IEmployee
	{
		EmployeeDTO GetEmployeeByUserName(string userName);
		EmployeeDTO GetEmployeeByGuid(Guid guid);
		EmployeeDTO GetEmployeeByEmail(string email);

        List<EmployeeDTO> GetAll();


		string GetEmailByGuid(Guid guid);


	}
}
