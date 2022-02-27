Feature: Calculator
![Calculator](https://specflow.org/wp-content/uploads/2020/09/calculator.png)
Simple calculator for adding **two** numbers

Link to a feature: [Calculator]($projectname$/Features/Calculator.feature)
***Further read***: **[Learn more about how to generate Living Documentation](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/LivingDocGenerator/Generating-Documentation.html)**

@add
Scenario: Add two numbers #1
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	Then the result should be 120
	
@add
Scenario: Add two numbers #2
	Given the first number is 60
	And the second number is 80
	When the two numbers are added
	Then the result should be 140
	
@subtract
Scenario: Subtract two numbers #1
	Given the first number is 80
	And the second number is 20
	When the two numbers are subtracted
	Then the result should be 60