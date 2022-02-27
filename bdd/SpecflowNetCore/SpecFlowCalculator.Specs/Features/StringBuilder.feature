Feature: StringBuilder
	Simple string manipulator

@reverse
Scenario: Reverse string
	Given string hello is provided
	When the string is reversed
	Then the resulting string should be olleh
	
@reverse
Scenario: Reverse null string
	Given null string is provided
	When the string is reversed
	Then null is returned