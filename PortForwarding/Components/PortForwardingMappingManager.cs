using MyTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PortForwarding
{
    /// <summary>
    /// 端口映射管理器
    /// </summary>
    public class PortForwardingMappingManager
    {
        private PortForwardingMappingComparer MappingComparer { get; set; }
        private PortForwardingSrcMappingSrcComparer SrcMappingComparer { get; set; }
        private PortForwardingDestMappingComparer DestMappingComparer { get; set; }
        public List<PortForwardingMappingModel> MappingList { get; private set; } = new List<PortForwardingMappingModel>();
        public event Action<List<PortForwardingMappingModel>> MappingListChanged;
        public event Action<string> ConsoleOutput;
        public PortForwardingMappingManager()
        {
            MappingComparer = new PortForwardingMappingComparer();
            SrcMappingComparer = new PortForwardingSrcMappingSrcComparer();
            DestMappingComparer = new PortForwardingDestMappingComparer();
        }
        /// <summary>
        /// 添加一个端口映射
        /// </summary>
        /// <param name="model"></param>
        public void AddMapping(PortForwardingMappingModel model)
        {
            AddMapping(model.SrcIpAddr, model.SrcPort, model.DestIpAddr, model.DestPort);
        }

        /// <summary>
        /// 添加一个端口映射
        /// </summary>
        /// <param name="model"></param>
        public void AddMapping(string srcIpAddr, int srcPort, string destIpAddr, int destPort)
        {
            var cmd = $"netsh interface portproxy add v4tov4 listenaddress={srcIpAddr} listenport={srcPort} connectaddress={destIpAddr} connectport={destPort}";
            var result = ConsoleUtils.GetCmdResult(cmd);
            ConsoleOutput?.Invoke(result);
        }

        /// <summary>
        /// 移除一个端口映射
        /// </summary>
        /// <param name="model"></param>
        public void RemoveMapping(PortForwardingMappingModel model)
        {
            RemoveMapping(model.SrcIpAddr, model.SrcPort);
        }
        /// <summary>
        /// 移除一个端口映射
        /// </summary>
        /// <param name="model"></param>
        public void RemoveMapping(string srcIpAddr, int srcPort)
        {
            var cmd = $"netsh interface portproxy delete v4tov4 listenaddress={srcIpAddr} listenport={srcPort}";
            var result = ConsoleUtils.GetCmdResult(cmd);
            ConsoleOutput?.Invoke(result);
        }

        /// <summary>
        /// 刷新映射
        /// </summary>
        public void Refresh()
        {
            Load();
        }

        /// <summary>
        /// 加载映射到内存
        /// 只加载变化部分
        /// 排序方式一样
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void Load()
        {
            const string cmd = "netsh interface portproxy show v4tov4";
            var result = ConsoleUtils.GetCmdResult(cmd);
            ConsoleOutput?.Invoke(result);
            var lines = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(3).Select(x => x.Trim()).ToArray();
            var mappingList = new List<PortForwardingMappingModel>();
            var index = 0;
            foreach (var line in lines)
            {
                var items = Regex.Split(line, "\\s+");
                if (items.Length != 4)
                    throw new Exception($"索引为{index}的项:{line},不能匹配规则");
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
                index++;
            }

            //比较内存中的信息,更新发生变动的信息
            var valueCount = 0;
            for (int i = 0; i < mappingList.Count; i++)
            {
                var currMapping = mappingList[i];
                var mappingValue = MappingList.FirstOrDefault(x => MappingComparer.Equals(x, currMapping));
                if (mappingValue != null)
                {
                    mappingList[i] = mappingValue;
                    valueCount++;
                }
            }
            var listChanged = MappingList.Count == valueCount;
            MappingList = mappingList;
            if (listChanged)
            {
                //通知集合更新
                MappingListChanged?.Invoke(MappingList);
            }
        }
    }
    public class PortForwardingMappingComparer : IEqualityComparer<PortForwardingMappingModel>
    {

        /// <summary>
        /// 判断两个映射是否是相同的
        /// </summary>
        /// <param name="mapping1"></param>
        /// <param name="mapping2"></param>
        /// <returns></returns>
        public static bool IsSameMapping(PortForwardingMappingModel mapping1, PortForwardingMappingModel mapping2)
        {
            return IsSameSrcMapping(mapping1, mapping2) &&
            IsSameDestMapping(mapping1, mapping2);
        }

        /// <summary>
        /// 只判断两个映射的源地址和源端口是否相同
        /// </summary>
        /// <param name="mapping1"></param>
        /// <param name="mapping2"></param>
        /// <returns></returns>
        public static bool IsSameSrcMapping(PortForwardingMappingModel mapping1, PortForwardingMappingModel mapping2)
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
        public static bool IsSameDestMapping(PortForwardingMappingModel mapping1, PortForwardingMappingModel mapping2)
        {
            return mapping1.DestIpAddr == mapping2.DestIpAddr &&
            mapping1.DestPort == mapping2.DestPort;
        }

        public virtual bool Equals(PortForwardingMappingModel x, PortForwardingMappingModel y)
        {
            return IsSameMapping(x, y);
        }

        public int GetHashCode(PortForwardingMappingModel obj)
        {
            throw new NotImplementedException();
        }
    }
    public class PortForwardingSrcMappingSrcComparer : PortForwardingMappingComparer
    {
        public override bool Equals(PortForwardingMappingModel x, PortForwardingMappingModel y)
        {
            return IsSameSrcMapping(x, y);
        }
    }
    public class PortForwardingDestMappingComparer : PortForwardingMappingComparer
    {
        public override bool Equals(PortForwardingMappingModel x, PortForwardingMappingModel y)
        {
            return IsSameDestMapping(x, y);
        }
    }
}
