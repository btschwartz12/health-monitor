using ISHealthMonitor.Core.Data.DbSet;
using ISHealthMonitor.Core.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Contracts
{
   public interface IHealthModel
    {
        List<CertificateDTO> GetSites();
    }
}
