using MyTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PortForwarding
{
    public class PortForwardingMappingManager
    {
        private List<PortForwardingMappingModel> PortForwardingMappingList { get; set; } = new List<PortForwardingMappingModel>();
        public event Action<List<PortForwardingMappingModel>> MappingChanged;
        public void AddMapping(PortForwardingMappingModel model)
        {

        }
        /// <summary>
        /// 加载映射到内存
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void Load()
        {
            const string cmd = "netsh interface portproxy show v4tov4";
            var result = ConsoleUtils.GetCmdResult(cmd);
            var lines = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(3).Select(x => x.Trim()).ToArray();
            var mappingList = new List<PortForwardingMappingModel>();
            var lineIndex = 0;
            foreach (var line in lines)
            {
                var items = Regex.Split(line, "\\s+");
                if (items.Length != 4)
                    throw new Exception($"索引为{lineIndex}的项:{line},不能匹配规则");
                var srcIpAddr = items[0];
                var srcPort = int.Parse(items[1]);
                var destIpAddr = items[2];
                var destPort = int.Parse(items[3]);
                var mappingModel = new PortForwardingMappingModel
                {
                    SrcIpAddr = srcIpAddr,
                    DestIpAddr = destIpAddr,
                    SrcPort = srcPort,
                    DestPort = destPort
                };
                mappingList.Add(mappingModel);
                lineIndex++;
            }

        }

        /// <summary>
        /// 判断两个映射是否是相同的
        /// </summary>
        /// <param name="mapping1"></param>
        /// <param name="mapping2"></param>
        /// <returns></returns>
        public bool IsSameMapping(PortForwardingMappingModel mapping1, PortForwardingMappingModel mapping2)
        {
            return IsSameSourceMapping(mapping1, mapping2) &&
            IsSameDestMapping(mapping1, mapping2);
        }

        /// <summary>
        /// 只判断两个映射的源地址和源端口是否相同
        /// </summary>
        /// <param name="mapping1"></param>
        /// <param name="mapping2"></param>
        /// <returns></returns>
        public bool IsSameSourceMapping(PortForwardingMappingModel mapping1, PortForwardingMappingModel mapping2)
        {
            return mapping1.SrcIpAddr == mapping2.SrcIpAddr &&
            mapping1.SrcPort == mapping2.SrcPort;
        }

        /// <summary>
        /// 只判断两个映射的目的地址和目的端口是否相同
        /// </summary>
        /// <param name="mapping1"></param>
        /// <param name="mapping2"></param>
        /// <returns></returns>
        public bool IsSameDestMapping(PortForwardingMappingModel mapping1, PortForwardingMappingModel mapping2)
        {
            return mapping1.DestIpAddr == mapping2.DestIpAddr &&
            mapping1.DestPort == mapping2.DestPort;
        }
    }
}
