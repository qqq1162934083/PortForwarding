using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyTool
{
    public class ConsoleUtils
    {
        public static string GetCmdResult(string cmd)
        {
            var guid = Guid.NewGuid().ToString("N");
            var batPath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, guid + ".bat");
            var batContentBuilder = new StringBuilder("@echo off\r\n");
            batContentBuilder.Append(cmd);
            using (var writer = File.CreateText(batPath))
            {
                var batContent = batContentBuilder.ToString();
                writer.Write(batContent);
            }
            var nowTime = DateTime.Now;
            var result = (string)null;
            using (var cmdProcess = new Process())
            {
                var backgrounder = true;
                var redirectInput = true;
                cmdProcess.StartInfo.FileName = batPath;//"cmd.exe";//设定程序名
                cmdProcess.StartInfo.UseShellExecute = false; //关闭Shell的使用
                cmdProcess.StartInfo.RedirectStandardInput = redirectInput;//重定向标准输入
                cmdProcess.StartInfo.RedirectStandardOutput = backgrounder;//重定向标准输出
                cmdProcess.StartInfo.RedirectStandardError = backgrounder;//重定向错误输出
                cmdProcess.StartInfo.CreateNoWindow = backgrounder;//设置不显示窗口
                cmdProcess.Start();
                if (backgrounder)
                {
                    result = cmdProcess.StandardOutput.ReadToEnd();
                    result += cmdProcess.StandardError.ReadToEnd();
                }
                cmdProcess.WaitForExit();//等待程序执行完退出进程   
                cmdProcess.Close();//结束
            }
            FileSystem.DeleteFile(batPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            return result;
        }
    }
}
