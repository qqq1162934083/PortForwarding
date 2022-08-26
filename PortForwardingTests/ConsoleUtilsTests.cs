using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Tests
{
    [TestClass()]
    public class ConsoleUtilsTests
    {
        [TestMethod()]
        public void GetCmdResultTest()
        {
            //执行失败
            var message = ConsoleUtils.GetCmdResult("java");
            Assert.IsTrue(message.Length > 0);
        }

        [TestMethod()]
        public void RunCmdTest()
        {
            //执行失败
            Assert.ThrowsException<CmdProcessException>(() =>
            {
                ConsoleUtils.RunCmd("java");
            });
        }
    }
}