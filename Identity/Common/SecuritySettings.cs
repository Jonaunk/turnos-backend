﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Common
{
    public class SecuritySettings
    {
        public string? Provider { get; set; }
        public bool RequireConfirmedAccount { get; set; }
    }
}
