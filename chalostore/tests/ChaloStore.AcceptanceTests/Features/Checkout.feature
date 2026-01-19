Feature: Checkout API

  In order to receive my purchase
  As a ChaloStore customer
  I want to confirm the checkout through the public API

  Scenario: Confirming checkout with available stock
    Given the catalog has product 1 with sku "SKU-001" and stock 3
    When the customer places an order for product 1 with email "customer@test.com"
    Then the API should respond with status 200
    And the stored order for product 1 should exist
    And the product 1 should have stock 2
    And an order confirmation email should be sent to "customer@test.com"
    And an order created event should be published for product 1

  Scenario: Rejecting checkout when product is unavailable
    Given the catalog has product 2 with sku "SKU-002" and stock 0
    When the customer places an order for product 2 with email "customer@test.com"
    Then the API should respond with status 400
    And the response body should be "Product not available"

  Scenario: Rejecting checkout when product does not exist
    When the customer places an order for product 42 with email "customer@test.com"
    Then the API should respond with status 400
    And the response body should be "Product not available"

  Scenario: Payment provider rejects the charge
    Given the catalog has product 3 with sku "SKU-003" and stock 1
    And the payment provider will reject requests
    When the customer places an order for product 3 with email "customer@test.com"
    Then the API should respond with status 503
    And the response body should be "Payment rejected"

  @ui
  Scenario: Completing checkout from the web UI
    Given the catalog has product 4 with sku "SKU-UI-01" and stock 1
    When the shopper completes the checkout form for product 4 with email "web@test.com"
    Then the UI should show "Pedido creado"
