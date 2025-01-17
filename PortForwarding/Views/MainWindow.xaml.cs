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
        #region 视图
        #endregion
        public string Version { get => Assembly.GetExecutingAssembly().GetName().Version.ToString(); set { } }
        public PortForwardingMappingManager MappingMgr { get; set; } = new PortForwardingMappingManager();

        public MainWindow()
        {
            InitializeComponent();
            MappingMgr.ConsoleOutput += WriteConsole;
            MappingMgr.MappingListChanged += Load_dataGrid_mappingList;
            MappingMgr.MappingListChanged += UpdateStatus_dataGrid_configPanel;
            Task.Run(() =>
            {
                while (true)
                {
                    MappingMgr.Sync();
                    Thread.Sleep(10000);
                }
            });
            Task.Run(() =>
            {
                while (true)
                {
                    ReloadIpAddr();
                    Thread.Sleep(1000);
                }
            });
        }

        private void Load_dataGrid_mappingList(List<PortForwardingMappingModel> mappingList)
        {
            Dispatcher.Invoke(() =>
            {
                dataGrid_mappingList.Items.Clear();
                foreach (var item in mappingList.Select(x => new PortForwardingMappingViewModel(x)))
                {
                    dataGrid_mappingList.Items.Add(item);
                }
            });
        }

        private void UpdateStatus_dataGrid_configPanel(List<PortForwardingMappingModel> mappingList)
        {
            Dispatcher.Invoke(() =>
            {
                dataGrid_configPanel.Items.Cast<ConfigPanelViewModel>().ToList().ForEach(item =>
                {
                    item.Enabled = MappingSrcEnabled(item.SrcIpAddr, item.SrcPort);
                });
            });
        }

        private void WriteConsole(string text)
        {
            Dispatcher.Invoke(() =>
            {
                var nowTime = DateTime.Now;
                tbx_console.AppendText(nowTime.ToString("[yyyy-MM-dd HH:mm:ss fff] > ") + text + Environment.NewLine);
                tbx_console.ScrollToEnd();
            });
        }

        private void ReloadIpAddr()
        {
            var result = ConsoleUtils.GetCmdResult("ipconfig");
            result = result.Replace("\r\n", "\n");
            var parts = result.Split(new string[] { "\n\n" }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
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
                    var contentItems = content.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
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
                var text = Regex.Replace(textBuilder.ToString(), "\n$", x => string.Empty);
                Dispatcher.Invoke(() =>
                {
                    if (tbx_adapterMappingIp.Text != text)
                    {
                        tbx_adapterMappingIp.Text = text;
                    }
                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void menuItem_saveConfigAs_Click(object sender, RoutedEventArgs e)
        {
            //保存配置
            FileDialogUtils.SelectSaveFile(x => x.Filter = "配置文件|*.cfg", x =>
            {
                var doYes = true;
                if (File.Exists(x.FileName))
                {
                    doYes = MsgBox.Show("该文件已存在，确定要删除该文件(移入回收站)并重新生成吗？", "重要提示", MsgBoxBtnOption.HasOkCancelBtn);
                    if (doYes) FileSystem.DeleteFile(x.FileName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }

                if (!doYes) return;

                File.Create(x.FileName).Dispose();
                var configContent = string.Join(Environment.NewLine + Environment.NewLine, MappingMgr.MappingList.Select(r => $"{r.SrcIpAddr}{Environment.NewLine}{r.SrcPort}{Environment.NewLine}{r.DestIpAddr}{Environment.NewLine}{r.DestPort}"));
                //写入文件
                File.WriteAllText(x.FileName, configContent);
            });
        }

        private void menuItem_loadConfig_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                var configMappings = ReadConfigMappingList(r.FileName);

                //删除所有现有映射
                var mappingList = MappingMgr.MappingList;
                mappingList.ForEach(x => MappingMgr.RemoveMapping(x.SrcIpAddr, x.SrcPort));
                //添加配置中的映射
                foreach (var mapping in configMappings)
                {
                    MappingMgr.AddMapping(mapping.SrcIpAddr, mapping.SrcPort, mapping.DestIpAddr, mapping.DestPort);
                }
                MappingMgr.Sync();
            });
        }

        private List<PortForwardingMappingModel> ReadConfigMappingList(string configFilePath)
        {
            var configContent = File.ReadAllText(configFilePath);
            configContent = configContent.Replace("\r\n", "\n");
            var configMappings = configContent.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Select(section =>
            {
                var itemList = section.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                if (itemList.Count < 4 || itemList.Count > 5) throw new Exception("error config");
                if (itemList.Count == 4) itemList.Insert(0, string.Empty);//补全
                return new PortForwardingMappingModel()
                {
                    Name = itemList[0],
                    SrcIpAddr = itemList[1],
                    SrcPort = int.Parse(itemList[2]),
                    DestIpAddr = itemList[3],
                    DestPort = int.Parse(itemList[4])
                };
            }).ToList();
            return configMappings;
        }

        private void menuItem_appendConfig_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                var configMappings = ReadConfigMappingList(r.FileName);

                //添加配置中的映射
                foreach (var mapping in configMappings)
                {
                    MappingMgr.AddMapping(mapping.SrcIpAddr, mapping.SrcPort, mapping.DestIpAddr, mapping.DestPort);
                }
                MappingMgr.Sync();
            });
        }

        private void menuItem_removeConfig_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                //获取当前映射
                var mappings = MappingMgr.MappingList;

                //获取配置映射
                var configMappings = ReadConfigMappingList(r.FileName);

                //获取交集
                var toRemoveMappings = mappings.Intersect(configMappings, new PortForwardingSrcEqualityComparer()).ToArray();

                //删除映射
                foreach (var mapping in toRemoveMappings)
                {
                    MappingMgr.RemoveMapping(mapping.SrcIpAddr, mapping.SrcPort);
                }
                MappingMgr.Sync();
            });
        }

        private void btn_selectConfig2ConfigPanel_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                var viewModelItemList = ReadConfigMappingList(r.FileName).Select(x => new ConfigPanelViewModel(x)
                {
                    Enabled = MappingSrcEnabled(x.SrcIpAddr, x.SrcPort)
                }).ToList();
                dataGrid_configPanel.ItemsSource = new ObservableCollection<ConfigPanelViewModel>(viewModelItemList);
            });
        }

        /// <summary>
        /// 判断该映射源是否已经配置
        /// </summary>
        /// <param name="srcIpAddr"></param>
        /// <param name="srcPort"></param>
        /// <returns></returns>
        private bool MappingSrcEnabled(string srcIpAddr, int srcPort)
        {
            return MappingMgr.MappingList.Any(y => PortForwardingMappingComparer.IsSameSrcMapping(y, new PortForwardingMappingModel
            {
                SrcIpAddr = srcIpAddr,
                SrcPort = srcPort
            }));
        }

        private void btn_enabledConfigItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var data = (ConfigPanelViewModel)btn.DataContext;
            data.Enabled = !data.Enabled;
            var mapping = data.Mapping;
            if (data.Enabled)
            {
                MappingMgr.AddMapping(mapping.SrcIpAddr, mapping.SrcPort, mapping.DestIpAddr, mapping.DestPort);
            }
            else
            {
                MappingMgr.RemoveMapping(mapping.SrcIpAddr, mapping.SrcPort);
            }
            MappingMgr.Sync();
        }

        /// <summary>
        /// 同步概览视图中的行数据到模型中
        /// 视图到DataContext的更新失效,原因未知,通过该方法同步值
        /// </summary>
        /// <returns></returns>
        private void SyncMappingRowViewData2Data(DependencyObject children)
        {
            var dataGridCellsPanel = VisualTreeUtils.FindFirstParent<DataGridCellsPanel>(children);
            var dataContext = dataGridCellsPanel.DataContext;
            if (!(dataContext is PortForwardingMappingViewModel)) return;
            var viewModel = (PortForwardingMappingViewModel)dataContext;
            var dataGridCells = VisualTreeUtils.FindChildrens<DataGridCell>(dataGridCellsPanel);
            viewModel.SrcIpAddr = dataGridCells[0].ChildrenAt<TextBox>(0, 0, 0, 0).Text;
            viewModel.SrcPort = int.Parse(dataGridCells[1].ChildrenAt<TextBox>(0, 0, 0, 0).Text);
            viewModel.DestIpAddr = dataGridCells[2].ChildrenAt<TextBox>(0, 0, 0, 0).Text;
            viewModel.DestPort = int.Parse(dataGridCells[3].ChildrenAt<TextBox>(0, 0, 0, 0).Text);
        }

        private void btn_mappingList_switchEditStatus_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var viewModel = (PortForwardingMappingViewModel)btn.DataContext;
            viewModel.Editing = !viewModel.Editing;

            //var dataGridCellsPanel = VisualTreeUtils.FindFirstParent<DataGridCellsPanel>(btn);
            //var dataGridCells = VisualTreeUtils.FindChildrens<DataGridCell>(dataGridCellsPanel);
            //testTextBox = dataGridCells[1].ChildrenAt<TextBox>(0, 0, 0, 0);

            VisualTreeUtils.RecursiveFindChildrens(dataGrid_mappingList, x => x is Button button &&
                (button.Name == "btn_mappingList_switchEditStatus" || button.Name == "btn_mappingList_removeItem"),
                (a, b, c) => a.Count < dataGrid_mappingList.Items.Count * 2)
                .Cast<Button>().ToList().ForEach(x => x.IsEnabled = !viewModel.Editing);
            btn_mappingList_newItem.IsEnabled = !viewModel.Editing;
            menuItem_mappingList_options.IsEnabled = !viewModel.Editing;
            btn.IsEnabled = true;

            if (!viewModel.Editing)//点击完成修改时,有可能是新增,有可能是修改
            {
                SyncMappingRowViewData2Data(btn);
                if (viewModel.IsNewData)//新增
                {
                    viewModel.IsNewData = false;
                    MappingMgr.Sync();
                    if (!MappingMgr.MappingList.Any(x => PortForwardingMappingComparer.IsSameMapping(x, new PortForwardingMappingModel
                    {
                        SrcIpAddr = viewModel.SrcIpAddr,
                        SrcPort = viewModel.SrcPort,
                        DestIpAddr = viewModel.DestIpAddr,
                        DestPort = viewModel.DestPort
                    })))
                    {
                        MappingMgr.AddMapping(viewModel.SrcIpAddr, viewModel.SrcPort, viewModel.DestIpAddr, viewModel.DestPort);
                        MappingMgr.Sync();
                    }
                    else
                    {
                        dataGrid_mappingList.Items.Remove(viewModel);
                    }
                }
                else if (viewModel.DiffFromData())//修改
                {
                    MappingMgr.RemoveMapping(viewModel.Mapping);
                    MappingMgr.AddMapping(viewModel.SrcIpAddr, viewModel.SrcPort, viewModel.DestIpAddr, viewModel.DestPort);
                    MappingMgr.Sync();
                }
            }
            else
            {
                //变灰

            }
        }

        private void btn_mappingList_refresh_Click(object sender, RoutedEventArgs e)
        {
            MappingMgr.Refresh();
            btn_mappingList_newItem.IsEnabled = true;
            menuItem_mappingList_options.IsEnabled = true;
        }

        private void btn_mappingList_removeItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var viewModel = (PortForwardingMappingViewModel)btn.DataContext;
            dataGrid_mappingList.Items.Remove(viewModel);
            SyncMappingRowViewData2Data(btn);
            MappingMgr.Sync();
            if (MappingMgr.MappingList.Any(x => PortForwardingSrcMappingComparer.IsSameSrcMapping(new PortForwardingMappingModel
            {
                SrcIpAddr = viewModel.SrcIpAddr,
                SrcPort = viewModel.SrcPort
            }, x)))
            {
                MappingMgr.RemoveMapping(viewModel.Mapping);
                MappingMgr.Sync();
            }
        }

        private void btn_mappingList_newItem_Click(object sender, RoutedEventArgs e)
        {
            PortForwardingMappingViewModel newItem = new PortForwardingMappingViewModel(new PortForwardingMappingModel
            {
                SrcIpAddr = "*",
                SrcPort = 0,
                DestIpAddr = "0.0.0.0",
                DestPort = 0
            })
            {
                IsNewData = true
            };

            dataGrid_mappingList.Items.Insert(0, newItem);


            //模拟点击
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    var handleOk = false;
                    Dispatcher.Invoke(() =>
                    {
                        var cell = (DataGridCell)VisualTreeUtils.RecursiveFindFirstChildren(dataGrid_mappingList, x => x is DataGridCell);
                        if (cell != null && cell.DataContext != null)
                        {
                            var viewModel = (PortForwardingMappingViewModel)cell.DataContext;
                            if (viewModel.IsNewData)
                            {
                                var cellPanel = VisualTreeUtils.FindFirstParent<DataGridCellsPanel>(cell);
                                var btn = VisualTreeUtils.RecursiveFindFirstChildren(cellPanel, x => x is Button button && button.Name == nameof(viewModel.btn_mappingList_switchEditStatus));
                                if (btn != null) btn_mappingList_switchEditStatus_Click(btn, null);
                                handleOk = true;
                            }
                        }
                    });
                    if (handleOk) break;
                }
            });
        }

        private void dataGrid_mappingList_item_tab_keyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                var textBox = (TextBox)sender;
                textBox.Text = textBox.Text.Replace("\t", string.Empty);
                var cell = VisualTreeUtils.FindFirstParent<DataGridCell>(textBox);
                var cellsPanel = VisualTreeUtils.FindFirstParent<DataGridCellsPanel>(textBox);
                var cells = VisualTreeUtils.FindChildrens<DataGridCell>(cellsPanel);
                var index = cells.IndexOf(cell);
                index++;
                if (index < cells.Count)
                {
                    try
                    {
                        var elem = cells[index].ChildrenAt<FrameworkElement>(0, 0, 0, 0);
                        if (elem is TextBox)
                        {
                            var textBoxElem = (TextBox)elem;
                            textBoxElem.Focus();
                            textBoxElem.SelectAll();
                        }
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                    }
                }
            }
        }

        private void HandleTest(object sender, RoutedEventArgs e)
        {
            var btn = (Button)VisualTreeUtils.RecursiveFindFirstChildren(dataGrid_mappingList, x => x is Button && ((Button)x).Name == "btn_mappingList_removeItem");
            Console.WriteLine();
        }

        private void test(object sender, DependencyPropertyChangedEventArgs e)
        {
            //将视图绑定到viewModel
            var rowPanelList = VisualTreeUtils.RecursiveFindChildrens(dataGrid_mappingList, x => x is DataGridCellsPanel, (a, b, c) => a.Count == dataGrid_mappingList.Items.Count)
                .Cast<DataGridCellsPanel>().ToList();

            foreach (var rowPanel in rowPanelList)
            {
                var viewModel = (PortForwardingMappingViewModel)rowPanel.DataContext;
                if (viewModel == null) break;
                viewModel.tbx_mappingList_srcIpAddr = (TextBox)rowPanel.FindName(nameof(viewModel.tbx_mappingList_srcIpAddr));
                viewModel.tbx_mappingList_srcPort = (TextBox)rowPanel.FindName("tbx_mappingList_srcPort");
                viewModel.tbx_mappingList_destIpAddr = (TextBox)rowPanel.FindName("tbx_mappingList_destIpAddr");
                viewModel.tbx_mappingList_destPort = (TextBox)rowPanel.FindName("tbx_mappingList_destPort");
                viewModel.btn_mappingList_switchEditStatus = (Button)rowPanel.FindName("btn_mappingList_switchEditStatus");
                viewModel.btn_mappingList_removeItem = (Button)rowPanel.FindName("btn_mappingList_removeItem");
            }
        }

        private void DataContextElementMemberBind(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is FrameworkElement)) return;
            var elem = (FrameworkElement)sender;
            if (string.IsNullOrWhiteSpace(elem.Name)) return;
            if (elem.DataContext == null) return;
            var dataContextType = elem.DataContext.GetType();

            //优先级 属性>字段
            var propInfo = dataContextType.GetProperty(elem.Name, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo != null && propInfo.PropertyType.IsAssignableFrom(elem.GetType()))
            {
                propInfo.SetValue(elem.DataContext, elem);
                return;
            }
            var fieldInfo = dataContextType.GetField(elem.Name, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo != null && fieldInfo.FieldType.IsAssignableFrom(elem.GetType()))
            {
                fieldInfo.SetValue(elem.DataContext, elem);
                return;
            }
        }

        private void btn_mappingList_switchEditStatus_Loaded(object sender, RoutedEventArgs e)
        {
            DataContextElementMemberBind(sender, e);
        }
    }
}
