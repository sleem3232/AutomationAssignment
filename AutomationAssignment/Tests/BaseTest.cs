using AutomationAssignment.Utils;
using Microsoft.Playwright;
using NUnit.Framework;

namespace AutomationAssignment.Tests
{
    public class BaseTest
    {
        protected IPlaywright Playwright;
        protected IBrowser Browser;
        protected IBrowserContext Context;
        protected IPage page;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            HtmlReportManager.Init();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            HtmlReportManager.FinalizeReport();
        }

        [SetUp]
        public async Task SetUp()
        {
            ReportContext.Start();

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                Channel = "chrome"
            });

            Context = await Browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width = 1920,
                    Height = 1080
                }
            });

            page = await Context.NewPageAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            var result = TestContext.CurrentContext.Result;

            var details = ReportContext.GetDetails();

            HtmlReportManager.AddResult(
                testName: TestContext.CurrentContext.Test.Name,
                status: result.Outcome.Status.ToString(),
                details: string.IsNullOrWhiteSpace(details)
                    ? "No details were captured."
                    : details + (!string.IsNullOrWhiteSpace(result.Message)
                        ? $"\nERROR: {result.Message}"
                        : string.Empty),
                url: ReportContext.GetUrl()
            );

            ReportContext.Clear();

            if (Context != null)
                await Context.CloseAsync();

            if (Browser != null)
                await Browser.CloseAsync();

            Playwright?.Dispose();
        }
    }
}