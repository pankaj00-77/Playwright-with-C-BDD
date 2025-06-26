Feature: Complete Ecommerce Flow

  Scenario: Valid user completes order successfully
    Given I navigate to the login page
    When I login with valid credentials
    And I select a product and add it to the cart
    And I place the order with valid information
    Then I should see a successful purchase confirmation
