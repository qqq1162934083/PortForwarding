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
        public PortForwardingMappingManager MappingMgr { get; set; } = new PortForwardingMappingManager();

        public MainWindow()
        {
            InitializeComponent();
            MappingMgr.ConsoleOutput += WriteConsole;
            MappingMgr.MappingListChanged += Load_dataGrid_mappingList;
            MappingMgr.MappingListChanged += Load_dataGrid_configPanel;
            Task.Run(() => MappingMgr.Refresh());
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
                dataGrid_mappingList.ItemsSource = mappingList.Select(x => new PortForwardingMappingViewModel(x)).ToList();
            });
        }

        private void Load_dataGrid_configPanel(List<PortForwardingMappingModel> mappingList)
        {
            Dispatcher.Invoke(() =>
            {
                dataGrid_configPanel.ItemsSource = mappingList.Select(x => new ConfigPanelViewModel(x)).ToList();
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

        //private void btn_new_delete(object sender, RoutedEventArgs e)
        //{
        //    var btn = (Button)sender;
        //    var viewModel = (PortForwardingMappingViewModel)btn.DataContext;
        //    if (viewModel.RowType == RowType.Header)
        //    {
        //        var mappingModel = new PortForwardingMappingModel
        //        {
        //            SrcIpAddr = "0.0.0.0",
        //            SrcPort = 0,
        //            DestIpAddr = "0.0.0.0",
        //            DestPort = 0
        //        };
        //        PortForwardingMappingViewModel newItem;
        //        var itemIndex = 1;
        //        lbx_mappingList.Items.Insert(itemIndex, newItem = new PortForwardingMappingViewModel(mappingModel)
        //        {
        //            Col5 = "完成修改",
        //            Editabled = true,
        //            ReadOnly = false,
        //            MappingModel = null,
        //        });
        //    }
        //    else
        //    {
        //        var mappingModel = viewModel?.MappingModel;
        //        if (mappingModel != null)
        //        {
        //            DeleteMapping(mappingModel.SrcIpAddr, mappingModel.SrcPort);
        //            ReloadMappingList();
        //        }
        //    }
        //}

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
                var configMappings = ReadConfigMappingList(r.FileName);

                //添加配置中的映射
                foreach (var mapping in configMappings)
                {
                    MappingMgr.AddMapping(mapping.SrcIpAddr, mapping.SrcPort, mapping.DestIpAddr, mapping.DestPort);
                }
            });
        }

        private void menuItem_removeConfig_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                //获取当前映射
                var mappings = MappingMgr.MappingList;

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
                    MappingMgr.RemoveMapping(mapping.SrcIpAddr, mapping.SrcPort);
                }

            });
        }

        private void btn_selectConfig2ConfigPanel_Click(object sender, RoutedEventArgs e)
        {
            FileDialogUtils.SelectOpenFile(r => r.Filter = "配置文件|*.cfg", r =>
            {
                var enabledMappingList = MappingMgr.MappingList;
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
                MappingMgr.AddMapping(mapping.SrcIpAddr, mapping.SrcPort, mapping.DestIpAddr, mapping.DestPort);
            }
            else
            {
                MappingMgr.RemoveMapping(mapping.SrcIpAddr, mapping.SrcPort);
            }
        }

        private void btn_menuItemHeaderOption_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var obj = VisualTreeHelper.GetParent(btn);

        }

        private void btn_mappingList_switchEditStatus_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var viewModel = (PortForwardingMappingViewModel)btn.DataContext;
            var index = dataGrid_mappingList.Items.IndexOf(viewModel);
            if (index < 0) throw new Exception("行序号获取失败");
            var dataGridCell = VisualTreeUtils.GetFirstParent<DataGridCell>(btn);
            var dataGridCellList = VisualTreeUtils.GetChildrens<DataGridCell>(VisualTreeUtils.GetParent<FrameworkElement>(dataGridCell));
            //for (int i = 0; i < 4; i++)
            //{
            //    var cell = dataGridCellList[i];
            //    var hasReadOnlyList = VisualTreeUtils.RecursiveFindChildrens<FrameworkElement>(cell, elem =>
            //    {
            //        var elemType = elem.GetType();
            //        var ssp = elemType.GetProperties();
            //        var pp = elemType.GetProperty("ContentEnd");
            //        if (pp != null)
            //        {
            //            Console.WriteLine();
            //        }

            //        return pp != null && pp.PropertyType == typeof(bool) && pp.SetMethod != null;
            //    });
            //}
            new VisualTreePrinter().PrintVisualTree(0, dataGridCellList[0]);
            new VisualTreePrinter().PrintLogicaTree(0, dataGridCellList[0]);
            var ss = VisualTreeUtils.FindChildrens<Border>(dataGridCellList[0]).FirstOrDefault();
            var ss2 = VisualTreeUtils.GetChildrens<FrameworkElement>(ss);
            //dataGrid_mappingList
            viewModel.Editing = !viewModel.Editing;
            //btn.Content = viewModel.ing ? "完成修改" : "开始修改";
            //for (var i = 1; i <= 4; i++)
            //{
            //    var textBox = VisualTreeUtils.FindChildren<TextBox>((FrameworkElement)VisualTreeHelper.GetParent(btn), "tbx_col" + i);
            //    textBox.IsReadOnly = !viewModel.Editabled;
            //}
            //if (!viewModel.Editabled)//点击完成修改时
            //{
            //    var mappingModel = viewModel.MappingModel;
            //    if (mappingModel != null)
            //    {
            //        DeleteMapping(mappingModel.SrcIpAddr, mappingModel.SrcPort);
            //    }
            //    mappingModel = new PortForwardingMappingModel()
            //    {
            //        SrcIpAddr = viewModel.Col1,
            //        SrcPort = int.Parse(viewModel.Col2),
            //        DestIpAddr = viewModel.Col3,
            //        DestPort = int.Parse(viewModel.Col4)
            //    };
            //    NewMapping(mappingModel.SrcIpAddr, mappingModel.SrcPort, mappingModel.DestIpAddr, mappingModel.DestPort);
            //    ReloadMappingList();
            //}
        }

        private void btn_mappingList_refresh_Click(object sender, RoutedEventArgs e)
        {
            MappingMgr.Refresh();
        }
    }
    public class VisualTreePrinter
    {
        public void PrintLogicaTree(int depth, object obj)  //输出逻辑树
        {
            test1(new string(' ', depth) + obj);
            if (!(obj is DependencyObject))
            {
                return;
            }
            foreach (object child in LogicalTreeHelper.GetChildren(obj as DependencyObject))
                PrintLogicaTree(depth + 1, child);
        }

        public void PrintVisualTree(int depth, DependencyObject DObj)
        {
            test1(new string(' ', depth) + DObj);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(DObj); i++)
            {
                PrintVisualTree(depth + 1, VisualTreeHelper.GetChild(DObj, i));
            }
        }
        //需先建立文件夹
        public void test1(string a) //切换用户不会停止当前代码
        {
            //写输出信息      
            //StreamWriter sr = new StreamWriter(@"C:\Users\Public\test\a.txt", true, System.Text.Encoding.Default);  // 保留文件原来的内容
            //sr.WriteLine(DateTime.Now.ToString("\r\n" + "HH:mm:ss"));
            //sr.WriteLine(a);
            //sr.Close();
            Console.WriteLine(a);
        }
    }
}
