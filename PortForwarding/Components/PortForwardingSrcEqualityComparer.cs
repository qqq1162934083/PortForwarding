using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortForwarding
{
    public class PortForwardingSrcEqualityComparer : IEqualityComparer<PortForwardingMappingModel>
    {
        public bool Equals(PortForwardingMappingModel x, PortForwardingMappingModel y)
        {
            return x.SrcIpAddr == y.SrcIpAddr && x.SrcPort == y.SrcPort;
        }

        public int GetHashCode(PortForwardingMappingModel obj)
        {
            return -1;
        }
    }
}
