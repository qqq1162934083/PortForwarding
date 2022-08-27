using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PortForwarding
{
    public class PortForwardingMappingViewModel : INotifyPropertyChanged
    {
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
            get => _srcPort;
            set
            {
                _srcPort = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SrcPort)));
            }
        }
        private int _srcPort;

        public string DestIpAddr
        {
            get => _destIpAddr;
            set
            {
                _destIpAddr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DestIpAddr)));
            }
        }
        private string _destIpAddr;

        public int DestPort
        {
            get => _destPort;
            set
            {
                _destPort = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DestPort)));
            }
        }
        private int _destPort;

        public bool Editing
        {
            get => _editing;
            set
            {
                _editing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Editing)));
            }
        }
        private bool _editing;

        public event PropertyChangedEventHandler PropertyChanged;

        private PortForwardingMappingModel Mapping { get; set; }

        public PortForwardingMappingViewModel(PortForwardingMappingModel mapping)
        {
            if (mapping != null)
            {
                SrcIpAddr = mapping.SrcIpAddr;
                SrcPort = mapping.SrcPort;
                DestIpAddr = mapping.DestIpAddr;
                DestPort = mapping.DestPort;
            }
            Mapping = mapping;
        }
    }
}
