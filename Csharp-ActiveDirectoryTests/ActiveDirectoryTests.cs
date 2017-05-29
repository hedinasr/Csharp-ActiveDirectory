using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csharp_ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp_ActiveDirectory.Tests
{
    [TestClass()]
    public class ActiveDirectoryTests
    {
        [TestMethod()]
        public void ActiveDirectoryTest()
        {

        }

        [TestMethod()]
        public void ActiveDirectoryTest1()
        {
            ActiveDirectory activeDirectory = new ActiveDirectory("192.168.31.134", "Administrateur", "SRIVéà&è");
        }

        [TestMethod()]
        public void IsUserExistingTest()
        {

        }

        [TestMethod()]
        public void IsGroupExistingTest()
        {

        }

        [TestMethod()]
        public void GetUserTest()
        {

        }

        [TestMethod()]
        public void GetGroupTest()
        {

        }

        [TestMethod()]
        public void CreateNewGroupTest()
        {

        }
    }
}