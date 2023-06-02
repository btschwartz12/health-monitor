using ISHealthMonitor.Core.Data.DTO;
using Microsoft.EntityFrameworkCore;

public class DatawarehouseContext : DbContext
{
	public DatawarehouseContext()
	{
	}



	public DatawarehouseContext(DbContextOptions<DatawarehouseContext> options)
		: base(options)
	{
	}
	public DbSet<EmployeeDTO> Employee { get; set; }

}