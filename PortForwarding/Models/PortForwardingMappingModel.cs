using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortForwarding
{
    public class PortForwardingMappingModel : INotifyPropertyChanged
    {
        public string Name
        {
            get => _name; 
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        private string _name;

        public string SrcIpAddr
        {
            get => _srcIpAddr;
            set
            {
                _srcIpAddr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SrcIpAddr)));
            }
        }
        private string _srcIpAddr;

        public int SrcPort
        {
            get => _srcPort; set
            {
                _srcPort = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SrcPort)));
            }
        }
        private int _srcPort;

        public string DestIpAddr
        {
            get => _destIpAddr; set
            {
                _destIpAddr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DestIpAddr)));
            }
        }
        private string _destIpAddr;

        public int DestPort
        {
            get => _destPort; set
            {
                _destPort = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DestPort)));
            }
        }
        private int _destPort;


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
