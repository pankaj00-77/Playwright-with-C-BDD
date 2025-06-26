using Microsoft.Playwright;// used for Playwright functionalities like IPage, WaitForSelectorState, etc.

using TechTalk.SpecFlow; // used for in order to use [Binding] and [Given], [When], [Then] attributes

using PlaywrightEcommerce.PageObjects;// used for page object model classes

using System.Text.Json;// used for JSON serialization

namespace PlaywrightEcommerce.StepDefinitions // Namespace for the step definitions, following the Cucumber BDD pattern
{
    [Binding]
    public class CompleteEcommerceSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private IPage _page = null!;
        private LoginPage _loginPage = null!; 
        private DashboardPage _dashboardPage = null!;
        private AddToCartPage _cartPage = null!;
        private PlaceOrderPage _orderPage = null!;
        private TestData _testData = null!;

        private class TestData
        {
            public string? username { get; set; }
            public string? password { get; set; }
            public string? productName { get; set; }
        }

        public CompleteEcommerceSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"I navigate to the login page")]
        public async Task GivenINavigateToTheLoginPage()
        {
            _page = (IPage)_scenarioContext["page"];

            var jsonPath = Path.Combine(AppContext.BaseDirectory, @"Test Data\Data.json");
            var json = File.ReadAllText(jsonPath);
            _testData = JsonSerializer.Deserialize<TestData>(json)
                ?? throw new Exception("Test data JSON could not be parsed.");

            _loginPage = new LoginPage(_page);
            await _loginPage.NavigatePageAsync();
        }

        [When(@"I login with valid credentials")]
        public async Task WhenILoginWithValidCredentials()
        {
            await _loginPage.LoginAsync(_testData.username!, _testData.password!);
            _dashboardPage = new DashboardPage(_page);
        }

        [When(@"I select a product and add it to the cart")]
        public async Task WhenISelectAProductAndAddItToTheCart()
        {
            await _dashboardPage.SelectProductAsync(_testData.productName!);
            await _dashboardPage.AddToCartAsync();

            // Better alert handling - wait for and accept the alert
            _page.Dialog += async (_, dialog) => 
            {
                Console.WriteLine($"Alert detected: {dialog.Message}");
                await dialog.AcceptAsync();
            };
            
            // Wait for the alert to appear and be handled
            await _page.WaitForTimeoutAsync(2000);

            _cartPage = new AddToCartPage(_page);
            await _cartPage.NavigateToCartAsync();
            
            // Wait for cart table to be visible instead of NetworkIdle
            await _page.WaitForSelectorAsync("#tbodyid", new() { State = WaitForSelectorState.Visible });
            await _page.WaitForTimeoutAsync(1000);

            // Debug: Check what's actually in the cart
            var cartContent = await _page.Locator("#tbodyid").InnerTextAsync();
          

            
        }

        [When(@"I place the order with valid information")]
        public async Task WhenIPlaceTheOrderWithValidInformation()
        {
            _orderPage = new PlaceOrderPage(_page);
            await _orderPage.ClickPlaceOrderAsync();

            await _page.WaitForSelectorAsync("#orderModal", new() { State = WaitForSelectorState.Visible });

            await _orderPage.FillOrderFormAndSubmitAsync(
                name: _testData.username!,
                country: "India",
                city: "Delhi",
                card: "123412341234",
                month: "06",
                year: "2025"
            );
        }

        [Then(@"I should see a successful purchase confirmation")]
        public async Task ThenIShouldSeeASuccessfulPurchaseConfirmation()
        {
            bool isSuccess = await _orderPage.IsOrderSuccessfulAsync();
            Assert.That(isSuccess, "Order confirmation modal did not appear.");
            await _orderPage.ConfirmSuccessAsync();
        }

        
    }
}
