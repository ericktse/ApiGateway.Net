using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiGateway.Net.Model
{
    public class HostData
    {
        public string Name { get; set; }

        public ServiceRandomObject[] Hosts { get; set; }
    }
}