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
        /// <summary>
        /// 通过bat的方式运行cmd获取cmd的结果
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        /// <exception cref="CmdProcessException"></exception>
        public static string RunCmd(string cmd)
        {
            var nowTime = DateTime.Now;
            var showDialog = false;//是否显示窗口运行
            var batPath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, Guid.NewGuid().ToString("N") + ".bat");
            var batContentBuilder = new StringBuilder();
            if (!showDialog) batContentBuilder.AppendLine("@echo off");
            batContentBuilder.Append(cmd);

            try
            {
                using (var writer = File.CreateText(batPath))
                {
                    var batContent = batContentBuilder.ToString();
                    writer.Write(batContent);
                }
                var result = (string)null;

                using (var cmdProcess = new Process())
                {
                    var redirectInput = true;
                    cmdProcess.StartInfo.FileName = batPath;//"cmd.exe";//设定程序名
                    cmdProcess.StartInfo.UseShellExecute = false; //关闭Shell的使用
                    cmdProcess.StartInfo.RedirectStandardInput = redirectInput;//重定向标准输入
                    cmdProcess.StartInfo.RedirectStandardOutput = !showDialog;//重定向标准输出
                    cmdProcess.StartInfo.RedirectStandardError = !showDialog;//重定向错误输出
                    cmdProcess.StartInfo.CreateNoWindow = !showDialog;//设置不显示窗口
                    cmdProcess.Start();
                    if (!showDialog)
                    {
                        var successOutputStr = cmdProcess.StandardOutput.ReadToEnd();
                        var errorOutputStr = cmdProcess.StandardError.ReadToEnd();
                        if (errorOutputStr.Length > 0) throw new CmdProcessException(errorOutputStr);
                        result = successOutputStr;
                    }
                    cmdProcess.WaitForExit();//等待程序执行完退出进程   
                    cmdProcess.Close();//结束
                }

                return result;
            }
            finally
            {
                if (File.Exists(batPath))
                    File.Delete(batPath);
                    //FileSystem.DeleteFile(batPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string GetCmdResult(string cmd)
        {
            try
            {
                return RunCmd(cmd);
            }
            catch (CmdProcessException exp)
            {
                return exp.Message;
            }
        }
    }
    public class CmdProcessException : Exception
    {
        public CmdProcessException(string message) : base(message)
        {
        }
    }
}
