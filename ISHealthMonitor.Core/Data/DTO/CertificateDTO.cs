﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Models.DTO
{
    public class CertificateDTO 
    {
        public string EffectiveDate { get; set; }
        public string ExpDate { get; set; }
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public string CommonName { get; set; }
        public bool ErrorCommonName { get; set; }
        public string Thumbprint { get; set; }
    }
}
