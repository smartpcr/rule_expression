﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Evaluator_feature_steps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using LightBDD.Framework;
    using LightBDD.Framework.Parameters;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rules.Expressions;
    using Rules.Expressions.Evaluators;
    using TestModels;

    public partial class Evaluator_feature : FeatureFixture
    {
        private object evaluationContext;
        private IConditionExpression conditionExpression;

        private void An_evaluation_context<T>(string cityName)
        {
            evaluationContext = new JsonFixtureFile($"{cityName}.json").JObjectOf<T>();
            StepExecution.Current.Comment($"Current context \n{typeof(T).Name}:\n{evaluationContext.FormatObject()}\n");
        }

        private void I_evaluate_context_with_condition(string left, Operator op, string right)
        {
            conditionExpression = new LeafExpression()
            {
                Left = left,
                Operator = op,
                Right = right
            };
        }

        private void I_evaluate_context_with_filter(IConditionExpression filter)
        {
            conditionExpression = filter;
            StepExecution.Current.Comment($"Current filter:\n{conditionExpression.FormatObject()}\n");
        }

        private void Evaluation_results_should_be(Verifiable<bool> expected)
        {
            var builder = new ExpressionEvaluator();
            bool actual = false;
            if (evaluationContext is Location location)
            {
                var lambda = builder.Evaluate<Location>(conditionExpression);
                actual = lambda(location);
            }
            else if (evaluationContext is Person person)
            {
                var lambda = builder.Evaluate<Person>(conditionExpression);
                actual = lambda(person);
            }
            else
            {
                Assert.Fail($"context type '{evaluationContext.GetType().Name}' is not supported");
            }

            expected.SetActual(actual);
        }
    }
}