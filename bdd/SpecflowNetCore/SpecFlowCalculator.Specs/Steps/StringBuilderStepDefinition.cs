using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Xunit;

namespace SpecFlowCalculator.Specs.Steps
{
    [Binding]
    public sealed class StringBuilderStepDefinition
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;
        
        private readonly StringBuilder _builder = new StringBuilder();
        
        private string _result;

        public StringBuilderStepDefinition(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }
        
        [Given("null string is provided")]
        public void GivenTheStringIsNull()
        {
            _builder.Value = null;
        }

        [Given("string (.*) is provided")]
        public void GivenTheStringIs(string value)
        {
            _builder.Value = value;
        }

        [When("the string is reversed")]
        public void WhenStringIsReversed()
        {
            _result = _builder.Reverse();
        }
        
        [Then("null is returned")]
        public void ThenTheResultShouldBeNull()
        {
            Assert.Null(_result);
        }

        [Then("the resulting string should be (.*)")]
        public void ThenTheResultShouldBe(string result)
        {
            Assert.Equal(result, _result);
        }
    }
}