using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortForwarding
{
    public class PortForwardingMappingModel
    {
        public string SrcIpAddr { get; set; }
        public int SrcPort { get; set; }
        public string DestIpAddr { get; set; }
        public int DestPort { get; set; }
    }
}
