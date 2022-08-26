using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortForwarding
{
    public class ConfigPanelViewModel : INotifyPropertyChanged
    {
        public string SrcIpAddr { get; set; }
        public int SrcPort { get; set; }
        public string DestIpAddr { get; set; }
        public int DestPort { get; set; }
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            }
        }
        private bool _enabled;
        public ConfigPanelViewModel(PortForwardingMappingModel mappingModel)
        {
            SrcIpAddr = mappingModel.SrcIpAddr;
            SrcPort = mappingModel.SrcPort;
            DestIpAddr = mappingModel.DestIpAddr;
            DestPort = mappingModel.DestPort;
            Mapping = mappingModel;
        }
        public PortForwardingMappingModel Mapping { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
