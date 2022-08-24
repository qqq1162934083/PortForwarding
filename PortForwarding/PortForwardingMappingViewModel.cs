using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PortForwarding
{
    public enum RowType
    {
        Header,
        Data
    }
    public class PortForwardingMappingViewModel
    {
        public string Col1 { get; set; }
        public string Col2 { get; set; }
        public string Col3 { get; set; }
        public string Col4 { get; set; }
        public string Col5 { get; set; } = "开始修改";
        public string Col6 { get; set; } = "移除";
        public RowType RowType { get; set; } = RowType.Data;
        public bool Editabled { get; set; }
        public bool ReadOnly { get; set; } = true;
        public PortForwardingMappingModel MappingModel { get; set; }

        public PortForwardingMappingViewModel(PortForwardingMappingModel mappingModel)
        {
            if (mappingModel != null)
            {
                Col1 = mappingModel.SrcIpAddr;
                Col2 = mappingModel.SrcPort.ToString();
                Col3 = mappingModel.DestIpAddr;
                Col4 = mappingModel.DestPort.ToString();
            }
            MappingModel = mappingModel;
        }
    }
}
