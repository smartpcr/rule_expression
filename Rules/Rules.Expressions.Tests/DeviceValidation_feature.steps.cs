﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidation_feature_steps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Rules;
    using LightBDD.Framework;
    using LightBDD.Framework.Parameters;
    using LightBDD.MsTest2;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Rules.Expressions.Eval;
    using Rules.Expressions.Evaluators;
    using Rules.Expressions.Helpers;
    using Rules.Expressions.Parsers;
    using Rules.Expressions.Tests.TestData;

    public partial class DeviceValidation_feature : FeatureFixture
    {
        private PowerDevice evaluationContext;
        private IConditionExpression whenCondition;
        private Func<PowerDevice, bool> filter;
        private IConditionExpression ifCondition;
        private Func<PowerDevice, bool> assert;

        private void A_device(string testFileName)
        {
            evaluationContext = new JsonFixtureFile($"{testFileName}.json").JObjectOf<PowerDevice>();
            var random = new Random();
            foreach (var reading in evaluationContext.LastReadings)
            {
                if (reading.DataPoint.Contains("Pwr.kW tot", StringComparison.OrdinalIgnoreCase))
                {
                    var minutesAgo = random.Next(25);
                    reading.EventTime = DateTime.UtcNow.AddMinutes(0 - minutesAgo);
                }
            }
        }

        private void A_filter_condition(IConditionExpression filterCondition)
        {
            whenCondition = filterCondition;
            IExpressionBuilder builder = new ExpressionBuilder();
            filter = builder.Build<PowerDevice>(whenCondition);
        }

        private void I_use_json_rule(string jsonRuleFile)
        {
            var parser = new ExpressionParser();
            IExpressionBuilder builder = new ExpressionBuilder();
            var rule = new JsonFixtureFile($"{jsonRuleFile}.json").JObjectOf<ValidationRule>();
            var whenExpression = JObject.Parse(rule.WhenExpression);
            StepExecution.Current.Comment($"Current filter:\n{whenExpression.FormatObject()}\n");
            whenCondition = parser.Parse(whenExpression);
            filter = builder.Build<PowerDevice>(whenCondition);

            var ifExpression = JObject.Parse(rule.IfExpression);
            StepExecution.Current.Comment($"Current assert:\n{ifExpression.FormatObject()}\n");
            ifCondition = parser.Parse(ifExpression);
            assert = builder.Build<PowerDevice>(ifCondition);
        }

        private void Context_should_pass_filter(Verifiable<bool> expected)
        {
            var evidence = GetEvidence(whenCondition, evaluationContext);
            if (evidence != null)
            {
                StepExecution.Current.Comment($"Evidence\n{evidence.FormatObject()}\n");
            }

            var actual = filter(evaluationContext);
            expected.SetActual(actual);
        }

        private void Filter_results_should_be(Verifiable<bool> expected)
        {
            var evidence = GetEvidence(whenCondition, evaluationContext);
            if (evidence != null)
            {
                StepExecution.Current.Comment($"Evidence\n{evidence.FormatObject()}\n");
            }

            var actual = filter(evaluationContext);
            expected.SetActual(actual);
        }

        private void Assert_results_should_be(Verifiable<bool> expected)
        {
            var evidence = GetEvidence(ifCondition, evaluationContext);
            if (evidence != null)
            {
                StepExecution.Current.Comment($"Evidence\n{evidence.FormatObject()}\n");
            }

            var actual = assert(evaluationContext);
            expected.SetActual(actual);
        }

        private JArray GetEvidence<T>(IConditionExpression condition, T instance)
        {
            var leafEvaluators = new List<LeafExpression>();
            condition.PopulateLeafFieldEvaluators(leafEvaluators);
            var array = new JArray();
            foreach(var leafExpr in leafEvaluators)
            {
                var ctxParameter = Expression.Parameter(typeof(T), "ctx");
                var leftExpression = ctxParameter.BuildExpression(leafExpr.Left);
                var lambda = Expression.Lambda(leftExpression, ctxParameter);
                var getValue = lambda.Compile();
                var actualObj = getValue.DynamicInvoke(instance);

                string expected = leafExpr.Right;
                if (leafExpr.RightSideIsExpression)
                {
                    var rightExpression = ctxParameter.BuildExpression(leafExpr.Right);
                    lambda = Expression.Lambda(rightExpression, ctxParameter);
                    getValue = lambda.Compile();
                    var expectedObj = getValue.DynamicInvoke(instance);
                    expected = JsonConvert.SerializeObject(expectedObj);
                }

                var evidence = new
                {
                    left = leafExpr.Left,
                    op = leafExpr.Operator.ToString(),
                    actual = actualObj,
                    expected
                };
                array.Add(JToken.FromObject(evidence));
            }

            return array;
        }
    }
}