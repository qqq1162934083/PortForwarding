using Microsoft.VisualBasic.FileIO;
using MyTool;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace PortForwarding
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Version { get => Assembly.GetExecutingAssembly().GetName().Version.ToString(); set { } }

        //public List<PortForwardingMappingModel> PortForwardingMappingList
        //{
        //    get => _portForwardingMappingList;
        //    set
        //    {
        //        _portForwardingMappingList = value;
        //    }
        //}
        //private List<PortForwardingMappingModel> _portForwardingMappingList;
        //private event Action<List<PortForwardingMappingModel>> _portForwardingMappingListChanged { get;set; }

        private List<PortForwardingMappingModel> PortForwardingMappingList { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Task.Run(() =>
            {
                while (true)
                {
                    ReloadIpAddr();
                    Thread.Sleep(1000);
                }
            });
        }

        private void ReloadMappingList()
        {
            var cmd = "netsh interface portproxy show v4tov4";
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
            LoadMappingList(mappingList);

            WriteConsole(result);
        }
        private void LoadMappingList(List<PortForwardingMappingModel> mappingList)
        {
            lbx_mappingList.Items.Clear();
            lbx_mappingList.Items.Add(new PortForwardingMappingViewModel(null)
            {
                Col1 = "IP地址",
                Col2 = "端口",
                Col3 = "IP地址",
                Col4 = "端口",
                Col5 = "刷 新",
                Col6 = "新增",
                RowType = RowType.Header
            });
            if (mappingList != null)
                mappingList.ForEach(x => lbx_mappingList.Items.Add(new PortForwardingMappingViewModel(x)));

            //更新配置页面
            if (dataGrid_configPanel.Items != null && dataGrid_configPanel.Items.Count > 0)
            {
                foreach (ConfigPanelViewModel item in dataGrid_configPanel.Items)
                {
                    item.Enabled = mappingList.Any(x => x.SrcIpAddr == item.SrcIpAddr && x.SrcPort == item.SrcPort);
                }
            }
        }
        private void NewMapping(string srcIpAddr, int srcPort, string destIpAddr, int destPort)
        {
            var cmd = $"netsh interface portproxy add v4tov4 listenaddress={srcIpAddr} listenport={srcPort} connectaddress={destIpAddr} connectport={destPort}";
            var result = ConsoleUtils.GetCmdResult(cmd);
            WriteConsole(result);
        }
        private void DeleteMapping(string srcIpAddr, int srcPort)
        {
            var cmd = $"netsh interface portproxy delete v4tov4 listenaddress={srcIpAddr} listenport={srcPort}";
            var result = ConsoleUtils.GetCmdResult(cmd);
            WriteConsole(result);
        }
        private void WriteConsole(string text)
        {
            var nowTime = DateTime.Now;
            tbx_console.AppendText(nowTime.ToString("[yyyy-MM-dd HH:mm:ss fff] > ") + text + Environment.NewLine);
            tbx_console.ScrollToEnd();
        }

        private void btn_refresh_switchModify_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var viewModel = (PortForwardingMappingViewModel)btn.DataContext;
            if (viewModel.RowType == RowType.Header)
            {
                ReloadMappingList();
            }
            else
            {
                viewModel.Editabled = !viewModel.Editabled;
                for (var i = 1; i <= 4; i++)
                {
                    var textBox = VisualTreeUtils.FindChildren<TextBox>((FrameworkElement)VisualTreeHelper.GetParent(btn), "tbx_col" + i);
                    textBox.IsReadOnly = !viewModel.Editabled;
                }
                btn.Content = viewModel.Editabled ? "完成修改" : "开始修改";
                if (!viewModel.Editabled)//点击完成修改时
                {
                    var mappingModel = viewModel.MappingModel;
                    if (mappingModel != null)
                    {
                        DeleteMapping(mappingModel.SrcIpAddr, mappingModel.SrcPort);
                    }
                    mappingModel = new PortForwardingMappingModel()
                    {
                        SrcIpAddr = viewModel.Col1,
                        SrcPort = int.Parse(viewModel.Col2),
                        DestIpAddr = viewModel.Col3,
                        DestPort = int.Parse(viewModel.Col4)
                    };
                    NewMapping(mappingModel.SrcIpAddr, mappingModel.SrcPort, mappingModel.DestIpAddr, mappingModel.DestPort);
                    ReloadMappingList();
                }
            }
        }

        private void ReloadIpAddr()
        {
            var result = ConsoleUtils.GetCmdResult("ipconfig");
            var parts = result.Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
            while (parts.Count > 0 && !Regex.IsMatch(parts[0], "^.*?适配器"))
            {
                parts.RemoveAt(0);
            }
            if ((parts.Count % 2) == 0)
            {
                var mappingDic = new Dictionary<string, Dictionary<string, string>>();

                for (var i = 0; i < parts.Count; i += 2)
                {
                    var adapterName = parts[i];
                    adapterName = Regex.Match(adapterName, "(?<=适配器\\s).*?(?=:)").Value;
                    var itemDic = mappingDic[adapterName] = new Dictionary<string, string>();
                    var content = parts[i + 1];
                    var contentItems = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var contentItem in contentItems)
                    {
                        var splitIndex = contentItem.IndexOf(":");
                        var itemName = contentItem.Substring(0, splitIndex).TrimStart().TrimEnd(new char[] { '.', ' ' });
                        var itemValue = contentItem.Substring(splitIndex + 1, contentItem.Length - splitIndex - 1).Trim();
                        itemDic[itemName] = itemValue;
                    }
                }

                //加载到视图
                var ipItemKeyName = "IPv4 地址";
                var keyValuePairs = mappingDic.Where(x => x.Value.ContainsKey(ipItemKeyName)).ToList();
                var textBuilder = new StringBuilder();
                foreach (var keyValue in keyValuePairs)
                {
                    textBuilder.AppendLine($"{keyValue.Key} -> {keyValue.Value[ipItemKeyName]}");
                }
                var text = Regex.Replace(textBuilder.ToString(), Environment.NewLine + "$", x => string.Empty);
                Dispatcher.Invoke(() =>
                {
                    if (tbx_adapterMappingIp.Text != text)
                    {
                        tbx_adapterMappingIp.Text = text;
                    }
                });
            }
        }

        private void btn_new_delete(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var viewModel = (PortForwardingMappingViewModel)btn.DataContext;
            if (viewModel.RowType == RowType.Header)
            {
                var mappingModel = new PortForwardingMappingModel
                {
                    SrcIpAddr = "0.0.0.0",
                    SrcPort = 0,
                    DestIpAddr = "0.0.0.0",
                    DestPort = 0
                };
                PortForwardingMappingViewModel newItem;
                var itemIndex = 1;
                lbx_mappingList.Items.Insert(itemIndex, newItem = new PortForwardingMappingViewModel(mappingModel)
                {
                    Col5 = "完成修改",
                    Editabled = true,
                    ReadOnly = false,
                    MappingModel = null,
                });
            }
            else
            {
                var mappingModel = viewModel?.MappingModel;
                if (mappingModel != null)
                {
                    DeleteMapping(mappingModel.SrcIpAddr, mappingModel.SrcPort);
                    ReloadMappingList();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadMappingList();
        }

        private void menuItem_saveConfigAs_Click(object sender, RoutedEventArgs e)
        {
            //保存配置
            FileDialogUtils.SelectSaveFile(x => x.Filter = "配置文件|*.cfg", x =>
            {
                ReloadMappingList();
                var doYes = true;
                if (File.Exists(x.FileName))
                {
                    doYes = MsgBox.Show("该文件已存在，确定要删除该文件(移入回收站)并重新生成吗？", "重要提示", MsgBoxBtnOption.HasOkCancelBtn);
                    if (doYes) FileSystem.DeleteFile(x.FileName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }

                if (!doYes) return;

                File.Create(x.FileName).Dispose();
                var configContent = string.Join(Environment.NewLine + Environment.NewLine, GetMappingList().Select(r => $"{r.SrcIpAddr}{Environment.NewLine}{r.SrcPort}{Environment.NewLine}{r.DestIpAddr}{Environment.NewLine}{r.DestPort}"));
                //写入文件
                File.WriteAllText(x.FileName, configContent);
            });
        }

        private List<PortForwardingMappingModel> GetMappingList()
        {
            return lbx_mappingList.Items.Cast<PortForwardingMappingViewModel>().Where(r => r.MappingModel != null).Select(r => r.MappingModel).ToList();
        }

        private void menuItem_loadConfig_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                ReloadMappingList();
                var configMappings = ReadConfigMappingList(r.FileName);

                //删除所有现有映射
                var mappingList = GetMappingList();
                mappingList.ForEach(x => DeleteMapping(x.SrcIpAddr, x.SrcPort));
                //添加配置中的映射
                foreach (var mapping in configMappings)
                {
                    NewMapping(mapping.SrcIpAddr, mapping.SrcPort, mapping.DestIpAddr, mapping.DestPort);
                }
                ReloadMappingList();
            });
        }

        private List<PortForwardingMappingModel> ReadConfigMappingList(string configFilePath)
        {
            var configContent = File.ReadAllText(configFilePath);
            var configMappings = configContent.Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Select(section =>
            {
                var items = section.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                if (items.Length != 4) throw new Exception("error config");
                return new PortForwardingMappingModel()
                {
                    SrcIpAddr = items[0],
                    SrcPort = int.Parse(items[1]),
                    DestIpAddr = items[2],
                    DestPort = int.Parse(items[3])
                };
            }).ToList();
            return configMappings;
        }

        private void menuItem_appendConfig_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                ReloadMappingList();
                var configMappings = ReadConfigMappingList(r.FileName);

                //添加配置中的映射
                foreach (var mapping in configMappings)
                {
                    NewMapping(mapping.SrcIpAddr, mapping.SrcPort, mapping.DestIpAddr, mapping.DestPort);
                }
                ReloadMappingList();
            });
        }

        private void menuItem_removeConfig_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                ReloadMappingList();
                //获取当前映射
                var mappings = GetMappingList();

                //获取配置映射
                var configContent = File.ReadAllText(r.FileName);
                var configMappings = configContent.Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Select(section =>
                {
                    var items = section.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                    if (items.Length != 4) throw new Exception("error config");
                    return new PortForwardingMappingModel()
                    {
                        SrcIpAddr = items[0],
                        SrcPort = int.Parse(items[1]),
                        DestIpAddr = items[2],
                        DestPort = int.Parse(items[3])
                    };
                }).ToArray();

                //获取交集
                var toRemoveMappings = mappings.Intersect(configMappings, new PortForwardingSrcEqualityComparer()).ToArray();

                //删除映射
                foreach (var mapping in toRemoveMappings)
                {
                    DeleteMapping(mapping.SrcIpAddr, mapping.SrcPort);
                }

                ReloadMappingList();
            });
        }

        private void btn_selectConfig2ConfigPanel_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                var enabledMappingList = GetMappingList();
                var viewModelItemList = ReadConfigMappingList(r.FileName).Select(x => new ConfigPanelViewModel(x)
                {
                    Enabled = enabledMappingList.Any(y => y.SrcIpAddr == x.SrcIpAddr && y.SrcPort == x.SrcPort)
                }).ToList();
                dataGrid_configPanel.ItemsSource = new ObservableCollection<ConfigPanelViewModel>(viewModelItemList);
            });
        }

        private void btn_enabledConfigItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var data = (ConfigPanelViewModel)btn.DataContext;
            data.Enabled = !data.Enabled;
            var mapping = data.Mapping;
            if (data.Enabled)
            {
                NewMapping(mapping.SrcIpAddr, mapping.SrcPort, mapping.DestIpAddr, mapping.DestPort);
            }
            else
            {
                DeleteMapping(mapping.SrcIpAddr, mapping.SrcPort);
            }
            ReloadMappingList();
        }
    }
}
