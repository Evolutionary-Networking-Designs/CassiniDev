using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CassiniDev.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CassiniDev.FixtureExamples.VSTS
{
    public class TestAppFixture
    {
        private static Fixture _fixture;
        protected static ushort GetPort(ushort portRangeStart,ushort portRangeEnd, IPAddress ipAddress)
        {
            return Fixture.GetPort(portRangeStart, portRangeEnd, ipAddress);
        }

        protected Uri NormalizeUri(string relativeUrl)
        {
            return _fixture.NormalizeUri(relativeUrl);
        }


        protected static void InitializeFixture()
        {
            _fixture = new Fixture();
            const string path = @"..\..\..\..\..\..\CassiniDev.FixtureExamples.TestWeb\";
            _fixture.StartServer(path, IPAddress.Loopback, GetPort(8080, 9000, IPAddress.Loopback), "/", "localhost", false, 25000, 12000);
        }

        protected static void CleanupFixture()
        {
            _fixture.StopServer();
        }

    }
}
