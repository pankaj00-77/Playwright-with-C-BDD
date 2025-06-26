using Microsoft.Playwright;
using TechTalk.SpecFlow; //BDD bindings and hooks.
using System.IO;

namespace PlaywrightEcommerce.Hooks
{
    [Binding]
    public sealed class TestHooks
    {
        private static IPlaywright? _playwright;
        private static IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;

        private readonly ScenarioContext _scenarioContext;

        public TestHooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun]
        public static async Task BeforeTestRun()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false });
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            _context = await _browser!.NewContextAsync();
            _page = await _context.NewPageAsync();
            _scenarioContext["page"] = _page;
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            var testName = _scenarioContext.ScenarioInfo.Title;
            if (_scenarioContext.TestError != null && _page != null)
            {
                var screenshotPath = $"Reports/Screenshots/{testName}_failed.png";
                await _page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
            }
            if (_page != null) await _page.CloseAsync();
            if (_context != null) await _context.CloseAsync();
        }

        [AfterTestRun]
        public static async Task AfterTestRun()
        {
            if (_browser != null) await _browser.CloseAsync();
            _playwright?.Dispose();
        }
    }
}