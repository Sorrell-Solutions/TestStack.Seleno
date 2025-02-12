﻿using System;
using System.Diagnostics;
using System.Threading;
using Castle.Core.Internal;
using FluentAssertions;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TestStack.Seleno.Configuration;

namespace TestStack.Seleno.AcceptanceTests.Configuration
{
    [TestFixture]
    class ProcessLifecycleTests
    {
        private const string Chrome = "chromedriver";
        private const string IE = "IEDriverServer";
        private const string Firefox = "firefox";
        private const string IisExpress = "iisexpress";

        [TestCase(Chrome)]
        [TestCase(IE)]
        [TestCase(Firefox)]
        public void Closing_SelenoHost_should_close_child_browser(string driverName)
        {
			// ARRANGE
            Process.GetProcessesByName(driverName).ForEach(StopProcess);
            var selenoHost = new SelenoHost();
            Func<RemoteWebDriver> driver = GetBrowserFactory(driverName);
            selenoHost.Run("TestStack.Seleno.AcceptanceTests.Web", 12346,
                c => c.WithRemoteWebDriver(driver));
			// Somehow, Firefox spawns 6 processes
			const int NumberOfFirefoxProcesses = 6;
			int NumberOfExpectedProcesses;

			switch(driverName)
			{
				case Firefox:
					NumberOfExpectedProcesses = NumberOfFirefoxProcesses;
					break;
				default:
					NumberOfExpectedProcesses = 1;
					break;
			}

			Process.GetProcessesByName(driverName).Length.Should().Be(NumberOfExpectedProcesses);

			// ACT
            selenoHost.Dispose();

			// ASSERT
            Process.GetProcessesByName(driverName).Should().BeEmpty();
        }

        [Test]
        public void Closing_SelenoHost_should_close_Iis_Express()
        {
            PatientlyStopProcess(IisExpress);
            Process.GetProcessesByName("chromedriver").ForEach(StopProcess);

            var selenoHost = new SelenoHost();
            selenoHost.Run("TestStack.Seleno.AcceptanceTests.Web", 12346,
                c => c.WithRemoteWebDriver(BrowserFactory.Chrome));
            Process.GetProcessesByName(IisExpress).Length.Should().Be(1);

            selenoHost.Dispose();

            Process.GetProcessesByName("chromedriver").Should().BeEmpty();
            Process.GetProcessesByName(IisExpress).Should().BeEmpty();
        }

        private void PatientlyStopProcess(string processName)
        {
            for (int i = 0; i < 5; i++)
            {
                Process.GetProcessesByName(processName).ForEach(StopProcess);
                if (Process.GetProcessesByName(processName).Length > 0)
                {
                    Thread.Sleep(5000);
                }
            }

        }
        private void StopProcess(Process process)
        {
            if (process == null)
                return;
            if (!process.HasExited)
                process.Kill();
            process.Dispose();
        }

        private Func<RemoteWebDriver> GetBrowserFactory(string browser)
        {
            switch (browser)
            {
                case Chrome:
                    return BrowserFactory.Chrome;
				case IE:
					return BrowserFactory.InternetExplorer;
				case Firefox:
                    return BrowserFactory.FireFox;
            }
            return null;
        }
    }
}
