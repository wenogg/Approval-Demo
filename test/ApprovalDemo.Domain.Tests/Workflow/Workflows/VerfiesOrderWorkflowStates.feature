Feature: VerifiesOrderWorkflowStates
	Tests the order workflow process

Scenario: Start the workflow
	Given order
	When I take the action of (Submit Order)
	Then the order status should be (Preparing)
	And the next actions are (Mark Prepared,Return for correction)

Scenario: Cancel the order
	Given order
	When I take the action of (Cancel Order)
	Then the order status should be (Cancelled)
	And the workflow is complete